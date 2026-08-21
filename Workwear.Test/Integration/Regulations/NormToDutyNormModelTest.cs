using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Testing.DB;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Statements;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Models.Operations;
using Workwear.Models.Regulations;
using Workwear.Repository.Operations;
using Workwear.Repository.Stock;
using Workwear.Repository.Stock.Documents;

namespace Workwear.Test.Integration.Regulations {

	[TestFixture(TestOf = typeof(NormToDutyNormModel))]
	[Category("Integrated")]
	public class NormToDutyNormModelTest : InMemoryDBGlobalConfigTestFixtureBase {

		[OneTimeSetUp]
		public void Init() {
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
		}

		[Test(Description = "Перенос нормы c сотрудником, в дежурную. Норма должна быть удалена, создана дежурная норма с ответственным и с той же потребностью и датой следующей выдачи, документ выдачи перенесён на дежурную норму, а потребности сотрудника пересчитаны.")]
		public void CopyNormToDutyNorm_TransfersEmployeeNormAndUpdatesNeeds() {
			NewSessionWithSameDB();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var itemType = new ItemsType { Name = "Тип перчатки" };
				uow.Save(itemType);
				var nomenclature = new Nomenclature { Type = itemType };
				uow.Save(nomenclature);
				var protectionTools = new ProtectionTools { Name = "Перчатки", Type = itemType };
				protectionTools.AddNomenclature(nomenclature);
				uow.Save(protectionTools);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Year;
				normItem.PeriodCount = 1;
				uow.Save(norm);

				var employee = new EmployeeCard { FirstName = "Иван", Patronymic = "Сергеевич", LastName = "Г." , Sex = Sex.M};
				employee.AddUsedNorm(norm);
				uow.Save(employee);

				var expectedNextIssue = new DateTime(2025, 1, 10);
				employee.WorkwearItems.First().Created = new DateTime(2024, 1, 10);
				employee.WorkwearItems.First().NextIssue = expectedNextIssue;
				uow.Save(employee);

				var user = new UserBase();
				uow.Save(user);
				var warehouse = new Warehouse { Name = "Склад" };
				uow.Save(warehouse);

				var warehouseOperation = new WarehouseOperation {
					ExpenseWarehouse = warehouse,
					Nomenclature = nomenclature,
					Amount = 1,
					OperationTime = new DateTime(2024, 1, 10)
				};
				uow.Save(warehouseOperation);

				var issueOperation = new EmployeeIssueOperation {
					Employee = employee,
					ProtectionTools = protectionTools,
					Nomenclature = nomenclature,
					NormItem = normItem,
					OperationTime = new DateTime(2024, 1, 10),
					StartOfUse = new DateTime(2024, 1, 10),
					Issued = 1,
					WarehouseOperation = warehouseOperation
				};
				uow.Save(issueOperation);

				var expenseDoc = new Expense {
					Employee = employee,
					Warehouse = warehouse,
					IssueDate = new DateTime(2024, 1, 10),
					Date = new DateTime(2024, 1, 10),
					CreatedbyUser = user
				};
				uow.Save(expenseDoc);
				var expenseItem = new ExpenseItem {
					ExpenseDoc = expenseDoc,
					ProtectionTools = protectionTools,
					Nomenclature = nomenclature,
					Amount = 1,
					EmployeeIssueOperation = issueOperation,
					WarehouseOperation = warehouseOperation
				};
				expenseDoc.Items.Add(expenseItem);
				uow.Save(expenseItem);

				var issuanceSheet = new IssuanceSheet {
					Date = new DateTime(2024, 1, 10),
					Expense = expenseDoc
				};
				expenseDoc.IssuanceSheet = issuanceSheet;
				uow.Save(issuanceSheet);
				var issuanceSheetItem = issuanceSheet.AddItem(expenseItem);
				uow.Save(issuanceSheetItem);
				uow.Save(issuanceSheet);

				uow.Commit();

				var normId = norm.Id;
				var expenseDocId = expenseDoc.Id;
				var employeeId = employee.Id;

				var model = new NormToDutyNormModel(
					Substitute.For<IInteractiveService>(),
					Substitute.For<IProgressBarDisplayable>(),
					UnitOfWorkFactory,
					new EmployeeIssueRepository(),
					new EmployeeIssueModel(new EmployeeIssueRepository()),
					new StockDocumentRepository(),
					new BarcodeRepository(new UnitOfWorkProvider()));
				model.CopyNormToDutyNorm(normId);

				using(var checkUow = UnitOfWorkFactory.CreateWithoutRoot()) {
					Assert.That(checkUow.GetById<Norm>(normId), Is.Null, "Исходная норма должна быть удалена.");
					Assert.That(checkUow.GetById<Expense>(expenseDocId), Is.Null, "Исходный документ выдачи должен быть удалён, т.к. был перенесён целиком.");

					var newDutyNorms = checkUow.Session.QueryOver<DutyNorm>().List()
						.Where(x => x.ResponsibleEmployee != null && x.ResponsibleEmployee.Id == employeeId).ToList();
					Assert.That(newDutyNorms, Has.Count.EqualTo(1), "Для сотрудника должна быть создана ровно одна дежурная норма.");
					var newDutyNorm = newDutyNorms.Single();

					Assert.That(newDutyNorm.Items, Has.Count.EqualTo(1));
					Assert.That(newDutyNorm.Items.First().NextIssue, Is.EqualTo(expectedNextIssue),
						"Потребность (дата следующей выдачи) должна быть перенесена в дежурную норму без изменений.");

					var newExpenseDutyNormDocs = checkUow.Session.QueryOver<ExpenseDutyNorm>().List()
						.Where(x => x.DutyNorm.Id == newDutyNorm.Id).ToList();
					Assert.That(newExpenseDutyNormDocs, Has.Count.EqualTo(1), "Должен быть создан один документ выдачи по дежурной норме.");
					var newExpenseDutyNormDoc = newExpenseDutyNormDocs.Single();
					Assert.That(newExpenseDutyNormDoc.Items, Has.Count.EqualTo(1));
					Assert.That(newExpenseDutyNormDoc.Comment, Does.Contain($"Исходная выдача №{expenseDocId}"));

					var employeeAfter = checkUow.GetById<EmployeeCard>(employeeId);
					Assert.That(employeeAfter.WorkwearItems, Has.Count.EqualTo(0),
						"У сотрудника не должно остаться потребностей - норма у него больше не используется.");

					var issuanceSheetAfter = checkUow.GetById<IssuanceSheet>(issuanceSheet.Id);
					Assert.That(issuanceSheetAfter.Expense, Is.Null, "Ведомость должна быть отвязана от исходного документа выдачи.");
					Assert.That(issuanceSheetAfter.ExpenseDutyNorm, Is.Not.Null,
						"При полном переносе ведомость должна перепривязаться на новый документ по дежурной норме.");
					Assert.That(issuanceSheetAfter.ExpenseDutyNorm.Id, Is.EqualTo(newExpenseDutyNormDoc.Id));

					var issuanceSheetItemAfter = issuanceSheetAfter.Items.Single();
					Assert.That(issuanceSheetItemAfter.ExpenseItem, Is.Null, "Ссылка на строку исходного документа в ведомости должна быть очищена.");
					Assert.That(issuanceSheetItemAfter.ExpenseDutyNormItem, Is.Not.Null,
						"Строка ведомости должна получить ссылку на новую строку выдачи по дежурной норме.");
					Assert.That(issuanceSheetItemAfter.ExpenseDutyNormItem.Document.Id, Is.EqualTo(newExpenseDutyNormDoc.Id));
					Assert.That(issuanceSheetItemAfter.DutyNormIssueOperation, Is.Not.Null,
						"Строка ведомости должна получить ссылку на операцию выдачи по дежурной норме.");
				}
			}
		}

