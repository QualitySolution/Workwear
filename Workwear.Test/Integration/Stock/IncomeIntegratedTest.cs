﻿using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Testing.DB;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Models.Operations;
using Workwear.Repository.Operations;
using Workwear.Repository.Stock;
using Workwear.Tools;

namespace Workwear.Test.Integration.Stock
{
	[TestFixture(TestOf = typeof(Expense))]
	public class IncomeIntegratedTest : InMemoryDBGlobalConfigTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init() {
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
		}
		[SetUp]
		public void TestSetup() { }
		[TearDown]
		public void TestTearDown() { }

		[Test(Description = "Проверяем что можем приходовать на склад 2 позиции разных размеров одной номенклатуры.")]
		[Category("Integrated")]
		public void CanAddMultiRowWithSameNomenclatureTest()
		{
			var ask = Substitute.For<IInteractiveService>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var sizeType = new SizeType();
				uow.Save(sizeType);

				var nomenclatureType = new ItemsType {Name = "Тестовый тип номенклатуры", SizeType = sizeType};
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature {Type = nomenclatureType};
				uow.Save(nomenclature);

				var sizeX = new Size {Name = "X", SizeType = sizeType};
				var sizeXl = new Size {Name = "XL", SizeType = sizeType};
				uow.Save(sizeX);
				uow.Save(sizeXl);

				var income = new Income {
					Warehouse = warehouse,
					Date = new DateTime(2017, 1, 1)
				};
				var incomeItem1 = income.AddItem(nomenclature, ask);
				incomeItem1.WearSize = sizeX;
				incomeItem1.Amount = 10;
				var incomeItem2 = income.AddItem(nomenclature, ask);
				incomeItem2.WearSize = sizeXl;
				incomeItem2.Amount = 5;
				income.UpdateOperations(uow);
				var validator = new QS.Validation.ObjectValidator();
				Assert.That(validator.Validate(income), Is.True);
				uow.Save(income);
				uow.Commit();

				var stock = new StockRepository()
					.StockBalances(uow, warehouse, new List<Nomenclature> { nomenclature }, new DateTime(2017, 1,2));
				Assert.That(stock.Count, Is.EqualTo(2));
			}
		}

		[Test(Description = "Проверяем процент износа не теряется при сохранении.")]
		[Category("Integrated")]
		public void Income_ItemWearPercent_SaveInStockTest()
		{
			var ask = Substitute.For<IInteractiveService>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);
				
				var sizeType = new SizeType();
				uow.Save(sizeType);

				var nomenclatureType = new ItemsType {
					Name = "Тестовый тип номенклатуры",
					SizeType = sizeType
				};
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature {
					Type = nomenclatureType
				};
				uow.Save(nomenclature);
				
				var sizeX = new Size {Name = "X", SizeType = sizeType};
				uow.Save(sizeX);

				var income = new Income {
					Warehouse = warehouse,
					Date = new DateTime(2017, 1, 1)
				};
				var incomeItem1 = income.AddItem(nomenclature, ask);
				incomeItem1.WearSize = sizeX;
				incomeItem1.WearPercent = 0.8m;
				incomeItem1.Amount = 10;
				income.UpdateOperations(uow);
				var validator = new QS.Validation.ObjectValidator();
				Assert.That(validator.Validate(income), Is.True);
				uow.Save(income);
				uow.Commit();

				var stock = new StockRepository()
					.StockBalances(uow, warehouse, new List<Nomenclature> { nomenclature }, DateTime.Now);
				var stockItem = stock.First();
				Assert.That(stockItem.Amount, Is.EqualTo(10));
				Assert.That(stockItem.WearPercent, Is.EqualTo(0.8m));
			}
		}

		#region Возврат от сотрудника

		[Test(Description = "Убеждаемся что корректно рассчитываем дату следующей выдачи при возврате выданного на склад. " +
		                    "При разных id. Реальный баг был в том что проверялись id не тех сущностей, " +
		                    "но вы тестах id одинаковые, поэтому тесты работали..")]
		[Category("real case")]
		[Category("Integrated")]
		public void UpdateEmployeeWearItems_NextIssueDiffIdsTest()
		{
			NewSessionWithSameDB();
			var ask = Substitute.For<IInteractiveService>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var nomenclatureType = new ItemsType {
					Name = "Тестовый тип номенклатуры"
				};
				uow.Save(nomenclatureType);
				
				var nomenclature = new Nomenclature {
					Type = nomenclatureType,
					Name = "Тестовая номенклатура"
				};
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools {
					Name = "СИЗ для тестирования"
				};
				protectionTools.AddNomenclature(nomenclature);
				uow.Save(protectionTools);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Month;
				normItem.PeriodCount = 1;
				uow.Save(norm);

				var employee = new EmployeeCard();
				employee.AddUsedNorm(norm);
				uow.Save(employee);
				uow.Commit();

				var baseParameters = Substitute.For<BaseParameters>();
				baseParameters.ColDayAheadOfShedule.Returns(0);
				
				var uowProvider = new UnitOfWorkProvider(uow);
				var issueModel = new EmployeeIssueModel(new EmployeeIssueRepository(uowProvider), uowProvider);
				employee.FillWearReceivedInfo(new EmployeeIssueRepository(uowProvider));
				var stockModel = new StockBalanceModel(uowProvider, new StockRepository());
				stockModel.Warehouse = warehouse;
				stockModel.OnDate = new DateTime(2018, 10, 22);
				issueModel.FillWearInStockInfo(employee, stockModel);
				
				var expense = new Expense {
					Warehouse = warehouse,
					Employee = employee,
					Date = new DateTime(2018, 10, 22),
					IssueDate = new DateTime(2018, 10, 22)
				};
				var itemExpense = expense.AddItem(employee.WorkwearItems.First(), baseParameters);
				itemExpense.Nomenclature = nomenclature;
				itemExpense.Amount = 1;
				
				//Обновление операций
				expense.UpdateOperations(uow, baseParameters, ask);
				uow.Save(expense);
				uow.Commit();

				expense.UpdateEmployeeWearItems();
				uow.Commit();

				using(var uow2 = UnitOfWorkFactory.CreateWithoutRoot()) {
					var employeeTest = uow2.GetById<EmployeeCard>(employee.Id);
					Assert.That(employeeTest.WorkwearItems[0].NextIssue, Is.EqualTo(new DateTime(2018, 11, 22)));
				}

				var returnOnStock = new Return() {
					Warehouse = warehouse,
					Date = new DateTime(2018, 11, 2)
				};
				returnOnStock.AddItem(expense.Items.First().EmployeeIssueOperation, 1);
				returnOnStock.UpdateOperations(uow);
				uow.Save(returnOnStock);
				uow.Commit();

				returnOnStock.UpdateEmployeeWearItems(uow);
				uow.Commit();

				using(var uow2 = UnitOfWorkFactory.CreateWithoutRoot()) {
					var employeeTest = uow2.GetById<EmployeeCard>(employee.Id);
					Assert.That(employeeTest.WorkwearItems[0].NextIssue, Is.EqualTo(new DateTime(2018, 11, 2)));
				}
			}
		}

		#endregion

		#region Проверка влияния на складские остатки

		[Test(Description = "Проверяем что на складе появляются отдельные остатки по двум складам.")]
		[Category("Integrated")]
		public void IncomeOnTwoWarehousesTest()
		{
			var ask = Substitute.For<IInteractiveService>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var warehouse2 = new Warehouse();
				uow.Save(warehouse2);
				
				var sizeType = new SizeType();
				uow.Save(sizeType);

				var nomenclatureType = new ItemsType {
					Name = "Тестовый тип номенклатуры",
					SizeType = sizeType
				};
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature {
					Type = nomenclatureType
				};
				uow.Save(nomenclature);
				
				var sizeX = new Size {Name = "X", SizeType = sizeType};
				uow.Save(sizeX);

				var income1 = new Income {
					Warehouse = warehouse,
					Date = new DateTime(2017, 1, 1)
				};
				var incomeItem1 = income1.AddItem(nomenclature, ask);
				incomeItem1.WearSize = sizeX;
				incomeItem1.Amount = 10;
				var incomeItem2 = income1.AddItem(nomenclature, ask);
				incomeItem2.WearSize = sizeX;
				incomeItem2.Amount = 5;
				income1.UpdateOperations(uow);
				uow.Save(income1);

				var income2 = new Income {
					Warehouse = warehouse2,
					Date = new DateTime(2017, 1, 1)
				};
				var income2Item1 = income2.AddItem(nomenclature, ask);
				income2Item1.WearSize = sizeX;
				income2Item1.Amount = 7;
				income2.UpdateOperations(uow);
				uow.Save(income2);
				
				uow.Commit();
				var stockRepository = new StockRepository();
				var stock1 = stockRepository.StockBalances(uow, warehouse, new List<Nomenclature> { nomenclature }, new DateTime(2017, 1, 2));
				Assert.That(stock1.Sum(x => x.Amount), Is.EqualTo(15));
				var stock2 = stockRepository.StockBalances(uow, warehouse2, new List<Nomenclature> { nomenclature }, new DateTime(2017, 1, 2));
				Assert.That(stock2.Sum(x => x.Amount), Is.EqualTo(7));
			}
		}

		[Test(Description = "Проверяем что при удалении строки из приходной накладной изменяются остатки на складе.")]
		[Category("Integrated")]
		public void UpdateIncomeTest()
		{
			var ask = Substitute.For<IInteractiveService>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {

				var warehouse = new Warehouse { Name = "TestWarehouse" };
				uow.Save(warehouse);

				var income = new Income {
					Warehouse = warehouse,
					Date = new DateTime(2017, 1, 1)
				};
				var itemType = new ItemsType{Name = "Тестовый тип"};
				uow.Save(itemType);
				var nomenclature1 = new Nomenclature { Name = "TestNomenclature1", Type = itemType};
				uow.Save(nomenclature1);
				var nomenclature2 = new Nomenclature { Name = "TestNomenclature2", Type = itemType};
				uow.Save(nomenclature2);
				var item1 = income.AddItem(nomenclature1, ask);
				var item2 = income.AddItem(nomenclature2, ask);

				income.UpdateOperations(uow);
				uow.Save(income);
				uow.Commit();

				var stockRepository = new StockRepository();
				var stock1 = stockRepository.StockBalances(uow, warehouse, 
											new List<Nomenclature> { nomenclature1, nomenclature2}, 
											new DateTime(2017, 1,2));
				Assert.That(stock1.Count(), Is.EqualTo(2));

				income.RemoveItem(item1);
				income.UpdateOperations(uow);
				uow.Save(income);
				uow.Commit();

				var stock2 = stockRepository.StockBalances(uow, warehouse,
											new List<Nomenclature> { nomenclature1, nomenclature2 },
											new DateTime(2017, 1, 2));

				Assert.That(stock2.Count(), Is.EqualTo(1));
			}
		}


		#endregion
	}
}
