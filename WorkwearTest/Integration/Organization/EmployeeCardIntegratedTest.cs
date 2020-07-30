using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Testing.DB;
using workwear;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;

namespace WorkwearTest.Integration.Organization
{
	[TestFixture(TestOf = typeof(EmployeeCard), Description = "Карточка сотрудника")]
	public class EmployeeCardIntegratedTest : InMemoryDBGlobalConfigTestFixtureBase
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
		[Test(Description = "Корректно и правильно выводить всю доступную подходящую номенклатуру.")]
		[Category("real case")]
		[Category("Integrated")]

		public void BestChoise_IssuingMultipleRows_TwoNomeclatureSameNeedsTest()
		{
			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				nomenclatureType.Category = ItemTypeCategory.wear;
				nomenclatureType.WearCategory = workwear.Measurements.СlothesType.PPE;
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				uow.Save(nomenclature);

				var position1 = new StockPosition(nomenclature, null, null, 0);

				var nomenclature2 = new Nomenclature();
				nomenclature2.Type = nomenclatureType;
				uow.Save(nomenclature2);

				var position2 = new StockPosition(nomenclature2, null, null, 0);

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "Номенклатура ТОН";
				protectionTools.AddNomeclature(nomenclature);
				protectionTools.AddNomeclature(nomenclature2);
				uow.Save(protectionTools);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Year;
				normItem.PeriodCount = 1;
				uow.Save(norm);

				var employee = new EmployeeCard();
				employee.AddUsedNorm(norm);
				Assert.That(employee.WorkwearItems.Count, Is.GreaterThan(0));
				uow.Save(employee);
				uow.Commit();

				var income = new Income();
				income.Warehouse = warehouse;
				income.Date = new DateTime(2020, 07, 20);
				income.Operation = IncomeOperations.Enter;
				var incomeItem1 = income.AddItem(nomenclature);
				incomeItem1.Amount = 10;
				var incomeItem2 = income.AddItem(nomenclature2);
				incomeItem2.Amount = 5;
				income.UpdateOperations(uow, ask);
				uow.Save(income);
				Assert.That(uow.GetAll<WarehouseOperation>().Count(), Is.EqualTo(2));

				var operationTime = uow.GetAll<WarehouseOperation>().Select(x=> x.OperationTime).ToList();

				StockBalanceDTO stockBalance = null; 
				employee.FillWearInStockInfo(uow, warehouse, new DateTime(2020, 07, 22), false);
				Assert.That(employee.UnderreceivedItems.Count(), Is.GreaterThan(0));
				var employeeCardItem = employee.UnderreceivedItems.First();
				Assert.That(employeeCardItem.BestChoiceInStock.Count(), Is.GreaterThan(0));

				var bestChoice = employeeCardItem.BestChoiceInStock.First();

				Assert.That(bestChoice.Nomenclature, Is.EqualTo(nomenclature));
			}


			}

			public void BestChoise_IssuingMultipleRows_TwoNomeclatureWearTypeSameNeedsTest()
		{
			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				nomenclatureType.Category = ItemTypeCategory.wear;
				nomenclatureType.WearCategory = workwear.Measurements.СlothesType.Wear;
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				nomenclature.Sex = workwear.Measurements.ClothesSex.Men;
				nomenclature.SizeStd = "UnisexWearRus";
				nomenclature.WearGrowthStd = "UnisexGrowth";
				uow.Save(nomenclature);

				var nomenclature2 = new Nomenclature();
				nomenclature2.Type = nomenclatureType;
				nomenclature2.Sex = workwear.Measurements.ClothesSex.Men;
				nomenclature2.SizeStd = "UnisexWearRus";
				nomenclature2.WearGrowthStd = "UnisexGrowth";
				uow.Save(nomenclature2);

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "Номенклатура ТОН";
				protectionTools.AddNomeclature(nomenclature);
				protectionTools.AddNomeclature(nomenclature2);
				uow.Save(protectionTools);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Year;
				normItem.PeriodCount = 1;
				uow.Save(norm);

				var employee = new EmployeeCard();
				employee.AddUsedNorm(norm);
				employee.Sex = Sex.M;
				employee.WearSizeStd = "MenWearRus";
				employee.WearSize = "50";
				employee.WearGrowth = "176";
				Assert.That(employee.WorkwearItems.Count, Is.GreaterThan(0));
				uow.Save(employee);
				uow.Commit();

				var income = new Income();
				income.Warehouse = warehouse;
				income.Date = new DateTime(2020, 07, 20);
				
				income.Operation = IncomeOperations.Enter;
				
				var incomeItem1 = income.AddItem(nomenclature);
				
				incomeItem1.Amount = 10;
				incomeItem1.Size = "50";
				incomeItem1.WearGrowth = "176";
				var incomeItem2 = income.AddItem(nomenclature2);
				incomeItem2.Amount = 5;
				incomeItem2.Size = "50";
				incomeItem2.WearGrowth = "176";

				income.UpdateOperations(uow, ask);
				uow.Save(income);
				Assert.That(uow.GetAll<WarehouseOperation>().Count(), Is.EqualTo(2));

				employee.FillWearInStockInfo(uow, warehouse, new DateTime(2020, 07, 22), false);
				Assert.That(employee.UnderreceivedItems.Count(), Is.GreaterThan(0));
				var employeeCardItem = employee.UnderreceivedItems.First();
				var employeeCardItemCount = employee.UnderreceivedItems.Count();
				 
				var inStock = employeeCardItem.InStock;

				Assert.That(employeeCardItem.InStock.Count(), Is.GreaterThan(0));
				var bestChoiceCount = employeeCardItem.BestChoiceInStock.Count();
				Assert.That(employeeCardItem.BestChoiceInStock.Count(), Is.GreaterThan(0));

				var bestChoice = employeeCardItem.BestChoiceInStock.First();

				Assert.That(bestChoice.Nomenclature, Is.EqualTo(nomenclature));
			}
		}

		public void BestChoise_IssuingMultipleRows_TwoNomeclatureShoesTypeSameNeedsTest()
		{
			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Обувь";
				nomenclatureType.Category = ItemTypeCategory.wear;
				nomenclatureType.WearCategory = workwear.Measurements.СlothesType.Shoes;
				uow.Save(nomenclatureType);

				var nomenclatureType2 = new ItemsType();
				nomenclatureType2.Name = "Зимняя обувь";
				nomenclatureType2.Category = ItemTypeCategory.wear;
				nomenclatureType2.WearCategory = workwear.Measurements.СlothesType.WinterShoes;
				uow.Save(nomenclatureType2);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				nomenclature.Sex = workwear.Measurements.ClothesSex.Men;
				nomenclature.SizeStd = "MenShoesRus";
				uow.Save(nomenclature);

				var nomenclature2 = new Nomenclature();
				nomenclature2.Type = nomenclatureType2;
				nomenclature2.Sex = workwear.Measurements.ClothesSex.Men;
				nomenclature2.SizeStd = "MenShoesRus";
				uow.Save(nomenclature2);

				var nomenclature3 = new Nomenclature();
				nomenclature3.Type = nomenclatureType;
				nomenclature3.Sex = workwear.Measurements.ClothesSex.Men;
				nomenclature3.SizeStd = "MenShoesRus";
				uow.Save(nomenclature3);

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "Номенклатура ТОН";
				protectionTools.AddNomeclature(nomenclature);
				protectionTools.AddNomeclature(nomenclature3);

				uow.Save(protectionTools);

				var protectionTools2 = new ProtectionTools();
				protectionTools2.Name = "Номенклатура ТОН_2";
				protectionTools2.AddNomeclature(nomenclature2);
				uow.Save(protectionTools2);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Year;
				normItem.PeriodCount = 1;

				var normItem2 = norm.AddItem(protectionTools2);
				normItem2.Amount = 1;
				normItem2.NormPeriod = NormPeriodType.Year;
				normItem2.PeriodCount = 1;
				uow.Save(norm);

				var employee = new EmployeeCard();
				employee.AddUsedNorm(norm);
				employee.Sex = Sex.M;
				employee.WearSizeStd = "MenWearRus";
				employee.ShoesSizeStd = "MenShoesRus";
				employee.WinterShoesSizeStd = "MenShoesRus";
				employee.ShoesSize = "42";
				employee.WinterShoesSize = "43";
				Assert.That(employee.WorkwearItems.Count, Is.GreaterThan(0));
				uow.Save(employee);
				uow.Commit();

				var income = new Income();
				income.Warehouse = warehouse;
				income.Date = new DateTime(2020, 07, 20);

				income.Operation = IncomeOperations.Enter;

				var incomeItem1 = income.AddItem(nomenclature);

				incomeItem1.Amount = 1;
				incomeItem1.Size = "42";
				
				var incomeItem2 = income.AddItem(nomenclature2);
				incomeItem2.Amount = 2;
				incomeItem2.Size = "43";

				var incomeItem3 = income.AddItem(nomenclature3);
				incomeItem3.Amount = 3;
				incomeItem3.Size = "42";

				income.UpdateOperations(uow, ask);
				uow.Save(income);
				Assert.That(uow.GetAll<WarehouseOperation>().Count(), Is.EqualTo(3));

				employee.FillWearInStockInfo(uow, warehouse, new DateTime(2020, 07, 22), false);
				Assert.That(employee.UnderreceivedItems.Count(), Is.GreaterThan(0));

				Assert.That(employee.UnderreceivedItems.Count(), Is.EqualTo(2));

				var employeeCardItem = employee.UnderreceivedItems.First();

				Assert.That(employeeCardItem.InStock.Count(), Is.GreaterThan(0));

				var bestChoiceInStock = employeeCardItem.BestChoiceInStock;
				var bestChoiceCount = employeeCardItem.BestChoiceInStock.Count();
				Assert.That(employeeCardItem.BestChoiceInStock.Count(), Is.GreaterThan(0));

				Assert.That(employeeCardItem.BestChoiceInStock.Count(), Is.EqualTo(2));
			}
		}
	}
}