		[Test(Description = "Перенос документа персональной выдачи на дежурную норму: строки, для которых у целевой дежурной нормы нет соответствующей потребности, должны остаться в исходном документе, а он сам не должен быть удалён. Потребность сотрудника по перенесённой строке должна быть пересчитана.")]
		public void CopyExpenseToDutyNorm_PartialMatch() {
			NewSessionWithSameDB();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var matchingItemType = new ItemsType { Name = "Тип перчатки" };
				uow.Save(matchingItemType);
				var matchingNomenclature = new Nomenclature { Type = matchingItemType };
				uow.Save(matchingNomenclature);
				var matchingTools = new ProtectionTools { Name = "Перчатки", Type = matchingItemType };
				matchingTools.AddNomenclature(matchingNomenclature);
				uow.Save(matchingTools);

				var nonMatchingItemType = new ItemsType { Name = "Тип каски" };
				uow.Save(nonMatchingItemType);
				var nonMatchingNomenclature = new Nomenclature { Type = nonMatchingItemType };
				uow.Save(nonMatchingNomenclature);
				var nonMatchingTools = new ProtectionTools { Name = "Каска", Type = nonMatchingItemType };
				nonMatchingTools.AddNomenclature(nonMatchingNomenclature);
				uow.Save(nonMatchingTools);

				var norm = new Norm();
				var matchingNormItem = norm.AddItem(matchingTools);
				matchingNormItem.Amount = 1;
				matchingNormItem.NormPeriod = NormPeriodType.Year;
				matchingNormItem.PeriodCount = 1;
				var nonMatchingNormItem = norm.AddItem(nonMatchingTools);
				nonMatchingNormItem.Amount = 1;
				nonMatchingNormItem.NormPeriod = NormPeriodType.Year;
				nonMatchingNormItem.PeriodCount = 1;
				uow.Save(norm);

				var employee = new EmployeeCard { FirstName = "Иван", Patronymic = "Сергеевич", LastName = "Г." , Sex = Sex.M};
				employee.AddUsedNorm(norm);
				uow.Save(employee);
				var oldNextIssueForMatching = employee.WorkwearItems.First(x => x.ProtectionTools == matchingTools).NextIssue;

				var user = new UserBase();
				uow.Save(user);
				var warehouse = new Warehouse { Name = "Склад" };
				uow.Save(warehouse);

				var matchingWarehouseOperation = new WarehouseOperation {
					ExpenseWarehouse = warehouse,
					Nomenclature = matchingNomenclature,
					Amount = 1,
					OperationTime = new DateTime(2024, 2, 1)
				};
				uow.Save(matchingWarehouseOperation);
				var matchingIssueOperation = new EmployeeIssueOperation {
					Employee = employee,
					ProtectionTools = matchingTools,
					Nomenclature = matchingNomenclature,
					NormItem = matchingNormItem,
					OperationTime = new DateTime(2024, 2, 1),
					StartOfUse = new DateTime(2024, 2, 1),
					Issued = 1,
					WarehouseOperation = matchingWarehouseOperation
				};
				uow.Save(matchingIssueOperation);

				var nonMatchingWarehouseOperation = new WarehouseOperation {
					ExpenseWarehouse = warehouse,
					Nomenclature = nonMatchingNomenclature,
					Amount = 1,
					OperationTime = new DateTime(2024, 2, 1)
				};
				uow.Save(nonMatchingWarehouseOperation);
				var nonMatchingIssueOperation = new EmployeeIssueOperation {
					Employee = employee,
					ProtectionTools = nonMatchingTools,
					Nomenclature = nonMatchingNomenclature,
					NormItem = nonMatchingNormItem,
					OperationTime = new DateTime(2024, 2, 1),
					StartOfUse = new DateTime(2024, 2, 1),
					Issued = 1,
					WarehouseOperation = nonMatchingWarehouseOperation
				};
				uow.Save(nonMatchingIssueOperation);

				var expenseDoc = new Expense {
					Employee = employee,
					Warehouse = warehouse,
					IssueDate = new DateTime(2024, 2, 1),
					Date = new DateTime(2024, 2, 1),
					CreatedbyUser = user
				};
				uow.Save(expenseDoc);

				var matchingExpenseItem = new ExpenseItem {
					ExpenseDoc = expenseDoc,
					ProtectionTools = matchingTools,
					Nomenclature = matchingNomenclature,
					Amount = 1,
					EmployeeIssueOperation = matchingIssueOperation,
					WarehouseOperation = matchingWarehouseOperation
				};
				expenseDoc.Items.Add(matchingExpenseItem);
				uow.Save(matchingExpenseItem);

				var nonMatchingExpenseItem = new ExpenseItem {
					ExpenseDoc = expenseDoc,
					ProtectionTools = nonMatchingTools,
					Nomenclature = nonMatchingNomenclature,
					Amount = 1,
					EmployeeIssueOperation = nonMatchingIssueOperation,
					WarehouseOperation = nonMatchingWarehouseOperation
				};
				expenseDoc.Items.Add(nonMatchingExpenseItem);
				uow.Save(nonMatchingExpenseItem);

				var issuanceSheet = new IssuanceSheet {
					Date = new DateTime(2024, 2, 1),
					Expense = expenseDoc
				};
				expenseDoc.IssuanceSheet = issuanceSheet;
				uow.Save(issuanceSheet);
				var matchingIssuanceSheetItem = issuanceSheet.AddItem(matchingExpenseItem);
				uow.Save(matchingIssuanceSheetItem);
				var nonMatchingIssuanceSheetItem = issuanceSheet.AddItem(nonMatchingExpenseItem);
				uow.Save(nonMatchingIssuanceSheetItem);
				uow.Save(issuanceSheet);

				// Целевая дежурная норма понимает только "Перчатки" - потребности в "Каске" у неё нет.
				var dutyNorm = new DutyNorm();
				uow.Save(dutyNorm);
				var dutyNormItem = dutyNorm.AddItem(matchingTools);
				uow.Save(dutyNormItem);

				uow.Commit();

				var expenseDocId = expenseDoc.Id;
				var dutyNormId = dutyNorm.Id;
				var employeeId = employee.Id;
				var issuanceSheetId = issuanceSheet.Id;
				var nonMatchingProtectionToolsId = nonMatchingTools.Id;
				var matchingProtectionToolsId = matchingTools.Id;

				var model = new NormToDutyNormModel(
					Substitute.For<IInteractiveService>(),
					Substitute.For<IProgressBarDisplayable>(),
					UnitOfWorkFactory,
					new EmployeeIssueRepository(),
					new EmployeeIssueModel(new EmployeeIssueRepository()),
					new StockDocumentRepository(),
					new BarcodeRepository(new UnitOfWorkProvider()));
				model.CopyExpenseToDutyNorm(expenseDocId, dutyNormId);

				using(var checkUow = UnitOfWorkFactory.CreateWithoutRoot()) {
					var sourceDoc = checkUow.GetById<Expense>(expenseDocId);
					Assert.That(sourceDoc, Is.Not.Null, "Исходный документ не должен быть удалён, т.к. в нём осталась строка не подходящая дежурной норме.");
					Assert.That(sourceDoc.Items, Has.Count.EqualTo(1));
					Assert.That(sourceDoc.Items.First().ProtectionTools.Id, Is.EqualTo(nonMatchingProtectionToolsId));

					var newDoc = checkUow.Session.QueryOver<ExpenseDutyNorm>().List()
						.Single(x => x.DutyNorm.Id == dutyNormId);
					Assert.That(newDoc.Items, Has.Count.EqualTo(1));
					Assert.That(newDoc.Items.First().Operation.ProtectionTools.Id, Is.EqualTo(matchingProtectionToolsId));
					Assert.That(newDoc.Items.First().Operation.DutyNormItem.Id, Is.EqualTo(dutyNormItem.Id));

					var employeeAfter = checkUow.GetById<EmployeeCard>(employeeId);
					var matchingItemAfter = employeeAfter.WorkwearItems.First(x => x.ProtectionTools.Id == matchingProtectionToolsId);
					Assert.That(matchingItemAfter.NextIssue, Is.Not.EqualTo(oldNextIssueForMatching),
						"Потребность сотрудника по перенесённой строке должна быть пересчитана - выдача больше не числится за ним.");
					Assert.That(employeeAfter.WorkwearItems.Any(x => x.ProtectionTools.Id == nonMatchingProtectionToolsId), Is.True,
						"Потребность по строке оставшейся у сотрудника не должна была исчезнуть.");

					var issuanceSheetAfter = checkUow.GetById<IssuanceSheet>(issuanceSheetId);
					Assert.That(issuanceSheetAfter.Expense, Is.Null,
						"Ведомость должна быть отвязана от исходного документа даже при частичном переносе.");
					Assert.That(issuanceSheetAfter.ExpenseDutyNorm, Is.Null,
						"При частичном переносе (не все строки ведомости покрыты) ведомость НЕ должна перепривязываться на новый документ.");
					foreach(var item in issuanceSheetAfter.Items)
						Assert.That(item.ExpenseItem, Is.Null,
							"Ссылки на строки исходного документа должны быть очищены у всех строк ведомости, включая ту, что осталась в документе.");

					Assert.That(sourceDoc.Comment, Does.Contain("Отвязана ведомость"),
						"На исходном документе должен остаться след про отвязку ведомости.");
					Assert.That(newDoc.Comment, Does.Contain("ведомость №"),
						"На новом документе дежурной нормы должна остаться ссылка на номер ведомости.");
				}
			}
		}

		[Test(Description = "При переносе документа выдачи в дежурную норму связанное с перенесённой операцией списание должно быть перепривязано на операцию списания дежурной нормы, а старая операция списания с сотрудника должна быть удалена.")]
		public void CopyExpenseToDutyNorm_TransfersLinkedWriteOff() {
			NewSessionWithSameDB();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var itemType = new ItemsType { Name = "Тип ботинок" };
				uow.Save(itemType);
				var nomenclature = new Nomenclature { Type = itemType };
				uow.Save(nomenclature);
				var protectionTools = new ProtectionTools { Name = "Ботинки", Type = itemType };
				protectionTools.AddNomenclature(nomenclature);
				uow.Save(protectionTools);

				var employee = new EmployeeCard { FirstName = "Иван", Patronymic = "Сергеевич", LastName = "Г." , Sex = Sex.M};
				uow.Save(employee);

				var user = new UserBase();
				uow.Save(user);
				var warehouse = new Warehouse { Name = "Склад" };
				uow.Save(warehouse);

				var warehouseOperation = new WarehouseOperation {
					ExpenseWarehouse = warehouse,
					Nomenclature = nomenclature,
					Amount = 1,
					OperationTime = new DateTime(2024, 3, 1)
				};
				uow.Save(warehouseOperation);
				var issueOperation = new EmployeeIssueOperation {
					Employee = employee,
					ProtectionTools = protectionTools,
					Nomenclature = nomenclature,
					OperationTime = new DateTime(2024, 3, 1),
					StartOfUse = new DateTime(2024, 3, 1),
					Issued = 1,
					WarehouseOperation = warehouseOperation
				};
				uow.Save(issueOperation);

				var expenseDoc = new Expense {
					Employee = employee,
					Warehouse = warehouse,
					IssueDate = new DateTime(2024, 3, 1),
					Date = new DateTime(2024, 3, 1),
					CreatedbyUser = user
				};
				uow.Save(expenseDoc);
				var expenseItem = new ExpenseItem {
					ExpenseDoc = expenseDoc,
					ProtectionTools = protectionTools,
					Nomenclature = nomenclature,
					Amount = 1,
					EmployeeIssueOperation = issueOperation,
					WarehouseOperation = warehouseOperation
				};
				expenseDoc.Items.Add(expenseItem);
				uow.Save(expenseItem);

				var writeoff = new Writeoff { Date = new DateTime(2024, 4, 1) };
				uow.Save(writeoff);
				var writeoffItem = writeoff.AddItem(issueOperation, 1);
				uow.Save(writeoffItem.EmployeeWriteoffOperation);
				uow.Save(writeoffItem);

				var dutyNorm = new DutyNorm();
				uow.Save(dutyNorm);
				var dutyNormItem = dutyNorm.AddItem(protectionTools);
				uow.Save(dutyNormItem);

				uow.Commit();

				var expenseDocId = expenseDoc.Id;
				var dutyNormId = dutyNorm.Id;
				var writeoffItemId = writeoffItem.Id;
				var oldWriteOffOperationId = writeoffItem.EmployeeWriteoffOperation.Id;

				var model = new NormToDutyNormModel(
					Substitute.For<IInteractiveService>(),
					Substitute.For<IProgressBarDisplayable>(),
					UnitOfWorkFactory,
					new EmployeeIssueRepository(),
					new EmployeeIssueModel(new EmployeeIssueRepository()),
					new StockDocumentRepository(),
					new BarcodeRepository(new UnitOfWorkProvider()));
				model.CopyExpenseToDutyNorm(expenseDocId, dutyNormId);

				using(var checkUow = UnitOfWorkFactory.CreateWithoutRoot()) {
					Assert.That(checkUow.GetById<EmployeeIssueOperation>(oldWriteOffOperationId), Is.Null,
						"Старая операция списания с сотрудника должна быть удалена.");

					var writeoffItemAfter = checkUow.GetById<WriteoffItem>(writeoffItemId);
					Assert.That(writeoffItemAfter.EmployeeWriteoffOperation, Is.Null);
					Assert.That(writeoffItemAfter.DutyNormWriteOffOperation, Is.Not.Null,
						"Операция списания должна быть перепривязана на операцию списания дежурной нормы.");
					Assert.That(writeoffItemAfter.DutyNormWriteOffOperation.DutyNorm.Id, Is.EqualTo(dutyNormId));
					Assert.That(writeoffItemAfter.DutyNormWriteOffOperation.IssuedOperation.DutyNormItem.Id, Is.EqualTo(dutyNormItem.Id));
				}
			}
		}

		[Test(Description = "Перенос документа коллективной выдачи, затрагивающей нескольких сотрудников: должны переноситься только строки с потребностью, известной дежурной норме, а потребности должны быть пересчитаны у всех задействованных сотрудников.")]
		public void CopyCollectiveExpenseToDutyNorm_TransfersMatchingItems_ForAllEmployees() {
			NewSessionWithSameDB();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var itemType = new ItemsType { Name = "Тип жилета" };
				uow.Save(itemType);
				var nomenclature = new Nomenclature { Type = itemType };
				uow.Save(nomenclature);
				var protectionTools = new ProtectionTools { Name = "Жилет", Type = itemType };
				protectionTools.AddNomenclature(nomenclature);
				uow.Save(protectionTools);

				var employee1 = new EmployeeCard { FirstName = "Первый" };
				uow.Save(employee1);
				var employee2 = new EmployeeCard { FirstName = "Второй" };
				uow.Save(employee2);

				var user = new UserBase();
				uow.Save(user);
				var warehouse = new Warehouse { Name = "Склад" };
				uow.Save(warehouse);

				var warehouseOperation1 = new WarehouseOperation {
					ExpenseWarehouse = warehouse,
					Nomenclature = nomenclature,
					Amount = 1,
					OperationTime = new DateTime(2024, 5, 1)
				};
				uow.Save(warehouseOperation1);
				var issueOperation1 = new EmployeeIssueOperation {
					Employee = employee1,
					ProtectionTools = protectionTools,
					Nomenclature = nomenclature,
					OperationTime = new DateTime(2024, 5, 1),
					StartOfUse = new DateTime(2024, 5, 1),
					Issued = 1,
					WarehouseOperation = warehouseOperation1
				};
				uow.Save(issueOperation1);

				var warehouseOperation2 = new WarehouseOperation {
					ExpenseWarehouse = warehouse,
					Nomenclature = nomenclature,
					Amount = 1,
					OperationTime = new DateTime(2024, 5, 1)
				};
				uow.Save(warehouseOperation2);
				var issueOperation2 = new EmployeeIssueOperation {
					Employee = employee2,
					ProtectionTools = protectionTools,
					Nomenclature = nomenclature,
					OperationTime = new DateTime(2024, 5, 1),
					StartOfUse = new DateTime(2024, 5, 1),
					Issued = 1,
					WarehouseOperation = warehouseOperation2
				};
				uow.Save(issueOperation2);

				var collectiveExpenseDoc = new CollectiveExpense {
					Warehouse = warehouse,
					Date = new DateTime(2024, 5, 1),
					CreatedbyUser = user
				};
				uow.Save(collectiveExpenseDoc);

				var item1 = new CollectiveExpenseItem {
					Document = collectiveExpenseDoc,
					Employee = employee1,
					ProtectionTools = protectionTools,
					Nomenclature = nomenclature,
					Amount = 1,
					EmployeeIssueOperation = issueOperation1,
					WarehouseOperation = warehouseOperation1
				};
				collectiveExpenseDoc.Items.Add(item1);
				uow.Save(item1);

				var item2 = new CollectiveExpenseItem {
					Document = collectiveExpenseDoc,
					Employee = employee2,
					ProtectionTools = protectionTools,
					Nomenclature = nomenclature,
					Amount = 1,
					EmployeeIssueOperation = issueOperation2,
					WarehouseOperation = warehouseOperation2
				};
				collectiveExpenseDoc.Items.Add(item2);
				uow.Save(item2);

				var dutyNorm = new DutyNorm();
				uow.Save(dutyNorm);
				var dutyNormItem = dutyNorm.AddItem(protectionTools);
				uow.Save(dutyNormItem);

				uow.Commit();

				var collectiveExpenseDocId = collectiveExpenseDoc.Id;
				var dutyNormId = dutyNorm.Id;

				var model = new NormToDutyNormModel(
					Substitute.For<IInteractiveService>(),
					Substitute.For<IProgressBarDisplayable>(),
					UnitOfWorkFactory,
					new EmployeeIssueRepository(),
					new EmployeeIssueModel(new EmployeeIssueRepository()),
					new StockDocumentRepository(),
					new BarcodeRepository(new UnitOfWorkProvider()));
				model.CopyCollectiveExpenseToDutyNorm(collectiveExpenseDocId, dutyNormId);

				using(var checkUow = UnitOfWorkFactory.CreateWithoutRoot()) {
					Assert.That(checkUow.GetById<CollectiveExpense>(collectiveExpenseDocId), Is.Null,
						"Исходный документ должен быть удалён, т.к. обе строки перенесены.");

					var newDoc = checkUow.Session.QueryOver<ExpenseDutyNorm>().List().Single(x => x.DutyNorm.Id == dutyNormId);
					Assert.That(newDoc.Items, Has.Count.EqualTo(2));
				}
			}
		}

		[Test(Description = "Перенос документа коллективной выдачи в дежурную норму: строка без соответствующей потребности в целевой дежурной норме остаётся в исходном документе (документ не удаляется), совпавшая строка переносится, потребность соответствующего сотрудника пересчитывается, а ведомость (покрывающая весь документ) для коллективной выдачи всегда только отвязывается - даже частично.")]
		public void CopyCollectiveExpenseToDutyNorm_PartialMatch_KeepsNonMatchingItemsAndUpdatesEmployeeNeeds() {
			NewSessionWithSameDB();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var matchingItemType = new ItemsType { Name = "Тип перчатки" };
				uow.Save(matchingItemType);
				var matchingNomenclature = new Nomenclature { Type = matchingItemType };
				uow.Save(matchingNomenclature);
				var matchingTools = new ProtectionTools { Name = "Перчатки", Type = matchingItemType };
				matchingTools.AddNomenclature(matchingNomenclature);
				uow.Save(matchingTools);

				var nonMatchingItemType = new ItemsType { Name = "Тип каски" };
				uow.Save(nonMatchingItemType);
				var nonMatchingNomenclature = new Nomenclature { Type = nonMatchingItemType };
				uow.Save(nonMatchingNomenclature);
				var nonMatchingTools = new ProtectionTools { Name = "Каска", Type = nonMatchingItemType };
				nonMatchingTools.AddNomenclature(nonMatchingNomenclature);
				uow.Save(nonMatchingTools);

				var norm = new Norm();
				var matchingNormItem = norm.AddItem(matchingTools);
				matchingNormItem.Amount = 1;
				matchingNormItem.NormPeriod = NormPeriodType.Year;
				matchingNormItem.PeriodCount = 1;
				uow.Save(norm);

				var employee1 = new EmployeeCard { FirstName = "Первый" };
				employee1.AddUsedNorm(norm);
				uow.Save(employee1);
				var oldNextIssueForMatching = employee1.WorkwearItems.First().NextIssue;

				var employee2 = new EmployeeCard { FirstName = "Второй" };
				uow.Save(employee2);

				var user = new UserBase();
				uow.Save(user);
				var warehouse = new Warehouse { Name = "Склад" };
				uow.Save(warehouse);

				var matchingWarehouseOperation = new WarehouseOperation {
					ExpenseWarehouse = warehouse,
					Nomenclature = matchingNomenclature,
					Amount = 1,
					OperationTime = new DateTime(2024, 6, 1)
				};
				uow.Save(matchingWarehouseOperation);
				var matchingIssueOperation = new EmployeeIssueOperation {
					Employee = employee1,
					ProtectionTools = matchingTools,
					Nomenclature = matchingNomenclature,
					NormItem = matchingNormItem,
					OperationTime = new DateTime(2024, 6, 1),
					StartOfUse = new DateTime(2024, 6, 1),
					Issued = 1,
					WarehouseOperation = matchingWarehouseOperation
				};
				uow.Save(matchingIssueOperation);

				var nonMatchingWarehouseOperation = new WarehouseOperation {
					ExpenseWarehouse = warehouse,
					Nomenclature = nonMatchingNomenclature,
					Amount = 1,
					OperationTime = new DateTime(2024, 6, 1)
				};
				uow.Save(nonMatchingWarehouseOperation);
				var nonMatchingIssueOperation = new EmployeeIssueOperation {
					Employee = employee2,
					ProtectionTools = nonMatchingTools,
					Nomenclature = nonMatchingNomenclature,
					OperationTime = new DateTime(2024, 6, 1),
					StartOfUse = new DateTime(2024, 6, 1),
					Issued = 1,
					WarehouseOperation = nonMatchingWarehouseOperation
				};
				uow.Save(nonMatchingIssueOperation);

				var collectiveExpenseDoc = new CollectiveExpense {
					Warehouse = warehouse,
					Date = new DateTime(2024, 6, 1),
					CreatedbyUser = user
				};
				uow.Save(collectiveExpenseDoc);

				var matchingItem = new CollectiveExpenseItem {
					Document = collectiveExpenseDoc,
					Employee = employee1,
					ProtectionTools = matchingTools,
					Nomenclature = matchingNomenclature,
					Amount = 1,
					EmployeeIssueOperation = matchingIssueOperation,
					WarehouseOperation = matchingWarehouseOperation
				};
				collectiveExpenseDoc.Items.Add(matchingItem);
				uow.Save(matchingItem);

				var nonMatchingItem = new CollectiveExpenseItem {
					Document = collectiveExpenseDoc,
					Employee = employee2,
					ProtectionTools = nonMatchingTools,
					Nomenclature = nonMatchingNomenclature,
					Amount = 1,
					EmployeeIssueOperation = nonMatchingIssueOperation,
					WarehouseOperation = nonMatchingWarehouseOperation
				};
				collectiveExpenseDoc.Items.Add(nonMatchingItem);
				uow.Save(nonMatchingItem);

				// Ведомость покрывает весь документ (обе строки) - для коллективной выдачи она всегда только отвязывается,
				// перепривязки на новый документ по дежурной норме не бывает, даже если бы перенеслось всё.
				var issuanceSheet = new IssuanceSheet {
					Date = new DateTime(2024, 6, 1),
					CollectiveExpense = collectiveExpenseDoc
				};
				collectiveExpenseDoc.IssuanceSheet = issuanceSheet;
				uow.Save(issuanceSheet);
				var matchingIssuanceSheetItem = issuanceSheet.AddItem(matchingItem);
				uow.Save(matchingIssuanceSheetItem);
				var nonMatchingIssuanceSheetItem = issuanceSheet.AddItem(nonMatchingItem);
				uow.Save(nonMatchingIssuanceSheetItem);
				uow.Save(issuanceSheet);

				// Целевая дежурная норма понимает только "Перчатки" - потребности в "Каске" у неё нет.
				var dutyNorm = new DutyNorm();
				uow.Save(dutyNorm);
				var dutyNormItem = dutyNorm.AddItem(matchingTools);
				uow.Save(dutyNormItem);

				uow.Commit();

				var collectiveExpenseDocId = collectiveExpenseDoc.Id;
				var dutyNormId = dutyNorm.Id;
				var employee1Id = employee1.Id;
				var issuanceSheetId = issuanceSheet.Id;
				var nonMatchingProtectionToolsId = nonMatchingTools.Id;
				var matchingProtectionToolsId = matchingTools.Id;

				var model = new NormToDutyNormModel(
					Substitute.For<IInteractiveService>(),
					Substitute.For<IProgressBarDisplayable>(),
					UnitOfWorkFactory,
					new EmployeeIssueRepository(),
					new EmployeeIssueModel(new EmployeeIssueRepository()),
					new StockDocumentRepository(),
					new BarcodeRepository(new UnitOfWorkProvider()));
				model.CopyCollectiveExpenseToDutyNorm(collectiveExpenseDocId, dutyNormId);

				using(var checkUow = UnitOfWorkFactory.CreateWithoutRoot()) {
					var sourceDoc = checkUow.GetById<CollectiveExpense>(collectiveExpenseDocId);
					Assert.That(sourceDoc, Is.Not.Null,
						"Исходный документ не должен быть удалён, т.к. в нём осталась строка не подходящая дежурной норме.");
					Assert.That(sourceDoc.Items, Has.Count.EqualTo(1));
					Assert.That(sourceDoc.Items.First().ProtectionTools.Id, Is.EqualTo(nonMatchingProtectionToolsId));

					var newDoc = checkUow.Session.QueryOver<ExpenseDutyNorm>().List().Single(x => x.DutyNorm.Id == dutyNormId);
					Assert.That(newDoc.Items, Has.Count.EqualTo(1));
					Assert.That(newDoc.Items.First().Operation.ProtectionTools.Id, Is.EqualTo(matchingProtectionToolsId));
					Assert.That(newDoc.Items.First().Operation.DutyNormItem.Id, Is.EqualTo(dutyNormItem.Id));

					var employee1After = checkUow.GetById<EmployeeCard>(employee1Id);
					var matchingItemAfter = employee1After.WorkwearItems.First(x => x.ProtectionTools.Id == matchingProtectionToolsId);
					Assert.That(matchingItemAfter.NextIssue, Is.Not.EqualTo(oldNextIssueForMatching),
						"Потребность сотрудника, чья строка перенесена, должна быть пересчитана.");

					var issuanceSheetAfter = checkUow.GetById<IssuanceSheet>(issuanceSheetId);
					Assert.That(issuanceSheetAfter.CollectiveExpense, Is.Null,
						"Ведомость коллективной выдачи должна быть отвязана от документа даже при частичном переносе.");
					Assert.That(issuanceSheetAfter.ExpenseDutyNorm, Is.Null,
						"Для коллективной выдачи ведомость никогда не перепривязывается на документ по дежурной норме.");
					foreach(var item in issuanceSheetAfter.Items)
						Assert.That(item.CollectiveExpenseItem, Is.Null,
							"Ссылки на строки исходного документа должны быть очищены у всех строк ведомости.");
				}
			}
		}
	}
}
