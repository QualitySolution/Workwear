using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Testing.DB;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Repository.Operations;
using Workwear.Tools;

namespace Workwear.Test.Integration.Stock
{
	[TestFixture(TestOf = typeof(Writeoff), Description = "Списание с сотрудника")]
	public class WriteoffEmployeeIntegratedTest : InMemoryDBGlobalConfigTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init()
		{
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
		}

		[Test(Description = "Проверяем что процесс списания в целом работает")]
		[Category("Integrated")]
		public void WriteoffMainTest()
		{
			var ask = Substitute.For<IInteractiveService>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);
			var baseParameters = Substitute.For<BaseParameters>();

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

				var size = new Size();
				var height = new Size();
				uow.Save(size);
				uow.Save(height);

				var position1 = new StockPosition(nomenclature, 0, size, height, null);

				var protectionTools = new ProtectionTools {
					Name = "СИЗ для тестирования"
				};
				protectionTools.AddNomenclature(nomenclature);
				uow.Save(protectionTools);

				var employee = new EmployeeCard();
				uow.Save(employee);
				uow.Commit();

				var income = new Income {
					Warehouse = warehouse,
					Date = new DateTime(2017, 1, 1)
				};
				var incomeItem1 = income.AddItem(nomenclature, ask);
				incomeItem1.Amount = 10;
				income.UpdateOperations(uow);
				uow.Save(income);

				var expense = new Expense {
					Warehouse = warehouse,
					Employee = employee,
					Date = new DateTime(2018, 10, 22),
					IssueDate = new DateTime(2018, 10, 22)
				};
				var item = expense.AddItem(position1, 3);

				//Обновление операций
				expense.UpdateOperations(uow, baseParameters, ask);
				expense.SaveOperations(uow);
				uow.Save(expense);
				uow.Commit();

				var employeeRepository = new EmployeeIssueRepository(uow);
				var balance = employeeRepository.ItemsBalance(employee, new DateTime(2018, 10, 30));
				Assert.That(balance.First().Amount, Is.EqualTo(3));

				//Списываем
				var writeoff = new Writeoff {
					Date = new DateTime(2018, 10, 25)
				};
				writeoff.AddItem(item.EmployeeIssueOperation, 1);

				//Обновление операций
				writeoff.UpdateOperations(uow);
				uow.Save(writeoff);
				uow.Commit();

				var balanceAfter = employeeRepository.ItemsBalance(employee, new DateTime(2018, 10, 30));
				Assert.That(balanceAfter.First().Amount, Is.EqualTo(2));

			}
		}

		[Test(Description = "Списание выдачи с 2 промаркированными единицами через реальную БД (не in-memory доменные объекты) " +
			"должно давать 2 доступных для списания штрихкода и создавать 2 отдельные строки документа, по одному коду в каждой.")]
		[Category("Integrated")]
		public void WriteoffEmployee_TwoBarcodedUnits_BothAvailableAndSplitIntoTwoItems()
		{
			NewSessionWithSameDB();
			var ask = Substitute.For<IInteractiveService>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);
			var baseParameters = Substitute.For<BaseParameters>();

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var nomenclatureType = new ItemsType {
					Name = "Тестовый тип номенклатуры"
				};
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature {
					Type = nomenclatureType,
					UseBarcode = true
				};
				uow.Save(nomenclature);

				var size = new Size();
				var height = new Size();
				uow.Save(size);
				uow.Save(height);

				var position = new StockPosition(nomenclature, 0, size, height, null);

				var protectionTools = new ProtectionTools {
					Name = "СИЗ для тестирования"
				};
				protectionTools.AddNomenclature(nomenclature);
				uow.Save(protectionTools);

				var employee = new EmployeeCard();
				uow.Save(employee);
				uow.Commit();

				var income = new Income {
					Warehouse = warehouse,
					Date = new DateTime(2017, 1, 1)
				};
				var incomeItem = income.AddItem(nomenclature, ask);
				incomeItem.Amount = 2;
				income.UpdateOperations(uow);
				uow.Save(income);

				var expense = new Expense {
					Warehouse = warehouse,
					Employee = employee,
					Date = new DateTime(2018, 10, 22),
					IssueDate = new DateTime(2018, 10, 22)
				};
				var expenseItem = expense.AddItem(position, 2);
				expense.UpdateOperations(uow, baseParameters, ask);
				expense.SaveOperations(uow);
				uow.Save(expense);
				uow.Commit();

				var firstBarcode = new Barcode { Type = BarcodeTypes.EAN13, Title = "2000000000001", Nomenclature = nomenclature };
				var secondBarcode = new Barcode { Type = BarcodeTypes.EAN13, Title = "2000000000002", Nomenclature = nomenclature };
				uow.Save(firstBarcode);
				uow.Save(secondBarcode);
				var firstBarcodeOperation = new BarcodeOperation { Barcode = firstBarcode, EmployeeIssueOperation = expenseItem.EmployeeIssueOperation };
				var secondBarcodeOperation = new BarcodeOperation { Barcode = secondBarcode, EmployeeIssueOperation = expenseItem.EmployeeIssueOperation };
				uow.Save(firstBarcodeOperation);
				uow.Save(secondBarcodeOperation);
				uow.Commit();

				var employeeIssueOperationId = expenseItem.EmployeeIssueOperation.Id;
				var employeeId = employee.Id;
				Assert.That(employeeIssueOperationId, Is.GreaterThan(0), "У операции выдачи должен появиться реальный Id после Save/Commit.");

				//Отдельная сессия к той же самой БД - как и в программе документ списания открывается отдельным диалогом (своим UnitOfWork)
				using(var uow2 = UnitOfWorkFactory.CreateWithoutRoot()) {
					var operation = uow2.GetById<EmployeeIssueOperation>(employeeIssueOperationId);
					Assert.That(operation.BarcodeOperations, Has.Count.EqualTo(2), "У операции выдачи должно быть 2 привязанных BarcodeOperation.");

					var barcodeOperationRepository = new BarcodeOperationRepository(uow2);
					var availableBarcodeIds = barcodeOperationRepository.GetAvailableBarcodeIdsForReturn(operation, uow2);
					Assert.That(availableBarcodeIds, Has.Count.EqualTo(2), "Обе метки ещё не списаны - обе должны быть доступны.");

					var barcodes = barcodeOperationRepository.GetBarcodes(availableBarcodeIds, uow2);
					var writeoff = new Writeoff {
						Date = new DateTime(2018, 10, 25)
					};
					foreach(var barcode in barcodes)
						writeoff.AddItem(operation, 1, new[] { barcode });

					Assert.That(writeoff.Items, Has.Count.EqualTo(2), "Каждый штрихкод должен попасть в отдельную строку документа.");

					writeoff.UpdateOperations(uow2);
					uow2.Save(writeoff);
					uow2.Commit();

					var employeeAfter = uow2.GetById<EmployeeCard>(employeeId);
					var employeeRepository = new EmployeeIssueRepository(uow2);
					var balanceAfter = employeeRepository.ItemsBalance(employeeAfter, new DateTime(2018, 10, 30));
					Assert.That(balanceAfter.Sum(x => x.Amount), Is.EqualTo(0), "Оба списаны - на сотруднике ничего не должно остаться.");
				}
			}
		}
	}
}
