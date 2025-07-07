﻿using System;
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

namespace Workwear.Test.Integration.Operations
{
	[TestFixture(TestOf = typeof(EmployeeIssueRepository), Description = "Репозитория информации о выдачах сотруднику")]
	public class EmployeeIssueRepositoryTest : InMemoryDBGlobalConfigTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init() {
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
		}

		#region GetOperationsTouchDates
		[Test(Description = "Проверяем что запрос не захватывает операции до даты выдачи.")]
		[Category("Integrated")]
		public void GetOperationsTouchDates_GetNotOperationBeforeAndAfterSelectedDateTest()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {

				var nomenclatureType = new ItemsType {
					Name = "Тестовый тип номенклатуры"
				};
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature {
					Type = nomenclatureType
				};
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools {
					Name = "СИЗ для тестирования"
				};
				protectionTools.AddNomenclature(nomenclature);
				uow.Save(protectionTools);

				var employee = new EmployeeCard();
				uow.Save(employee);

				//Операция без номеклатуры
				var opBefore = new EmployeeIssueOperation {
					OperationTime = new DateTime(2018, 1, 1, 14, 0, 0),
					AutoWriteoffDate = new DateTime(2020, 1, 1),
					Employee = employee,
					Nomenclature = nomenclature,
					ProtectionTools = protectionTools,
					Issued = 1
				};
				uow.Save(opBefore);

				var opInRange = new EmployeeIssueOperation {
					OperationTime = new DateTime(2019, 1, 1, 13, 0, 0),
					AutoWriteoffDate = new DateTime(2021, 1, 1),
					Employee = employee,
					Nomenclature = nomenclature,
					ProtectionTools = protectionTools,
					Issued = 1
				};
				uow.Save(opInRange);

				var opAfter = new EmployeeIssueOperation {
					OperationTime = new DateTime(2021, 1, 1, 13, 0, 0),
					AutoWriteoffDate = new DateTime(2021, 5, 1),
					Employee = employee,
					Nomenclature = nomenclature,
					ProtectionTools = protectionTools,
					Issued = 1
				};
				uow.Save(opAfter);

				uow.Commit();

				var repository = new EmployeeIssueRepository(uow);
				var result = repository.GetOperationsByDates(new[] { employee }, new DateTime(2018, 12, 30), new DateTime(2020, 1, 1));
				Assert.That(result.Count, Is.EqualTo(1));
				Assert.That(result.First().OperationTime, Is.EqualTo(new DateTime(2019, 1, 1, 13, 0, 0)));
			}
		}
		
		[Test(Description = "Проверяем что можем захватить операцию если период установлен одним днем.")]
		[Category("Integrated")]
		public void GetOperationsTouchDates_OneDateRangeTest()
		{

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature {
					Type = nomenclatureType
				};
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools {
					Name = "СИЗ для тестирования"
				};
				protectionTools.AddNomenclature(nomenclature);
				uow.Save(protectionTools);

				var employee = new EmployeeCard();
				uow.Save(employee);
				
				var opBefore = new EmployeeIssueOperation {
					OperationTime = new DateTime(2018, 12, 31, 14, 0, 0),
					AutoWriteoffDate = new DateTime(2020, 1, 1),
					Employee = employee,
					Nomenclature = nomenclature,
					ProtectionTools = protectionTools,
					Issued = 1
				};
				uow.Save(opBefore);

				var opInRange = new EmployeeIssueOperation {
					OperationTime = new DateTime(2019, 1, 1, 0, 0, 0),
					AutoWriteoffDate = new DateTime(2021, 1, 1),
					Employee = employee,
					Nomenclature = nomenclature,
					ProtectionTools = protectionTools,
					Issued = 1
				};
				uow.Save(opInRange);

				var opInRange2 = new EmployeeIssueOperation {
					OperationTime = new DateTime(2019, 1, 1, 13, 0, 0),
					AutoWriteoffDate = new DateTime(2021, 2, 1),
					Employee = employee,
					Nomenclature = nomenclature,
					ProtectionTools = protectionTools,
					Issued = 2
				};
				uow.Save(opInRange2);
				
				var opAfter = new EmployeeIssueOperation {
					OperationTime = new DateTime(2019, 1, 2, 13, 0, 0),
					AutoWriteoffDate = new DateTime(2021, 5, 1),
					Employee = employee,
					Nomenclature = nomenclature,
					ProtectionTools = protectionTools,
					Issued = 1
				};
				uow.Save(opAfter);

				uow.Commit();

				var repository = new EmployeeIssueRepository(uow);
				var result = 
					repository.GetOperationsByDates(
						new[] { employee }, new DateTime(2019, 1, 1), new DateTime(2019, 1, 1));
				Assert.That(result.Any(x => x.OperationTime == new DateTime(2019, 1, 1, 0, 0, 0)), Is.True);
				Assert.That(result.Any(x => x.OperationTime == new DateTime(2019, 1, 1, 13, 0, 0)), Is.True);
				Assert.That(result.Count, Is.EqualTo(2));
			}
		}
		#endregion
		#region GetReferencedDocuments
		[Test(Description = "Проверяем получение ссылки на документ выдачи")]
		[Category("Integrated")]
		public void GetReferencedDocuments_ExpenseTest()
		{
			var interactive = Substitute.For<IInteractiveQuestion>();
			interactive.Question(string.Empty).ReturnsForAnyArgs(true);
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

				var employee = new EmployeeCard();
				uow.Save(employee);

				var expense = new Expense {
					Date = new DateTime(2021, 9, 10),
					Employee = employee,
					Warehouse = warehouse,
				};

				var size = new Size();
				var height = new Size();
				uow.Save(size);
				uow.Save(height);

				var stockPosition = new StockPosition(nomenclature, 0, size, height, null);
				var item = expense.AddItem(stockPosition, 1);

				expense.UpdateOperations(uow, baseParameters, interactive);
				uow.Save(expense);
				uow.Commit();

				var repository = new EmployeeIssueRepository(uow);
				var result = repository.GetReferencedDocuments(item.EmployeeIssueOperation.Id);
				Assert.That(result.First().DocumentType, Is.EqualTo(StockDocumentType.ExpenseEmployeeDoc));
				Assert.That(result.First().DocumentId, Is.EqualTo(expense.Id));
				Assert.That(result.First().ItemId, Is.EqualTo(item.Id));
			}
		}
		
		[Test(Description = "Проверяем получение ссылки на документ коллективной выдачи")]
		[Category("Integrated")]
		public void GetReferencedDocuments_CollectiveExpenseTest()
		{
			var interactive = Substitute.For<IInteractiveQuestion>();
			interactive.Question(string.Empty).ReturnsForAnyArgs(true);
			var baseParameters = Substitute.For<BaseParameters>();

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {

				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var protectionTools = new ProtectionTools {
					Name = "Тестовая курточка"
				};
				uow.Save(protectionTools);

				var nomenclatureType = new ItemsType {
					Name = "Тестовый тип номенклатуры"
				};
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature {
					Type = nomenclatureType
				};
				nomenclature.ProtectionTools.Add(protectionTools);
				uow.Save(nomenclature);

				var norm = new Norm();
				norm.AddItem(protectionTools);
				uow.Save(norm);

				var employee = new EmployeeCard();
				employee.AddUsedNorm(norm);
				uow.Save(employee);

				var employee2 = new EmployeeCard();
				employee2.AddUsedNorm(norm);
				uow.Save(employee2);

				var expense = new CollectiveExpense() {
					Date = new DateTime(2021, 9, 10),
					Warehouse = warehouse,
				};
				
				var size = new Size();
				var height = new Size();
				uow.Save(size);
				uow.Save(height);

				var stockPosition = new StockPosition(nomenclature, 0, size, height, null);
				var item = expense.AddItem(employee.WorkwearItems.FirstOrDefault(), stockPosition, 1);
				var item2 = expense.AddItem(employee2.WorkwearItems.FirstOrDefault(), stockPosition, 10);

				expense.UpdateOperations(uow, baseParameters, interactive);
				uow.Save(expense);
				uow.Commit();

				var repository = new EmployeeIssueRepository(uow);
				var results = repository
					.GetReferencedDocuments(item.EmployeeIssueOperation.Id, item2.EmployeeIssueOperation.Id);
				var result1 = results.First(x => x.OperationId == item.EmployeeIssueOperation.Id);
				Assert.That(result1.DocumentType, Is.EqualTo(StockDocumentType.CollectiveExpense));
				Assert.That(result1.DocumentId, Is.EqualTo(expense.Id));
				Assert.That(result1.ItemId, Is.EqualTo(item.Id));
				var result2 = results.First(x => x.OperationId == item2.EmployeeIssueOperation.Id);
				Assert.That(result2.DocumentType, Is.EqualTo(StockDocumentType.CollectiveExpense));
				Assert.That(result2.DocumentId, Is.EqualTo(expense.Id));
				Assert.That(result2.ItemId, Is.EqualTo(item2.Id));
			}
		}
		
		[Test(Description = "Проверяем получение ссылки на документ возврата от сотрудника")]
		[Category("Integrated")]
		public void GetReferencedDocuments_ReturnTest()
		{
			var interactive = Substitute.For<IInteractiveQuestion>();
			interactive.Question(string.Empty).ReturnsForAnyArgs(true);
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

				var employee = new EmployeeCard();
				uow.Save(employee);

				var expense = new Expense {
					Date = new DateTime(2021, 9, 10),
					Employee = employee,
					Warehouse = warehouse,
				};
				
				var size = new Size();
				var height = new Size();
				uow.Save(size);
				uow.Save(height);

				var stockPosition = new StockPosition(nomenclature, 0, size, height, null);
				var item = expense.AddItem(stockPosition, 10);

				expense.UpdateOperations(uow, baseParameters, interactive);
				uow.Save(expense);
				
				//Возвращаем 2 штуки
				var returndoc = new Return() {
					Date = new DateTime(2021, 9, 11),
					Warehouse = warehouse,
				};
				
				var returnItem = returndoc.AddItem(item.EmployeeIssueOperation, 2);
				returndoc.UpdateOperations(uow);
				uow.Save(returndoc);
				uow.Commit();

				var repository = new EmployeeIssueRepository(uow);
				var result = repository.GetReferencedDocuments(returnItem.ReturnFromEmployeeOperation.Id);
				Assert.That(result.First().DocumentId, Is.EqualTo(returndoc.Id));
				Assert.That(result.First().ItemId, Is.EqualTo(returnItem.Id));
			}
		}
		
		[Test(Description = "Проверяем получение ссылки на документ списания от сотрудника")]
		[Category("Integrated")]
		public void GetReferencedDocuments_WriteOffTest()
		{
			var interactive = Substitute.For<IInteractiveQuestion>();
			interactive.Question(string.Empty).ReturnsForAnyArgs(true);
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

				var employee = new EmployeeCard();
				uow.Save(employee);

				var expense = new Expense {
					Date = new DateTime(2021, 9, 10),
					Employee = employee,
					Warehouse = warehouse,
				};
				
				var size = new Size();
				var height = new Size();
				uow.Save(size);
				uow.Save(height);

				var stockPosition = new StockPosition(nomenclature, 0, size, height, null);
				var item = expense.AddItem(stockPosition, 10);

				expense.UpdateOperations(uow, baseParameters, interactive);
				uow.Save(expense);
				
				//Списываем 3 штуки
				var writeoff = new Writeoff() {
					Date = new DateTime(2021, 9, 11),
				};
				
				var writeoffItem = writeoff.AddItem(item.EmployeeIssueOperation, 2);
				writeoff.UpdateOperations(uow);
				uow.Save(writeoff);
				uow.Commit();

				var repository = new EmployeeIssueRepository(uow);
				var result = repository.GetReferencedDocuments(writeoffItem.EmployeeWriteoffOperation.Id);
				Assert.That(result.First().DocumentType, Is.EqualTo(StockDocumentType.WriteoffDoc));
				Assert.That(result.First().DocumentId, Is.EqualTo(writeoff.Id));
				Assert.That(result.First().ItemId, Is.EqualTo(writeoffItem.Id));
			}
		}
		#endregion
		
		#region GetLastOperationsForEmployee
		[Test(Description = "Проверяем запрос на поиск последних операций(happy path).")]
		[Category("Integrated")]
		public void GetLastOperationsForEmployee_HappyPathTest()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {

				var nomenclatureType = new ItemsType {
					Name = "Тестовый тип номенклатуры"
				};
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature {
					Type = nomenclatureType
				};
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools {
					Name = "СИЗ для тестирования"
				};
				protectionTools.AddNomenclature(nomenclature);
				uow.Save(protectionTools);
				
				var protectionTools2 = new ProtectionTools {
					Name = "СИЗ для тестирования 2"
				};
				protectionTools2.AddNomenclature(nomenclature);
				uow.Save(protectionTools2);

				var employee = new EmployeeCard();
				uow.Save(employee);
				
				var employee2 = new EmployeeCard();
				uow.Save(employee2);
				
				//Лишний
				var employee3 = new EmployeeCard();
				uow.Save(employee3);

				//Операция employee1 СИЗ 1
				var opE1P1 = new EmployeeIssueOperation {
					OperationTime = new DateTime(2018, 1, 1, 14, 0, 0),
					AutoWriteoffDate = new DateTime(2020, 1, 1),
					Employee = employee,
					Nomenclature = nomenclature,
					ProtectionTools = protectionTools,
					Issued = 1
				};
				uow.Save(opE1P1);

				var op2E1P1 = new EmployeeIssueOperation {
					OperationTime = new DateTime(2019, 1, 1, 13, 0, 0),
					AutoWriteoffDate = new DateTime(2021, 1, 1),
					Employee = employee,
					Nomenclature = nomenclature,
					ProtectionTools = protectionTools,
					Issued = 1
				};
				uow.Save(op2E1P1);

				var opE1P2 = new EmployeeIssueOperation {
					OperationTime = new DateTime(2021, 1, 1, 13, 0, 0),
					AutoWriteoffDate = new DateTime(2021, 5, 1),
					Employee = employee,
					Nomenclature = nomenclature,
					ProtectionTools = protectionTools2,
					Issued = 1
				};
				uow.Save(opE1P2);
				
				var opE1P2Return = new EmployeeIssueOperation {
					OperationTime = new DateTime(2021, 2, 1, 13, 0, 0),
					AutoWriteoffDate = new DateTime(2021, 5, 1),
					Employee = employee,
					Nomenclature = nomenclature,
					ProtectionTools = protectionTools2,
					Issued = 0,
					Returned = 1
				};
				uow.Save(opE1P2Return);
				
				var opE2P2 = new EmployeeIssueOperation {
					OperationTime = new DateTime(2021, 2, 1, 14, 0, 0),
					AutoWriteoffDate = new DateTime(2021, 5, 1),
					Employee = employee2,
					Nomenclature = nomenclature,
					ProtectionTools = protectionTools2,
					Issued = 1
				};
				uow.Save(opE2P2);

				//Не хотим видеть в выборке от сотрудника 3
				var opE3P2 = new EmployeeIssueOperation {
					OperationTime = new DateTime(2021, 2, 1, 14, 0, 0),
					AutoWriteoffDate = new DateTime(2021, 5, 1),
					Employee = employee3,
					Nomenclature = nomenclature,
					ProtectionTools = protectionTools2,
					Issued = 1
				};
				uow.Save(opE3P2);
				
				uow.Commit();

				var repository = new EmployeeIssueRepository(uow);
				var result = repository.GetLastIssueOperationsForEmployee(new[] { employee, employee2 });
				
				Assert.That(result, Has.No.Member(opE1P1));
				Assert.That(result, Has.Member(op2E1P1));
				Assert.That(result, Has.Member(opE1P2));
				Assert.That(result, Has.No.Member(opE1P2Return));
				Assert.That(result, Has.Member(opE2P2));
				Assert.That(result, Has.No.Member(opE3P2));
			}
		}
		#endregion

		#region ItemsBalance

		[Test(Description = "Проверяем запрос на баланс возвращает корректный результат(happy path).")]
		[Category("Integrated")]
		public void ItemsBalance_SimpleGetBalanceTest()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "СИЗ для тестирования";
				protectionTools.AddNomenclature(nomenclature);
				uow.Save(protectionTools);

				var employee = new EmployeeCard();
				uow.Save(employee);

				//Операция employee1 СИЗ 1
				var opE1P1 = new EmployeeIssueOperation();
				opE1P1.OperationTime = new DateTime(2018, 1, 1, 14, 0, 0);
				opE1P1.AutoWriteoffDate = new DateTime(2020, 1, 1);
				opE1P1.Employee = employee;
				opE1P1.Nomenclature = nomenclature;
				opE1P1.ProtectionTools = protectionTools;
				opE1P1.Issued = 15;
				uow.Save(opE1P1);

				uow.Commit();

				var repository = new EmployeeIssueRepository(uow);
				var result = repository.ItemsBalance( employee, new DateTime(2019, 1, 1));
				
				Assert.That(result, Has.Count.EqualTo(1));
				Assert.That(result.First().Amount, Is.EqualTo(15));
			}
		}
		
		[Test(Description = "Проверяем запрос на баланса не возвращает числящейся выдачу в будущем(реальный баг) или в прошлом.")]
		[Category("Integrated")]
		public void ItemsBalance_InFeatureTest()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "СИЗ для тестирования";
				protectionTools.AddNomenclature(nomenclature);
				uow.Save(protectionTools);

				var employee = new EmployeeCard();
				uow.Save(employee);

				//Операция employee1 СИЗ 1
				var opE1P1 = new EmployeeIssueOperation();
				opE1P1.OperationTime = new DateTime(2018, 1, 1, 14, 0, 0);
				opE1P1.AutoWriteoffDate = new DateTime(2020, 1, 1);
				opE1P1.Employee = employee;
				opE1P1.Nomenclature = nomenclature;
				opE1P1.ProtectionTools = protectionTools;
				opE1P1.Issued = 15;
				uow.Save(opE1P1);
				
				//Операция employee1 СИЗ 1
				var op2E1P1 = new EmployeeIssueOperation();
				op2E1P1.OperationTime = new DateTime(2023, 1, 1, 14, 0, 0);
				op2E1P1.AutoWriteoffDate = new DateTime(2023, 1, 1);
				op2E1P1.Employee = employee;
				op2E1P1.Nomenclature = nomenclature;
				op2E1P1.ProtectionTools = protectionTools;
				op2E1P1.Issued = 20;
				uow.Save(op2E1P1);

				uow.Commit();

				var repository = new EmployeeIssueRepository(uow);
				var result = repository.ItemsBalance( employee, new DateTime(2022, 5, 1));
				
				Assert.That(result, Has.Count.EqualTo(1).Or.EqualTo(0).Or.Empty);
				if(result.Any())
					Assert.That(result.First().Amount, Is.EqualTo(0));
			}
		}
		
		[Test(Description = "Проверяем запрос на баланса корректно считает списанное.")]
		[Category("Integrated")]
		public void ItemsBalance_ManualWriteoffTest()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "СИЗ для тестирования";
				protectionTools.AddNomenclature(nomenclature);
				uow.Save(protectionTools);

				var employee = new EmployeeCard();
				uow.Save(employee);

				//Операция employee1 СИЗ 1
				var opE1P1 = new EmployeeIssueOperation();
				opE1P1.OperationTime = new DateTime(2018, 1, 1, 14, 0, 0);
				opE1P1.AutoWriteoffDate = new DateTime(2020, 1, 1);
				opE1P1.Employee = employee;
				opE1P1.Nomenclature = nomenclature;
				opE1P1.ProtectionTools = protectionTools;
				opE1P1.Issued = 15;
				uow.Save(opE1P1);
				
				//Операция employee1 СИЗ 1 Списание 5
				var opE1P1W1 = new EmployeeIssueOperation();
				opE1P1W1.OperationTime = new DateTime(2019, 1, 1, 14, 0, 0);
				opE1P1W1.Employee = employee;
				opE1P1W1.Nomenclature = nomenclature;
				opE1P1W1.ProtectionTools = protectionTools;
				opE1P1W1.IssuedOperation = opE1P1;
				opE1P1W1.Returned = 5;
				uow.Save(opE1P1W1);
				
				//Операция employee1 СИЗ 1 Списание 2
				var opE1P1W2 = new EmployeeIssueOperation();
				opE1P1W2.OperationTime = new DateTime(2019, 2, 1, 14, 0, 0);
				opE1P1W2.Employee = employee;
				opE1P1W2.Nomenclature = nomenclature;
				opE1P1W2.ProtectionTools = protectionTools;
				opE1P1W2.IssuedOperation = opE1P1;
				opE1P1W2.Returned = 2;
				uow.Save(opE1P1W2);
				
				//Операция employee1 СИЗ 1 Списание 4 уже после даты баланса...
				var opE1P1W3 = new EmployeeIssueOperation();
				opE1P1W3.OperationTime = new DateTime(2019, 7, 1, 14, 0, 0);
				opE1P1W3.Employee = employee;
				opE1P1W3.Nomenclature = nomenclature;
				opE1P1W3.ProtectionTools = protectionTools;
				opE1P1W3.IssuedOperation = opE1P1;
				opE1P1W3.Returned = 4;
				uow.Save(opE1P1W3);

				uow.Commit();

				var repository = new EmployeeIssueRepository(uow);
				var result = repository.ItemsBalance( employee, new DateTime(2019, 5, 1));
				
				Assert.That(result, Has.Count.EqualTo(1));
				Assert.That(result.First().Amount, Is.EqualTo(8));
			}
		}

		#endregion
	}
}
