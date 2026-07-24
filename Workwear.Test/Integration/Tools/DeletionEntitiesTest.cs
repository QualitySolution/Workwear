using System;
using System.Linq;
using System.Threading;
using NSubstitute;
using NUnit.Framework;
using QS.Deletion;
using QS.Dialog;
using QS.Navigation;
using QS.Testing.DB;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Tools;

namespace Workwear.Test.Integration.Tools
{
	[TestFixture(Description = "Различные тесты на реальное удаление из базы различных сущностей.")]
	[Category("Integrated")]
	public class DeletionEntitiesTest : InMemoryDBGlobalConfigTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init()
		{
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
			ConfigureOneTime.ConfigureDeletion();
		}

		[Test(Description = "Проверяем что можем удалить созданный документ выдачи.")]
		[Category("Integrated")]
		public void Deletion_ExpenseEmployeeDocumentTest()
		{
			NewSessionWithSameDB();
			var ask = Substitute.For<IInteractiveService>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);
			var navigation = Substitute.For<INavigationManager>();

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var nomenclatureType = new ItemsType {
					Name = "Тестовый тип номенклатуры"
				};
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature {
					Type = nomenclatureType
				};
				uow.Save(nomenclature);

				var position1 = new StockPosition(nomenclature, 0, null, null, null);

				var nomenclature2 = new Nomenclature {
					Type = nomenclatureType
				};
				uow.Save(nomenclature2);

				var position2 = new StockPosition(nomenclature2, 0, null, null, null);

				var protectionTools = new ProtectionTools {
					Name = "СИЗ для тестирования"
				};
				protectionTools.AddNomenclature(nomenclature);
				uow.Save(protectionTools);

				var protectionTools2 = new ProtectionTools {
					Name = "СИЗ для тестирования 2"
				};
				protectionTools2.AddNomenclature(nomenclature2);
				uow.Save(protectionTools2);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Year;
				normItem.PeriodCount = 1;
				var normItem2 = norm.AddItem(protectionTools2);
				normItem2.Amount = 1;
				normItem2.NormPeriod = NormPeriodType.Month;
				normItem2.PeriodCount = 1;
				uow.Save(norm);

				var employee = new EmployeeCard();
				employee.AddUsedNorm(norm);
				uow.Save(employee);
				uow.Commit();

				var income = new Income {
					Warehouse = warehouse,
					Date = new DateTime(2017, 1, 1)
				};
				var incomeItem1 = income.AddItem(nomenclature, ask);
				incomeItem1.Amount = 10;
				var incomeItem2 = income.AddItem(nomenclature2, ask);
				incomeItem2.Amount = 5;
				income.UpdateOperations(uow);
				uow.Save(income);

				var expense = new Expense {
					Warehouse = warehouse,
					Employee = employee,
					Date = new DateTime(2018, 10, 22),
					IssueDate = new DateTime(2018, 10, 22),
				};
				var expenseItem1 = expense.AddItem(position1, 1);
				expenseItem1.EmployeeCardItem = employee.WorkwearItems.FirstOrDefault(x => x.ProtectionTools == protectionTools);
				expenseItem1.ProtectionTools = protectionTools;
				var expenseItem2 = expense.AddItem(position2, 1);
				expenseItem2.EmployeeCardItem = employee.WorkwearItems.FirstOrDefault(x => x.ProtectionTools == protectionTools2);
				expenseItem2.ProtectionTools = protectionTools2;

				var baseParameters = Substitute.For<BaseParameters>();
				baseParameters.ColDayAheadOfShedule.Returns(0);

				expense.CreateIssuanceSheet(null, null, null);

				//Обновление операций
				expense.UpdateOperations(uow, baseParameters, ask);
				uow.Save(expense);
				uow.Save(expense.IssuanceSheet);
				uow.Commit();

				expense.UpdateEmployeeWearItems();
				uow.Commit();

				var docs = uow.GetAll<Expense>().ToList();
				Assert.That(docs.Count, Is.EqualTo(1));

				//Непосредственно удаление документа
				var cancel = new CancellationTokenSource();
				using(var uowDel = UnitOfWorkFactory.CreateWithoutRoot()) {
					var deletionService = new DeleteCore(DeleteConfig.Main, uowDel);
					deletionService.PrepareDeletion(typeof(Expense), expense.Id, cancel.Token);
					Assert.That(deletionService.TotalLinks, Is.GreaterThan(0));
					deletionService.RunDeletion(cancel.Token);
					uowDel.Commit();
				}

