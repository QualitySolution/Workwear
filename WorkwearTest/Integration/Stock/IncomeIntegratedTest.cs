using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Testing.DB;
using workwear;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;

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

	}
}
