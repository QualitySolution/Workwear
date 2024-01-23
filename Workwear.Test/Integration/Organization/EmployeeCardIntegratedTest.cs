using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Testing.DB;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Models.Operations;
using Workwear.Repository.Operations;
using Workwear.Repository.Stock;
using Workwear.Tools;

namespace Workwear.Test.Integration.Organization
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
		public void BestChoose_IssuingMultipleRows_TwoNomenclatureSameNeedsTest()
		{
			var ask = Substitute.For<IInteractiveService>();
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
				protectionTools.AddNomenclature(nomenclature);
				protectionTools.AddNomenclature(nomenclature2);
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
				var incomeItem1 = income.AddItem(nomenclature, ask);
				incomeItem1.Amount = 10;
				var incomeItem2 = income.AddItem(nomenclature2, ask);
				incomeItem2.Amount = 5;
				income.UpdateOperations(uow, ask);
				uow.Save(income);
				
				uow.Commit();
				Assert.That(uow.GetAll<WarehouseOperation>().Count(), Is.EqualTo(2));

				var today = new DateTime(2020, 07, 22);
				var uowProvider = new UnitOfWorkProvider(uow);
				var issueModel = new EmployeeIssueModel(new EmployeeIssueRepository(uowProvider), uowProvider);
				employee.FillWearReceivedInfo(new EmployeeIssueRepository(uowProvider));
				var stockModel = new StockBalanceModel(uowProvider, new StockRepository());
				stockModel.Warehouse = warehouse;
				stockModel.OnDate = today;
				issueModel.FillWearInStockInfo(employee, stockModel);
				Assert.That(employee.GetUnderreceivedItems(baseParameters, today).Count(), Is.GreaterThan(0));
				var employeeCardItem = employee.GetUnderreceivedItems(baseParameters, today).First();
				Assert.That(employeeCardItem.BestChoiceInStock.Count(), Is.GreaterThan(0));

				var bestChoice = employeeCardItem.BestChoiceInStock.First();

				Assert.That(bestChoice.Position.Nomenclature, Is.EqualTo(nomenclature));
			}
		}

		[Test(Description = "Корректно и правильно выводить всю доступную подходящую номенклатуру.")]
		[Category("Integrated")]
		public void BestChoose_IssuingMultipleRows_TwoNomenclatureWearTypeSameNeedsTest()
		{
			var ask = Substitute.For<IInteractiveService>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var sizeType = new SizeType {Name = "Тестовый тип", CategorySizeType = CategorySizeType.Size};
				var heightType = new SizeType {Name = "Тестовый рост", CategorySizeType = CategorySizeType.Height};
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
				protectionTools.AddNomenclature(nomenclature);
				protectionTools.AddNomenclature(nomenclature2);
				uow.Save(protectionTools);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Year;
				normItem.PeriodCount = 1;
				uow.Save(norm);

				var size = new Size {Name = "50", SizeType = sizeType};
				var height = new Size {Name = "176", SizeType = heightType};
				uow.Save(size);
				uow.Save(height);

				var employee = new EmployeeCard();
				employee.AddUsedNorm(norm);
				employee.Sex = Sex.M;
				Assert.That(employee.WorkwearItems.Count, Is.GreaterThan(0));
				uow.Save(employee);
				var employeeSize = new EmployeeSize {Size = size, SizeType = sizeType, Employee = employee};
				var employeeHeight = new EmployeeSize {Size = height, SizeType = heightType, Employee = employee};
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

				var incomeItem1 = income.AddItem(nomenclature, ask);

				incomeItem1.Amount = 10;
				incomeItem1.WearSize = size;
				incomeItem1.Height = height;
				var incomeItem2 = income.AddItem(nomenclature2, ask);
				incomeItem2.Amount = 5;
				incomeItem2.WearSize = size;
				incomeItem2.Height = height;

				income.UpdateOperations(uow, ask);
				uow.Save(income);
				uow.Commit();
				Assert.That(uow.GetAll<WarehouseOperation>().Count(), Is.EqualTo(2));

				var today = new DateTime(2020, 07, 22);
				var uowProvider = new UnitOfWorkProvider(uow);
				var issueModel = new EmployeeIssueModel(new EmployeeIssueRepository(uowProvider), uowProvider);
				employee.FillWearReceivedInfo(new EmployeeIssueRepository(uowProvider));
				var stockModel = new StockBalanceModel(uowProvider, new StockRepository());
				stockModel.Warehouse = warehouse;
				stockModel.OnDate = today;
				issueModel.FillWearInStockInfo(employee, stockModel);
				Assert.That(employee.GetUnderreceivedItems(baseParameters, today).Count(), Is.GreaterThan(0));
				var employeeCardItem = employee.GetUnderreceivedItems(baseParameters, today).First();
				var employeeCardItemCount = employee.GetUnderreceivedItems(baseParameters, today).Count();

				var inStock = employeeCardItem.InStock;

				Assert.That(employeeCardItem.InStock.Count, Is.GreaterThan(0));
				var bestChoiceCount = employeeCardItem.BestChoiceInStock.Count();
				Assert.That(employeeCardItem.BestChoiceInStock.Count(), Is.GreaterThan(0));

				var bestChoice = employeeCardItem.BestChoiceInStock.First();

				Assert.That(bestChoice.Position.Nomenclature, Is.EqualTo(nomenclature));
			}
		}

		[Test(Description = "Корректно и правильно выводить всю доступную подходящую номенклатуру.")]
		[Category("Integrated")]
		public void BestChoose_IssuingMultipleRows_TwoNomenclatureShoesTypeSameNeedsTest()
		{
			var ask = Substitute.For<IInteractiveService>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var shoes = new SizeType {Name = "Обувь"};
				uow.Save(shoes);
				var winterShoes = new SizeType {Name = "Зимняя обувь"};
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

				var protectionTools = new ProtectionTools {Name = "Номенклатура нормы"};
				protectionTools.AddNomenclature(nomenclature);
				protectionTools.AddNomenclature(nomenclature3);

				uow.Save(protectionTools);

				var protectionTools2 = new ProtectionTools {Name = "Номенклатура нормы_2"};
				protectionTools2.AddNomenclature(nomenclature2);
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

				var shoesSize = new Size {SizeType = shoes, Name = "42"};
				var winterShoesSize = new Size {SizeType = winterShoes, Name = "43"};
				uow.Save(shoesSize);
				uow.Save(winterShoesSize);

				var employee = new EmployeeCard();
				employee.AddUsedNorm(norm);
				employee.Sex = Sex.M;
				Assert.That(employee.WorkwearItems.Count, Is.GreaterThan(0));
				uow.Save(employee);
				uow.Commit();
				
				var employeeShoesSize = new EmployeeSize {Size = shoesSize, SizeType = shoes, Employee = employee};
				var employeeWinterShoesSize = new EmployeeSize {Size = winterShoesSize, SizeType = winterShoes, Employee = employee};
				uow.Save(employeeShoesSize);
				uow.Save(employeeWinterShoesSize);
				
				employee.Sizes.Add(employeeShoesSize);
				employee.Sizes.Add(employeeWinterShoesSize);

				var income = new Income {
					Warehouse = warehouse,
					Date = new DateTime(2020, 07, 20),
					Operation = IncomeOperations.Enter
				};

				var incomeItem1 = income.AddItem(nomenclature, ask);
				incomeItem1.Amount = 1;
				incomeItem1.WearSize = shoesSize;

				var incomeItem2 = income.AddItem(nomenclature2, ask);
				incomeItem2.Amount = 2;
				incomeItem2.WearSize = winterShoesSize;

				var incomeItem3 = income.AddItem(nomenclature3, ask);
				incomeItem3.Amount = 3;
				incomeItem3.WearSize = shoesSize;

				income.UpdateOperations(uow, ask);
				uow.Save(income);
				uow.Commit();
				Assert.That(uow.GetAll<WarehouseOperation>().Count(), Is.EqualTo(3));

				var today = new DateTime(2020, 07, 22);
				var uowProvider = new UnitOfWorkProvider(uow);
				var issueModel = new EmployeeIssueModel(new EmployeeIssueRepository(uowProvider), uowProvider);
				employee.FillWearReceivedInfo(new EmployeeIssueRepository(uowProvider));
				var stockModel = new StockBalanceModel(uowProvider, new StockRepository());
				stockModel.Warehouse = warehouse;
				stockModel.OnDate = today;
				issueModel.FillWearInStockInfo(employee, stockModel);
				Assert.That(employee.GetUnderreceivedItems(baseParameters, today).Count(), Is.GreaterThan(0));
				Assert.That(employee.GetUnderreceivedItems(baseParameters, today).Count(), Is.EqualTo(2));

				var employeeCardItem = employee.GetUnderreceivedItems(baseParameters, today).First();
				Assert.That(employeeCardItem.InStock.Count(), Is.GreaterThan(0));

				var bestChoiceInStock = employeeCardItem.BestChoiceInStock;
				var bestChoiceCount = employeeCardItem.BestChoiceInStock.Count();
				Assert.That(employeeCardItem.BestChoiceInStock.Count(), Is.EqualTo(2));
			}
		}

		#endregion

		#region FillWearReceivedInfo
		[Test(Description = "Проверяем что при заполнении выданной спецодежды количество считаем с учетом автосписания.")]
		[Category("real case")]
		[Category("Integrated")]
		public void FillWearReceivedInfo_AutoWriteOffItemsTest()
		{
			var baseParameters = Substitute.For<BaseParameters>();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				nomenclatureType.Category = ItemTypeCategory.wear;
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				nomenclature.Sex = ClothesSex.Men;
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "Номенклатура нормы";
				protectionTools.AddNomenclature(nomenclature);
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

				var today = new DateTime(2020, 1, 1);
				employee.FillWearReceivedInfo(new EmployeeIssueRepository(uow));
				var item = employee.WorkwearItems.First();
				Assert.That(item.Issued(today), Is.EqualTo(1));
				Assert.That(item.LastIssued(today, baseParameters).First().date, Is.EqualTo(new DateTime(2019, 1, 20)));
			}
		}

		[Test(Description = "Проверяем что при заполнении выданной спецодежды количество считаем с учетом автосписания.")]
		[Category("Integrated")]
		public void FillWearReceivedInfo_AutoWriteOffItemsManualTest()
		{
			var baseParameters = Substitute.For<BaseParameters>();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				nomenclatureType.Category = ItemTypeCategory.wear;
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				nomenclature.Sex = ClothesSex.Men;
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "Номенклатура нормы";
				protectionTools.AddNomenclature(nomenclature);
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

				var today = new DateTime(2020, 1, 1);
				employee.FillWearReceivedInfo(new EmployeeIssueRepository(uow));
				var item = employee.WorkwearItems.First();
				Assert.That(item.Issued(today), Is.EqualTo(5));
				Assert.That(item.LastIssued(today, baseParameters).First().date, Is.EqualTo(new DateTime(2019, 1, 20)));
			}
		}

		[Test(Description = "Проверяем что при заполнении выданной спецодежды дата последней выдачи не выскакивает на дату списание.")]
		[Category("Integrated")]
		public void FillWearReceivedInfo_LastIssueDateNotReturnDateTest()
		{
			var baseParameters = Substitute.For<BaseParameters>();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				nomenclatureType.Category = ItemTypeCategory.wear;
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				nomenclature.Sex = ClothesSex.Men;
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "Номенклатура нормы";
				protectionTools.AddNomenclature(nomenclature);
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

				var today = new DateTime(2020, 1, 1);
				employee.FillWearReceivedInfo(new EmployeeIssueRepository(uow));
				var item = employee.WorkwearItems.First();
				Assert.That(item.LastIssued(today, baseParameters).First().date, Is.EqualTo(new DateTime(2018, 1, 20)));
				Assert.That(item.LastIssued(today, baseParameters).First().amount, Is.EqualTo(1));
			}
		}

		[Test(Description = "Проверяем что при заполнении выданной спецодежды дата последней выдачи " +
		                    "и количество отображается, даже если СИЗ уже изношен.")]
		[Category("Integrated")]
		public void FillWearReceivedInfo_LastIssueDateExistAfterAutoWriterDateTest()
		{
			var baseParameters = Substitute.For<BaseParameters>();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				nomenclatureType.Category = ItemTypeCategory.wear;
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				nomenclature.Sex = ClothesSex.Men;
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "Номенклатура нормы";
				protectionTools.AddNomenclature(nomenclature);
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

				var today = new DateTime(2020, 1, 1);
				employee.FillWearReceivedInfo(new EmployeeIssueRepository(uow));
				var item = employee.WorkwearItems.First();
				Assert.That(item.LastIssued(today, baseParameters).First().date, Is.EqualTo(new DateTime(2018, 1, 20)));
			}
		}
		#endregion
	}
}
