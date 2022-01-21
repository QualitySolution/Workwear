using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Testing.DB;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Repository.Stock;
using workwear.Tools;

namespace WorkwearTest.Integration.Stock
{
	[TestFixture(TestOf = typeof(Expense))]
	public class IncomeIntegratedTest : InMemoryDBGlobalConfigTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init()
		{
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
		}

		[SetUp]
		public void TestSetup()
		{
		}

		[TearDown]
		public void TestTearDown()
		{
		}

		[Test(Description = "Проверяем что можем приходовать на склад 2 позиции разных размеров одной номеклатуры.")]
		[Category("Integrated")]
		public void CanAddMultiRowWithSameNomenclatureTest()
		{
			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				uow.Save(nomenclature);

				var income = new Income();
				income.Warehouse = warehouse;
				income.Date = new DateTime(2017, 1, 1);
				income.Operation = IncomeOperations.Enter;
				var incomeItem1 = income.AddItem(nomenclature);
				incomeItem1.Size = "X";
				incomeItem1.Amount = 10;
				var incomeItem2 = income.AddItem(nomenclature);
				incomeItem2.Size = "XL";
				incomeItem2.Amount = 5;
				income.UpdateOperations(uow, ask);
				var valadator = new QS.Validation.ObjectValidator();
				Assert.That(valadator.Validate(income), Is.True);
				uow.Save(income);
				uow.Commit();

				var stock = new StockRepository().StockBalances(uow, warehouse, new List<Nomenclature> { nomenclature }, DateTime.Now);
				Assert.That(stock.Count, Is.EqualTo(2));
			}
		}

		[Test(Description = "Проверяем что не считаем документ валидным если в нем несколько раз приходуется одинаковая складская позинция.")]
		[Category("Integrated")]
		public void NotValidMultiRowWithSameStockPositionTest()
		{
			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				uow.Save(nomenclature);

				var income = new Income();
				income.Warehouse = warehouse;
				income.Date = new DateTime(2017, 1, 1);
				income.Operation = IncomeOperations.Enter;
				var incomeItem1 = income.AddItem(nomenclature);
				incomeItem1.Size = "X";
				incomeItem1.Amount = 10;
				var incomeItem2 = income.AddItem(nomenclature);
				incomeItem2.Size = "X";
				incomeItem2.Amount = 5;
				income.UpdateOperations(uow, ask);
				var valadator = new QS.Validation.ObjectValidator();
				Assert.That(valadator.Validate(income), Is.False);
			}
		}

		[Test(Description = "Проверяем процент износа не теряется при сохранении.")]
		[Category("Integrated")]
		public void Income_ItemWearPercent_SaveInStockTest()
		{
			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				uow.Save(nomenclature);

				var income = new Income();
				income.Warehouse = warehouse;
				income.Date = new DateTime(2017, 1, 1);
				income.Operation = IncomeOperations.Enter;
				var incomeItem1 = income.AddItem(nomenclature);
				incomeItem1.Size = "X";
				incomeItem1.WearPercent = 0.8m;
				incomeItem1.Amount = 10;
				income.UpdateOperations(uow, ask);
				var valadator = new QS.Validation.ObjectValidator();
				Assert.That(valadator.Validate(income), Is.True);
				uow.Save(income);
				uow.Commit();

				var stock = new StockRepository().StockBalances(uow, warehouse, new List<Nomenclature> { nomenclature }, DateTime.Now);
				var stockItem = stock.First();
				Assert.That(stockItem.Amount, Is.EqualTo(10));
				Assert.That(stockItem.WearPercent, Is.EqualTo(0.8m));
			}
		}

		#region Возврат от сотрудника

		[Test(Description = "Убеждаемся что корректно рассчитываем дату следущей выдачи при возврате выданного на склад. При разных id. Реальный баг был втом что проверялись id не тех сущьностей, но вы тестах id одинаковые, поэтому тесты работали..")]
		[Category("real case")]
		[Category("Integrated")]
		public void UpdateEmployeeWearItems_NextIssueDiffIdsTest()
		{
			NewSessionWithSameDB();
			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				uow.Save(nomenclatureType);

				//Поднимаем id номеклатуры до 2.
				uow.Save(new Nomenclature());

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				uow.Save(nomenclature);

				var position1 = new StockPosition(nomenclature, null, null, 0);

				//Поднимаем id сиза до 3.
				uow.Save(new ProtectionTools { Name = "Id = 1" });
				uow.Save(new ProtectionTools { Name = "Id = 2" });

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "СИЗ для тестирования";
				protectionTools.AddNomeclature(nomenclature);
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

				var income = new Income();
				income.Warehouse = warehouse;
				income.Date = new DateTime(2017, 1, 1);
				income.Operation = IncomeOperations.Enter;
				var incomeItem1 = income.AddItem(nomenclature);
				incomeItem1.Amount = 10;
				income.UpdateOperations(uow, ask);
				uow.Save(income);

				var expense = new Expense();
				expense.Operation = ExpenseOperations.Employee;
				expense.Warehouse = warehouse;
				expense.Employee = employee;
				expense.Date = new DateTime(2018, 10, 22);
				expense.AddItem(position1, 1);

				var baseParameters = Substitute.For<BaseParameters>();
				baseParameters.ColDayAheadOfShedule.Returns(0);

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

				var returnOnStock = new Income();
				returnOnStock.Operation = IncomeOperations.Return;
				returnOnStock.Warehouse = warehouse;
				returnOnStock.EmployeeCard = employee;
				returnOnStock.Date = new DateTime(2018, 11, 2);
				returnOnStock.AddItem(expense.Items.First().EmployeeIssueOperation, 1);
				returnOnStock.UpdateOperations(uow, ask);
				uow.Save(returnOnStock);
				returnOnStock.UpdateEmployeeWearItems();
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
			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var warehouse2 = new Warehouse();
				uow.Save(warehouse2);

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				uow.Save(nomenclature);

				var income1 = new Income();
				income1.Warehouse = warehouse;
				income1.Date = new DateTime(2017, 1, 1);
				income1.Operation = IncomeOperations.Enter;
				var incomeItem1 = income1.AddItem(nomenclature);
				incomeItem1.Size = "X";
				incomeItem1.Amount = 10;
				var incomeItem2 = income1.AddItem(nomenclature);
				incomeItem2.Size = "X";
				incomeItem2.Amount = 5;
				income1.UpdateOperations(uow, ask);
				uow.Save(income1);

				var income2 = new Income();
				income2.Warehouse = warehouse2;
				income2.Date = new DateTime(2017, 1, 1);
				income2.Operation = IncomeOperations.Enter;
				var income2Item1 = income2.AddItem(nomenclature);
				income2Item1.Size = "X";
				income2Item1.Amount = 7;
				income2.UpdateOperations(uow, ask);
				uow.Save(income2);

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
			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {

				var Warehouse = new Warehouse() { Name = "TestWarehouse" };
				uow.Save(Warehouse);

				var Income = new Income();
				Income.Warehouse = Warehouse;
				Income.Date = new DateTime(2017, 1, 1);
				Income.Operation = IncomeOperations.Enter;
				var Nomenclature1 = new Nomenclature() { Name = "TestNomenclature1" };
				uow.Save(Nomenclature1);
				var Nomenclature2 = new Nomenclature() { Name = "TestNomenclature2" };
				uow.Save(Nomenclature2);
				var item1 = Income.AddItem(Nomenclature1);
				var item2 = Income.AddItem(Nomenclature2);

				Income.UpdateOperations(uow, ask);
				uow.Save(Income);

				var stockRepository = new StockRepository();
				var stock1 = stockRepository.StockBalances(uow, Warehouse, 
											new List<Nomenclature> { Nomenclature1, Nomenclature2}, 
											new DateTime(2017, 1,2));
				Assert.That(stock1.Count(), Is.EqualTo(2));

				Income.ObservableItems.Remove(item1);
				Income.UpdateOperations(uow, ask);
				uow.Save(Income);

				var stock2 = stockRepository.StockBalances(uow, Warehouse,
											new List<Nomenclature> { Nomenclature1, Nomenclature2 },
											new DateTime(2017, 1, 2));
				Assert.That(stock1.Count(), Is.EqualTo(1));

			}
		}


		#endregion
	}
}
