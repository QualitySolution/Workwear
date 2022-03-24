using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Testing.DB;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using workwear.Domain.Sizes;
using Workwear.Measurements;
using workwear.Repository.Operations;
using workwear.Tools;

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

		#region BestChoise

		[Test(Description = "Корректно и правильно выводить всю доступную подходящую номенклатуру.")]
		[Category("Integrated")]
		public void BestChoise_IssuingMultipleRows_TwoNomeclatureSameNeedsTest()
		{
			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				nomenclatureType.Category = ItemTypeCategory.wear;
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature {
					Type = nomenclatureType
				};
				uow.Save(nomenclature);

				var nomenclature2 = new Nomenclature {
					Type = nomenclatureType
				};
				uow.Save(nomenclature2);

				var protectionTools = new ProtectionTools {
					Name = "Номенклатура нормы"
				};
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

				var income = new Income {
					Warehouse = warehouse,
					Date = new DateTime(2020, 07, 20),
					Operation = IncomeOperations.Enter
				};
				var incomeItem1 = income.AddItem(nomenclature);
				incomeItem1.Amount = 10;
				var incomeItem2 = income.AddItem(nomenclature2);
				incomeItem2.Amount = 5;
				income.UpdateOperations(uow, ask);
				uow.Save(income);
				
				uow.Commit();
				Assert.That(uow.GetAll<WarehouseOperation>().Count(), Is.EqualTo(2));

				employee.FillWearInStockInfo(uow, baseParameters, warehouse, new DateTime(2020, 07, 22));
				Assert.That(employee.GetUnderreceivedItems(baseParameters).Count(), Is.GreaterThan(0));
				var employeeCardItem = employee.GetUnderreceivedItems(baseParameters).First();
				Assert.That(employeeCardItem.BestChoiceInStock.Count(), Is.GreaterThan(0));

				var bestChoice = employeeCardItem.BestChoiceInStock.First();

				Assert.That(bestChoice.Nomenclature, Is.EqualTo(nomenclature));
			}
		}

		[Test(Description = "Корректно и правильно выводить всю доступную подходящую номенклатуру.")]
		[Category("Integrated")]
		public void BestChoise_IssuingMultipleRows_TwoNomeclatureWearTypeSameNeedsTest()
		{
			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var sizeType = new SizeType() {Name = "Тестовый тип", Category = Category.Size};
				var heightType = new SizeType() {Name = "Тестовый рост", Category = Category.Height};
				uow.Save(sizeType);
				uow.Save(heightType);

				var nomenclatureType = new ItemsType {
					Name = "Тестовый тип номенклатуры",
					Category = ItemTypeCategory.wear,
					SizeType = sizeType,
					HeightType = heightType
				};
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature {
					Type = nomenclatureType,
					Sex = ClothesSex.Men,
				};
				uow.Save(nomenclature);

				var nomenclature2 = new Nomenclature {
					Type = nomenclatureType,
					Sex = ClothesSex.Men,
				};
				uow.Save(nomenclature2);

				var protectionTools = new ProtectionTools {
					Name = "Номенклатура нормы"
				};
				protectionTools.AddNomeclature(nomenclature);
				protectionTools.AddNomeclature(nomenclature2);
				uow.Save(protectionTools);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Year;
				normItem.PeriodCount = 1;
				uow.Save(norm);

				var size = new Size() {Name = "50", SizeType = sizeType};
				var height = new Size() {Name = "176", SizeType = heightType};
				uow.Save(size);
				uow.Save(height);

				var employee = new EmployeeCard();
				employee.AddUsedNorm(norm);
				employee.Sex = Sex.M;
				Assert.That(employee.WorkwearItems.Count, Is.GreaterThan(0));
				uow.Save(employee);
				var employeeSize = new EmployeeSize() {Size = size, SizeType = sizeType, Employee = employee};
				var employeeHeight = new EmployeeSize() {Size = height, SizeType = heightType, Employee = employee};
				uow.Save(employeeSize);
				uow.Save(employeeHeight);
				employee.Sizes.Add(employeeSize);
				employee.Sizes.Add(employeeHeight);
				uow.Commit();

				var income = new Income {
					Warehouse = warehouse,
					Date = new DateTime(2020, 07, 20),
					Operation = IncomeOperations.Enter
				};

				var incomeItem1 = income.AddItem(nomenclature);

				incomeItem1.Amount = 10;
				incomeItem1.WearSize = size;
				incomeItem1.Height = height;
				var incomeItem2 = income.AddItem(nomenclature2);
				incomeItem2.Amount = 5;
				incomeItem2.WearSize = size;
				incomeItem2.Height = height;

				income.UpdateOperations(uow, ask);
				uow.Save(income);
				uow.Commit();
				Assert.That(uow.GetAll<WarehouseOperation>().Count(), Is.EqualTo(2));

				employee.FillWearInStockInfo(uow, baseParameters, warehouse, new DateTime(2020, 07, 22));
				Assert.That(employee.GetUnderreceivedItems(baseParameters).Count(), Is.GreaterThan(0));
				var employeeCardItem = employee.GetUnderreceivedItems(baseParameters).First();
				var employeeCardItemCount = employee.GetUnderreceivedItems(baseParameters).Count();

				var inStock = employeeCardItem.InStock;

				Assert.That(employeeCardItem.InStock.Count, Is.GreaterThan(0));
				var bestChoiceCount = employeeCardItem.BestChoiceInStock.Count();
				Assert.That(employeeCardItem.BestChoiceInStock.Count(), Is.GreaterThan(0));

				var bestChoice = employeeCardItem.BestChoiceInStock.First();

				Assert.That(bestChoice.Nomenclature, Is.EqualTo(nomenclature));
			}
		}

		[Test(Description = "Корректно и правильно выводить всю доступную подходящую номенклатуру.")]
		[Category("Integrated")]
		public void BestChoise_IssuingMultipleRows_TwoNomeclatureShoesTypeSameNeedsTest()
		{
			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var shoes = new SizeType() {Name = "Обувь"};
				uow.Save(shoes);
				var winterShoes = new SizeType() {Name = "Зимняя обувь"};
				uow.Save(winterShoes);

				var nomenclatureType = new ItemsType {
					Name = "Обувь",
					Category = ItemTypeCategory.wear,
					SizeType = shoes
				};
				uow.Save(nomenclatureType);

				var nomenclatureType2 = new ItemsType {
					Name = "Зимняя обувь",
					Category = ItemTypeCategory.wear,
					SizeType = winterShoes
				};
				uow.Save(nomenclatureType2);

				var nomenclature = new Nomenclature {
					Type = nomenclatureType,
					Sex = ClothesSex.Men
				};
				uow.Save(nomenclature);

				var nomenclature2 = new Nomenclature {
					Type = nomenclatureType2,
					Sex = ClothesSex.Men
				};
				uow.Save(nomenclature2);

				var nomenclature3 = new Nomenclature {
					Type = nomenclatureType,
					Sex = ClothesSex.Men
				};
				uow.Save(nomenclature3);

				var protectionTools = new ProtectionTools {Name = "Номеклатура нормы"};
				protectionTools.AddNomeclature(nomenclature);
				protectionTools.AddNomeclature(nomenclature3);

				uow.Save(protectionTools);

				var protectionTools2 = new ProtectionTools {Name = "Номенклатура нормы_2"};
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

				var shoesSize = new Size() {SizeType = shoes, Name = "42"};
				var winterShoesSize = new Size() {SizeType = winterShoes, Name = "43"};
				uow.Save(shoesSize);
				uow.Save(winterShoesSize);

				var employee = new EmployeeCard();
				employee.AddUsedNorm(norm);
				employee.Sex = Sex.M;
				Assert.That(employee.WorkwearItems.Count, Is.GreaterThan(0));
				uow.Save(employee);
				uow.Commit();
				
				var employeeShoesSize = new EmployeeSize() 
					{Size = shoesSize, SizeType = shoes, Employee = employee};
				var employeeWinterShoesSize = new EmployeeSize()
					{Size = winterShoesSize, SizeType = winterShoes, Employee = employee};
				uow.Save(employeeShoesSize);
				uow.Save(employeeWinterShoesSize);

				var income = new Income {
					Warehouse = warehouse,
					Date = new DateTime(2020, 07, 20),
					Operation = IncomeOperations.Enter
				};

				var incomeItem1 = income.AddItem(nomenclature);
				incomeItem1.Amount = 1;
				incomeItem1.WearSize = shoesSize;

				var incomeItem2 = income.AddItem(nomenclature2);
				incomeItem2.Amount = 2;
				incomeItem2.WearSize = winterShoesSize;

				var incomeItem3 = income.AddItem(nomenclature3);
				incomeItem3.Amount = 3;
				incomeItem3.WearSize = shoesSize;

				income.UpdateOperations(uow, ask);
				uow.Save(income);
				uow.Commit();
				Assert.That(uow.GetAll<WarehouseOperation>().Count(), Is.EqualTo(3));

				employee.FillWearInStockInfo(uow, baseParameters, warehouse, new DateTime(2020, 07, 22));
				Assert.That(employee.GetUnderreceivedItems(baseParameters).Count(), Is.GreaterThan(0));

				Assert.That(employee.GetUnderreceivedItems(baseParameters).Count(), Is.EqualTo(2));

				var employeeCardItem = employee.GetUnderreceivedItems(baseParameters).First();
				Assert.That(employeeCardItem.InStock.Count(), Is.GreaterThan(0));

				var bestChoiceInStock = employeeCardItem.BestChoiceInStock;
				var bestChoiceCount = employeeCardItem.BestChoiceInStock.Count();
				Assert.That(employeeCardItem.BestChoiceInStock.Count(), Is.GreaterThan(0));

				Assert.That(employeeCardItem.BestChoiceInStock.Count(), Is.EqualTo(2));
			}
		}

		[Test(Description = "В подходящей номеклатуре сначала выводим свои номеклатуры, а уже потом аналоги.")]
		[Category("Integrated")]
		public void BestChoise_SelfNomenclatureFirstTest()
		{
			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var sizeType = new SizeType();
				uow.Save(sizeType);

				var nomenclatureType = new ItemsType {
					Name = "Обувь",
					Category = ItemTypeCategory.wear,
					SizeType = sizeType
				};
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature {
					Type = nomenclatureType,
					Sex = ClothesSex.Men,
				};
				uow.Save(nomenclature);

				var nomenclature2 = new Nomenclature {
					Type = nomenclatureType,
					Sex = ClothesSex.Men,
				};
				uow.Save(nomenclature2);

				var protectionTools = new ProtectionTools {Name = "Номенклатура нормы"};
				protectionTools.AddNomeclature(nomenclature);

				var protectionTools2 = new ProtectionTools {Name = "Номенклатура нормы_2"};
				protectionTools2.AddNomeclature(nomenclature2);

				protectionTools.AddAnalog(protectionTools2);
				protectionTools2.AddAnalog(protectionTools);
				uow.Save(protectionTools);
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
				Assert.That(employee.WorkwearItems.Count, Is.EqualTo(2));
				uow.Save(employee);
				uow.Commit();

				var size = new Size() {SizeType = sizeType};
				uow.Save(size);
				var employeeSize = new EmployeeSize() {SizeType = sizeType, Size = size, Employee = employee};
				uow.Save(employeeSize);
				employee.Sizes.Add(employeeSize);

				var income = new Income {
					Warehouse = warehouse,
					Date = new DateTime(2020, 07, 20),
					Operation = IncomeOperations.Enter
				};

				var incomeItem1 = income.AddItem(nomenclature);

				incomeItem1.Amount = 1;
				incomeItem1.WearSize = size;

				var incomeItem2 = income.AddItem(nomenclature2);
				incomeItem2.Amount = 2;
				incomeItem2.WearSize = size;

				income.UpdateOperations(uow, ask);
				uow.Save(income);
				uow.Commit();
				Assert.That(uow.GetAll<WarehouseOperation>().Count(), Is.EqualTo(2));

				employee.FillWearInStockInfo(uow, baseParameters, warehouse, new DateTime(2020, 07, 22));
				var item1 = employee.WorkwearItems.FirstOrDefault(x => x.ActiveNormItem == normItem);
				var item2 = employee.WorkwearItems.FirstOrDefault(x => x.ActiveNormItem == normItem2);
				Assert.That(item1.BestChoiceInStock.First().Nomenclature.Id, Is.EqualTo(nomenclature.Id));
				Assert.That(item1.BestChoiceInStock.Count(), Is.EqualTo(2));
				Assert.That(item2.BestChoiceInStock.First().Nomenclature.Id, Is.EqualTo(nomenclature2.Id));
				Assert.That(item2.BestChoiceInStock.Count(), Is.EqualTo(2));
			}
		}

		#endregion

		#region FillWearRecivedInfo

		[Test(Description = "Проверяем что при заполнении выданной спецодежды проверяем аналоги тоже.")]
		[Category("real case")]
		[Category("Integrated")]
		public void FillWearRecivedInfo_AnalogItemsTest()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				nomenclatureType.Category = ItemTypeCategory.wear;
				nomenclatureType.WearCategory = СlothesType.Wear;
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				nomenclature.Sex = ClothesSex.Men;
				nomenclature.SizeStd = "UnisexWearRus";
				uow.Save(nomenclature);

				var protectionToolsAnalog = new ProtectionTools();
				protectionToolsAnalog.Name = "Номенклатура нормы Аналог";
				protectionToolsAnalog.AddNomeclature(nomenclature);
				uow.Save(protectionToolsAnalog);

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "Номенклатура нормы";
				protectionTools.AddAnalog(protectionToolsAnalog);
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

				var warehouseOperation = new WarehouseOperation {
					Nomenclature = nomenclature,
					Amount = 1,
					OperationTime = new DateTime(2018, 1, 20),
				};
				uow.Save(warehouseOperation);

				var operation = new EmployeeIssueOperation {
					Employee = employee,
					ExpiryByNorm = new DateTime(2019, 1, 20),
					Issued = 1,
					Nomenclature = nomenclature,
					NormItem = normItem,
					ProtectionTools = protectionToolsAnalog,
					OperationTime = new DateTime(2018, 1, 20),
					WarehouseOperation = warehouseOperation,
				};
				uow.Save(operation);
				uow.Commit();

				employee.FillWearRecivedInfo(new EmployeeIssueRepository(uow));
				var item = employee.WorkwearItems.First();
				Assert.That(item.Amount, Is.EqualTo(1));
				Assert.That(item.LastIssue, Is.EqualTo(new DateTime(2018, 1, 20)));
			}
		}

		[Test(Description = "Проверяем что при заполнении выданной спецодежды количество считаем с учетом автосписания.")]
		[Category("real case")]
		[Category("Integrated")]
		public void FillWearRecivedInfo_AutoWriteOffItemsTest()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				nomenclatureType.Category = ItemTypeCategory.wear;
				nomenclatureType.WearCategory = СlothesType.Wear;
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				nomenclature.Sex = ClothesSex.Men;
				nomenclature.SizeStd = "UnisexWearRus";
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "Номенклатура нормы";
				protectionTools.AddNomeclature(nomenclature);
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

				var warehouseOperation = new WarehouseOperation {
					Nomenclature = nomenclature,
					Amount = 1,
					OperationTime = new DateTime(2018, 1, 20),
				};
				uow.Save(warehouseOperation);

				var operation = new EmployeeIssueOperation {
					Employee = employee,
					ExpiryByNorm = new DateTime(2019, 1, 20),
					Issued = 1,
					Nomenclature = nomenclature,
					NormItem = normItem,
					ProtectionTools = protectionTools,
					OperationTime = new DateTime(2018, 1, 20),
					WarehouseOperation = warehouseOperation,
					UseAutoWriteoff = true,
					AutoWriteoffDate = new DateTime(2019, 1, 20),
				};
				uow.Save(operation);

				var warehouseOperation2 = new WarehouseOperation {
					Nomenclature = nomenclature,
					Amount = 1,
					OperationTime = new DateTime(2019, 1, 20),
				};
				uow.Save(warehouseOperation2);

				var operation2 = new EmployeeIssueOperation {
					Employee = employee,
					ExpiryByNorm = new DateTime(2020, 1, 20),
					Issued = 1,
					Nomenclature = nomenclature,
					NormItem = normItem,
					ProtectionTools = protectionTools,
					OperationTime = new DateTime(2019, 1, 20),
					WarehouseOperation = warehouseOperation2,
				};
				uow.Save(operation2);

				uow.Commit();

				employee.FillWearRecivedInfo(new EmployeeIssueRepository(uow));
				var item = employee.WorkwearItems.First();
				Assert.That(item.Amount, Is.EqualTo(1));
				Assert.That(item.LastIssue, Is.EqualTo(new DateTime(2019, 1, 20)));
			}
		}

		[Test(Description = "Проверяем что при заполнении выданной спецодежды количество считаем с учетом автосписания.")]
		[Category("Integrated")]
		public void FillWearRecivedInfo_AutoWriteOffItemsManualWriteoffWTest()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				nomenclatureType.Category = ItemTypeCategory.wear;
				nomenclatureType.WearCategory = СlothesType.Wear;
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				nomenclature.Sex = ClothesSex.Men;
				nomenclature.SizeStd = "UnisexWearRus";
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "Номенклатура нормы";
				protectionTools.AddNomeclature(nomenclature);
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

				var warehouseOperationAutowriteoff = new WarehouseOperation {
					Nomenclature = nomenclature,
					Amount = 1,
					OperationTime = new DateTime(2018, 1, 20),
				};
				uow.Save(warehouseOperationAutowriteoff);

				var operationAutowriteoff = new EmployeeIssueOperation {
					Employee = employee,
					ExpiryByNorm = new DateTime(2019, 1, 20),
					Issued = 1,
					Nomenclature = nomenclature,
					NormItem = normItem,
					ProtectionTools = protectionTools,
					OperationTime = new DateTime(2018, 1, 20),
					WarehouseOperation = warehouseOperationAutowriteoff,
					UseAutoWriteoff = true,
					AutoWriteoffDate = new DateTime(2019, 1, 20),
				};
				uow.Save(operationAutowriteoff);

				var warehouseOperationManualWriteoff = new WarehouseOperation {
					Nomenclature = nomenclature,
					Amount = 1,
					OperationTime = new DateTime(2019, 1, 20),
				};
				uow.Save(warehouseOperationManualWriteoff);

				var operationManualWriteoff = new EmployeeIssueOperation {
					Employee = employee,
					Returned = 1,
					Nomenclature = nomenclature,
					ProtectionTools = protectionTools,
					OperationTime = new DateTime(2019, 1, 20),
					WarehouseOperation = warehouseOperationManualWriteoff,
					IssuedOperation = operationAutowriteoff
				};
				uow.Save(operationManualWriteoff);

				var warehouseOperation = new WarehouseOperation {
					Nomenclature = nomenclature,
					Amount = 1,
					OperationTime = new DateTime(2019, 1, 20),
				};
				uow.Save(warehouseOperation);

				var operation = new EmployeeIssueOperation {
					Employee = employee,
					ExpiryByNorm = new DateTime(2020, 1, 20),
					Issued = 5,
					Nomenclature = nomenclature,
					NormItem = normItem,
					ProtectionTools = protectionTools,
					OperationTime = new DateTime(2019, 1, 20),
					WarehouseOperation = warehouseOperation,
				};
				uow.Save(operation);

				uow.Commit();

				employee.FillWearRecivedInfo(new EmployeeIssueRepository(uow));
				var item = employee.WorkwearItems.First();
				Assert.That(item.Amount, Is.EqualTo(5));
				Assert.That(item.LastIssue, Is.EqualTo(new DateTime(2019, 1, 20)));
			}
		}

		[Test(Description = "Проверяем что при заполнении выданной спецодежды дата последней выдачи не выскакивает на дату списание.")]
		[Category("Integrated")]
		public void FillWearRecivedInfo_LastIssueDateNotReturnDateTest()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				nomenclatureType.Category = ItemTypeCategory.wear;
				nomenclatureType.WearCategory = СlothesType.Wear;
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				nomenclature.Sex = ClothesSex.Men;
				nomenclature.SizeStd = "UnisexWearRus";
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "Номенклатура нормы";
				protectionTools.AddNomeclature(nomenclature);
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

				var warehouseOperation = new WarehouseOperation {
					Nomenclature = nomenclature,
					Amount = 1,
					OperationTime = new DateTime(2018, 1, 20),
				};
				uow.Save(warehouseOperation);

				var operationIssue = new EmployeeIssueOperation {
					Employee = employee,
					ExpiryByNorm = new DateTime(2019, 1, 20),
					Issued = 1,
					Nomenclature = nomenclature,
					NormItem = normItem,
					ProtectionTools = protectionTools,
					OperationTime = new DateTime(2018, 1, 20),
					WarehouseOperation = warehouseOperation,
				};
				uow.Save(operationIssue);

				var warehouseOperationManualWriteoff = new WarehouseOperation {
					Nomenclature = nomenclature,
					Amount = 1,
					OperationTime = new DateTime(2019, 1, 20),
				};
				uow.Save(warehouseOperationManualWriteoff);

				var operationManualWriteoff = new EmployeeIssueOperation {
					Employee = employee,
					Returned = 1,
					Nomenclature = nomenclature,
					ProtectionTools = protectionTools,
					OperationTime = new DateTime(2019, 1, 20),
					WarehouseOperation = warehouseOperationManualWriteoff,
					IssuedOperation = operationIssue
				};
				uow.Save(operationManualWriteoff);

				uow.Commit();

				employee.FillWearRecivedInfo(new EmployeeIssueRepository(uow));
				var item = employee.WorkwearItems.First();
				Assert.That(item.LastIssue, Is.EqualTo(new DateTime(2018, 1, 20)));
				Assert.That(item.Amount, Is.EqualTo(1));
				
			}
		}

		[Test(Description = "Проверяем что при заполнении выданной спецодежды дата последней выдачи и количество отображается, даже если СИЗ уже изношен.")]
		[Category("Integrated")]
		public void FillWearRecivedInfo_LastIssueDateExistAfterAutoWriteoffDateTest()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				nomenclatureType.Category = ItemTypeCategory.wear;
				nomenclatureType.WearCategory = СlothesType.Wear;
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				nomenclature.Sex = ClothesSex.Men;
				nomenclature.SizeStd = "UnisexWearRus";
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "Номенклатура нормы";
				protectionTools.AddNomeclature(nomenclature);
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

				var warehouseOperation = new WarehouseOperation {
					Nomenclature = nomenclature,
					Amount = 1,
					OperationTime = new DateTime(2018, 1, 20),
				};
				uow.Save(warehouseOperation);

				var operationIssue = new EmployeeIssueOperation {
					Employee = employee,
					ExpiryByNorm = new DateTime(2019, 1, 20),
					AutoWriteoffDate = new DateTime(2019, 1, 20),
					UseAutoWriteoff = true,
					Issued = 1,
					Nomenclature = nomenclature,
					NormItem = normItem,
					ProtectionTools = protectionTools,
					OperationTime = new DateTime(2018, 1, 20),
					StartOfUse = new DateTime(2018, 1, 20),
					WarehouseOperation = warehouseOperation,
				};
				uow.Save(operationIssue);

				uow.Commit();

				employee.FillWearRecivedInfo(new EmployeeIssueRepository(uow));
				var item = employee.WorkwearItems.First();
				Assert.That(item.Amount, Is.EqualTo(1));
				Assert.That(item.LastIssue, Is.EqualTo(new DateTime(2018, 1, 20)));
			}
		}

		[Test(Description = "Проверяем что при заполнении выданной спецодежды выданное для разных строк нормы, но при этом СИЗ-ы настроенные как аналоги не сливаются в одну строку. Реальная проблема в НЛМК.")]
		[Category("Integrated")]
		public void FillWearRecivedInfo_NotMergeAnalogTest()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				nomenclatureType.Category = ItemTypeCategory.wear;
				nomenclatureType.WearCategory = СlothesType.Wear;
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				nomenclature.Sex = ClothesSex.Men;
				nomenclature.SizeStd = "UnisexWearRus";
				uow.Save(nomenclature);

				var protectionToolsAnalog = new ProtectionTools();
				protectionToolsAnalog.Name = "Номенклатура нормы Аналог";
				protectionToolsAnalog.AddNomeclature(nomenclature);

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "Номенклатура нормы";
				protectionTools.AddAnalog(protectionToolsAnalog);
				protectionTools.AddNomeclature(nomenclature);
				protectionToolsAnalog.AddAnalog(protectionTools);
				uow.Save(protectionToolsAnalog);
				uow.Save(protectionTools);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Year;
				normItem.PeriodCount = 1;
				var normItem2 = norm.AddItem(protectionToolsAnalog);
				normItem2.Amount = 2;
				normItem2.NormPeriod = NormPeriodType.Year;
				normItem2.PeriodCount = 1;
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

				var warehouseOperation = new WarehouseOperation {
					Nomenclature = nomenclature,
					Amount = 1,
					OperationTime = new DateTime(2018, 1, 20),
				};
				uow.Save(warehouseOperation);

				var operation = new EmployeeIssueOperation {
					Employee = employee,
					ExpiryByNorm = new DateTime(2019, 1, 20),
					Issued = 1,
					Nomenclature = nomenclature,
					NormItem = normItem,
					ProtectionTools = protectionTools,
					OperationTime = new DateTime(2018, 1, 20),
					WarehouseOperation = warehouseOperation,
				};
				uow.Save(operation);

				var warehouseOperation2 = new WarehouseOperation {
					Nomenclature = nomenclature,
					Amount = 2,
					OperationTime = new DateTime(2018, 1, 20),
				};
				uow.Save(warehouseOperation2);

				var operation2 = new EmployeeIssueOperation {
					Employee = employee,
					ExpiryByNorm = new DateTime(2019, 1, 20),
					Issued = 2,
					Nomenclature = nomenclature,
					NormItem = normItem2,
					ProtectionTools = protectionToolsAnalog,
					OperationTime = new DateTime(2018, 1, 20),
					WarehouseOperation = warehouseOperation,
				};
				uow.Save(operation2);

				uow.Commit();

				employee.FillWearRecivedInfo(new EmployeeIssueRepository(uow));
				var item1 = employee.WorkwearItems.FirstOrDefault(x => x.ActiveNormItem == normItem);
				Assert.That(item1.Amount, Is.EqualTo(1));
				var item2 = employee.WorkwearItems.FirstOrDefault(x => x.ActiveNormItem == normItem2);
				Assert.That(item2.Amount, Is.EqualTo(2));

			}
		}

		#endregion
	}
}