				//Проверяем удаление
				var expenseId = expense.Id;
				using(var uowCheck = UnitOfWorkFactory.CreateWithoutRoot()) {
					//Проверяем что удалили документ.
					docs = uow.GetAll<Expense>().ToList();
					Assert.That(docs.Count, Is.Zero);

					//Проверяем что случайно не удалили СИЗ и номенклатуру.
					var protections = uow.GetAll<ProtectionTools>().ToList();
					Assert.That(protections.Count, Is.EqualTo(2));
					var nomenclatures = uow.GetAll<Nomenclature>().ToList();
					Assert.That(nomenclatures.Count, Is.EqualTo(2));
				}
			}
		}

		[Test(Description = "Проверяем удаление документа возврата выдачи вне нормы с маркировкой.")]
		[Category("Integrated")]
		public void Deletion_OverNormReturnWithBarcode_DoesNotDeleteSourceIssueTest()
		{
			NewSessionWithSameDB();
			var cancel = new CancellationTokenSource();
			int overNormDocumentId;
			int sourceOperationId;
			int sourceWarehouseOperationId;
			int returningBarcodeId;
			int notReturningBarcodeId;
			int returnDocumentId;
			int returnOperationId;
			int returnWarehouseOperationId;

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse { Name = "Тестовый склад" };
				uow.Save(warehouse);

				var nomenclatureType = new ItemsType {
					Name = "Тестовый тип номенклатуры"
				};
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature {
					Name = "Тестовая куртка",
					Type = nomenclatureType,
					UseBarcode = true
				};
				uow.Save(nomenclature);

				var employee = new EmployeeCard {
					FirstName = "Иван",
					LastName = "Иванов"
				};
				uow.Save(employee);

				var returningBarcode = new Barcode {
					Title = "000000000001",
					Nomenclature = nomenclature
				};
				var notReturningBarcode = new Barcode {
					Title = "000000000002",
					Nomenclature = nomenclature
				};
				uow.Save(returningBarcode);
				uow.Save(notReturningBarcode);

				var sourceWarehouseOperation = new WarehouseOperation {
					ExpenseWarehouse = warehouse,
					OperationTime = new DateTime(2026, 7, 1),
					Amount = 2,
					Nomenclature = nomenclature
				};
				uow.Save(sourceWarehouseOperation);
				var sourceOperation = new OverNormOperation {
					Type = OverNormType.Guest,
					Employee = employee,
					Nomenclature = nomenclature,
					OperationTime = sourceWarehouseOperation.OperationTime,
					WarehouseOperation = sourceWarehouseOperation
				};
				var sourceBarcodeOperation1 = new BarcodeOperation {
					Barcode = returningBarcode,
					OverNormOperation = sourceOperation,
					WarehouseOperation = sourceWarehouseOperation
				};
				var sourceBarcodeOperation2 = new BarcodeOperation {
					Barcode = notReturningBarcode,
					OverNormOperation = sourceOperation,
					WarehouseOperation = sourceWarehouseOperation
				};
				sourceOperation.BarcodeOperations.Add(sourceBarcodeOperation1);
				sourceOperation.BarcodeOperations.Add(sourceBarcodeOperation2);
				returningBarcode.BarcodeOperations.Add(sourceBarcodeOperation1);
				notReturningBarcode.BarcodeOperations.Add(sourceBarcodeOperation2);

				var overNorm = new OverNorm {
					Warehouse = warehouse,
					Date = sourceWarehouseOperation.OperationTime,
					Type = OverNormType.Guest
				};
				overNorm.AddItem(sourceOperation);
				uow.Save(overNorm);
				uow.Commit();

				var returnDocument = new Return {
					Warehouse = warehouse,
					Date = new DateTime(2026, 7, 5)
				};
				var returnItem = returnDocument.AddItem(sourceOperation, 1, barcodes: new[] { returningBarcode });
				returnDocument.UpdateOperations(uow);
				uow.Save(returnDocument);
				uow.Commit();

				overNormDocumentId = overNorm.Id;
				sourceOperationId = sourceOperation.Id;
				sourceWarehouseOperationId = sourceWarehouseOperation.Id;
				returningBarcodeId = returningBarcode.Id;
				notReturningBarcodeId = notReturningBarcode.Id;
				returnDocumentId = returnDocument.Id;
				returnOperationId = returnItem.ReturnFromOverNormOperation.Id;
				returnWarehouseOperationId = returnItem.WarehouseOperation.Id;

				using(var uowDel = UnitOfWorkFactory.CreateWithoutRoot()) {
					var deletionService = new DeleteCore(DeleteConfig.Main, uowDel);
					deletionService.PrepareDeletion(typeof(Return), returnDocumentId, cancel.Token);
					deletionService.RunDeletion(cancel.Token);
					uowDel.Commit();
				}

				using(var uowCheck = UnitOfWorkFactory.CreateWithoutRoot()) {
					Assert.That(uowCheck.GetById<Return>(returnDocumentId), Is.Null);
					Assert.That(uowCheck.GetById<OverNormOperation>(returnOperationId), Is.Null);
					Assert.That(uowCheck.GetById<WarehouseOperation>(returnWarehouseOperationId), Is.Null);

					var sourceDocument = uowCheck.GetById<OverNorm>(overNormDocumentId);
					var sourceOperationCheck = uowCheck.GetById<OverNormOperation>(sourceOperationId);
					var sourceWarehouseOperationCheck = uowCheck.GetById<WarehouseOperation>(sourceWarehouseOperationId);
					var returningBarcodeCheck = uowCheck.GetById<Barcode>(returningBarcodeId);
					var notReturningBarcodeCheck = uowCheck.GetById<Barcode>(notReturningBarcodeId);

					Assert.That(sourceDocument, Is.Not.Null);
					Assert.That(sourceOperationCheck, Is.Not.Null);
					Assert.That(sourceWarehouseOperationCheck, Is.Not.Null);
					Assert.That(sourceOperationCheck.WarehouseOperation.Id, Is.EqualTo(sourceWarehouseOperationId));
					Assert.That(sourceOperationCheck.BarcodeOperations.Select(x => x.Barcode.Id), Is.EquivalentTo(new[] {
						returningBarcodeId,
						notReturningBarcodeId
					}));
					Assert.That(returningBarcodeCheck.BarcodeOperations.Single().OverNormOperation.Id, Is.EqualTo(sourceOperationId));
					Assert.That(notReturningBarcodeCheck.BarcodeOperations.Single().OverNormOperation.Id, Is.EqualTo(sourceOperationId));
				}
			}
		}
	}
}
