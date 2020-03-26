using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using QS.Testing.DB;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;

namespace WorkwearTest.Stock
{
	[TestFixture(TestOf = typeof(MassExpense))]
	public class MassExpenseTest : InMemoryDBGlobalConfigTestFixtureBase
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

		[Test(Description = "Проверка правильного подсчета остатков на складе с учетом необходимой выдачи " +
							"и вывода верного сообщения о недостатке номенклатуры на складе. Тест второй")]
		public void ValidateNomenclature_TrueCalculateStockBalanseAndWriteRightDisplayMessage2()
		{

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {

				Warehouse warehouse = new Warehouse();
				warehouse.Name = "Тестовый склад";

				uow.Save(warehouse);

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Верхняя одежда";
				nomenclatureType.Category = ItemTypeCategory.wear;
				nomenclatureType.Name = "Халат х/б";
				nomenclatureType.WearCategory = workwear.Measurements.СlothesType.Wear;

				uow.Save(nomenclatureType);

				var nomenclatureType2 = new ItemsType();
				nomenclatureType2.Name = "Головной убор";
				nomenclatureType2.Category = ItemTypeCategory.wear;
				nomenclatureType2.Name = "Шапка";
				nomenclatureType2.WearCategory = workwear.Measurements.СlothesType.Headgear;

				uow.Save(nomenclatureType2);

				Nomenclature nomenclBathrobe = new Nomenclature();
				nomenclBathrobe.Name = "Халат х/б синий";
				nomenclBathrobe.Sex = workwear.Measurements.ClothesSex.Universal;
				nomenclBathrobe.SizeStd = "UnisexWearRus";
				nomenclBathrobe.WearGrowthStd = "UnisexGrowth";
				nomenclBathrobe.Type = nomenclatureType;

				uow.Save(nomenclBathrobe);

				Nomenclature nomenclHat = new Nomenclature();
				nomenclHat.Name = "шапка";
				nomenclHat.Sex = workwear.Measurements.ClothesSex.Universal;
				nomenclHat.SizeStd = "HeaddressRus";
				nomenclHat.Type = nomenclatureType2;

				uow.Save(nomenclHat);
				uow.Commit();

				WarehouseOperation warehouseOperation = new WarehouseOperation();
				warehouseOperation.Nomenclature = nomenclBathrobe;
				warehouseOperation.ReceiptWarehouse = warehouse;
				warehouseOperation.Amount = 4; // Количество халатов на складе
				warehouseOperation.Cost = 100;
				warehouseOperation.Size = "48-50";

				uow.Save(warehouseOperation);


				WarehouseOperation warehouseOperation2 = new WarehouseOperation();
				warehouseOperation2.Nomenclature = nomenclHat;
				warehouseOperation2.ReceiptWarehouse = warehouse;
				warehouseOperation2.Amount = 2; // Количество шапок на складе
				warehouseOperation2.Cost = 15;
				warehouseOperation2.Size = "56";

				uow.Save(warehouseOperation2);
				uow.Commit();

				EmployeeCard empCard = new EmployeeCard();
				empCard.FirstName = "FirstName";
				empCard.LastName = "LastName";
				empCard.Sex = Sex.M;
				empCard.WearSizeStd = "MenWearRus";
				empCard.WearSize = "48-50";
				empCard.HeaddressSizeStd = "HeaddressRus";
				empCard.HeaddressSize = "56";

				EmployeeCard empCard2 = new EmployeeCard();
				empCard2.FirstName = "FirstName2";
				empCard2.LastName = "LastName2";
				empCard2.Sex = Sex.M;
				empCard2.WearSizeStd = "MenWearRus";
				empCard2.WearSize = "50";
				empCard2.HeaddressSizeStd = "HeaddressRus";
				empCard2.HeaddressSize = "56";

				MassExpenseEmployee masEmployee = new MassExpenseEmployee();
				masEmployee.EmployeeCard = empCard;
				masEmployee.Sex = empCard.Sex;
				masEmployee.WearSizeStd = empCard.WearSizeStd;
				masEmployee.WearSize = empCard.WearSize;
				masEmployee.HeaddressSizeStd = empCard.HeaddressSizeStd;
				masEmployee.HeaddressSize = empCard.HeaddressSize;

				MassExpenseEmployee masEmployee2 = new MassExpenseEmployee();
				masEmployee2.EmployeeCard = empCard2;
				masEmployee2.Sex = empCard2.Sex;
				masEmployee2.WearSizeStd = empCard2.WearSizeStd;
				masEmployee2.WearSize = empCard2.WearSize;
				masEmployee2.HeaddressSizeStd = empCard2.HeaddressSizeStd;
				masEmployee2.HeaddressSize = empCard2.HeaddressSize;

				List<MassExpenseEmployee> listMassemployee = new List<MassExpenseEmployee> { masEmployee, masEmployee2 };

				MassExpenseNomenclature massNomencl = new MassExpenseNomenclature();
				massNomencl.Nomenclature = nomenclBathrobe;
				massNomencl.Amount = 3; //Количество халатов на каждого сотрудника

				MassExpenseNomenclature massNomencl2 = new MassExpenseNomenclature();
				massNomencl2.Nomenclature = nomenclHat;
				massNomencl2.Amount = 1; //Количество шапок на каждого сотрудника

				List<MassExpenseNomenclature> listMassNomecl = new List<MassExpenseNomenclature> { massNomencl, massNomencl2 };

				MassExpense massExpense = new MassExpense();
				massExpense.Employees = listMassemployee;
				massExpense.ItemsNomenclature = listMassNomecl;
				massExpense.WarehouseFrom = warehouse;
				massExpense.Date = new DateTime(2020, 03, 23);

				string message = massExpense.ValidateNomenclature(uow);
				Assert.That(message.Length, Is.GreaterThan(0));
				//Assert.That(message, Is.EqualTo(""));
			}

		}



		public MassExpenseTest()
		{
		}
	}
}
