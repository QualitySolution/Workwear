using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.Testing.DB;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Repository.Stock;
using Workwear.Tools.Features;

namespace Workwear.Test.Integration.Stock
{
	[TestFixture(TestOf = typeof(StockRepository))]
	public class StockRepositoryTest : InMemoryDBGlobalConfigTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init()
		{
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
		}

		[Test(Description = "Если в справочнике складов более одного склада, мы не должны возвращать склад по умолчанию.(Версия с поддержкой складов)")]
		[Category("Integrated")]
		public void GetDefaultWarehouse_ManyWarehouses_WarehousesEnableTest()
		{
			var featuresService = Substitute.For<FeaturesService>();
			featuresService.Available(WorkwearFeature.Warehouses).Returns(true);
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse1 = new Warehouse();
				uow.Save(warehouse1);

				var warehouse2 = new Warehouse();
				uow.Save(warehouse2);
				uow.Commit();

				var defaultWarehouse = new StockRepository().GetDefaultWarehouse(uow, featuresService, 0);
				Assert.That(defaultWarehouse, Is.Null);
			}
		}

		[Test(Description = "Если в справочнике складов более одного склада и склады не поддерживаются, мы все равно должны вернуть склад по умолчанию.")]
		[Category("Integrated")]
		public void GetDefaultWarehouse_ManyWarehouses_WarehousesDisableTest()
		{
			var featuresService = Substitute.For<FeaturesService>();
			featuresService.Available(WorkwearFeature.Warehouses).Returns(false);
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse1 = new Warehouse();
				uow.Save(warehouse1);

				var warehouse2 = new Warehouse();
				uow.Save(warehouse2);
				uow.Commit();

				var defaultWarehouse = new StockRepository().GetDefaultWarehouse(uow, featuresService, 0);
				Assert.That(defaultWarehouse, Is.Not.Null);
			}
		}

		[Test(Description = "Если в справочнике складов один склад, возвращаем его как склад по умолчанию.")]
		[Category("Integrated")]
		public void GetDefaultWarehouse_OneWarehouseTest()
		{
			var featuresService = Substitute.For<FeaturesService>();
			featuresService.Available(WorkwearFeature.Warehouses).Returns(true);
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse1 = new Warehouse();
				warehouse1.Name = "Единственный";
				uow.Save(warehouse1);
				uow.Commit();

				var defaultWarehouse = new StockRepository().GetDefaultWarehouse(uow, new FeaturesService(), 0);
				Assert.That(defaultWarehouse.Name, Is.EqualTo("Единственный"));
			}
		}
		
		[Test(Description = "Проверяем что считаем правильно количество в случае если есть на складе одна номенклатура с размером и без." +
		                    "Реальный баг, в строку без размера, показывалась сумма всех в том числе и с размерами.")]
		[Category("Integrated")]
		[Category("Real case")]
		public void StockBalances_DontSumSizeAndEmptySizeTest()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				warehouse.Name = "Единственный";
				uow.Save(warehouse);

				var size = new Size();
				uow.Save(size);

				var nomenclature = new Nomenclature {
					Name = "Тестовая номенклатура"
				};
				uow.Save(nomenclature);

				var operationWithSize = new WarehouseOperation {
					OperationTime = new DateTime(2022, 10, 1),
					Nomenclature = nomenclature,
					Amount = 66,
					ReceiptWarehouse = warehouse,
					WearSize = size
				};
				uow.Save(operationWithSize);
				
				var operationWithoutSize = new WarehouseOperation {
					OperationTime = new DateTime(2022, 10, 2),
					Nomenclature = nomenclature,
					Amount = 10,
					ReceiptWarehouse = warehouse
				};
				uow.Save(operationWithoutSize);
				
				var operationWithoutSize2 = new WarehouseOperation {
					OperationTime = new DateTime(2022, 10, 3),
					Nomenclature = nomenclature,
					Amount = 5,
					ReceiptWarehouse = warehouse
				};
				uow.Save(operationWithoutSize2);
				
				uow.Commit();

				var balance = new StockRepository().StockBalances(uow, warehouse, new List<Nomenclature>{nomenclature}, new DateTime(2022, 10, 5));
				Assert.That(balance, Has.Count.EqualTo(2));
				var itemWithSize = balance.First(x => x.WearSize == size);
				Assert.That(itemWithSize.Amount, Is.EqualTo(66));
				var itemWithoutSize = balance.First(x => x.WearSize == null);
				Assert.That(itemWithoutSize.Amount, Is.EqualTo(15));
			}
		}
		
		[Test(Description = "Проверяем что результаты делятся по собственникам.")]
		[Category("Integrated")]
		public void StockBalances_SplitByOwnersTest()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				warehouse.Name = "Единственный";
				uow.Save(warehouse);

				var owner = new Owner {
					Name = "Самый важный собственник"
				};
				uow.Save(owner);
				
				var owner2 = new Owner {
					Name = "Второй собственник"
				};
				uow.Save(owner2);

				var nomenclature = new Nomenclature {
					Name = "Тестовая номенклатура"
				};
				uow.Save(nomenclature);

				var operationOwner1 = new WarehouseOperation {
					OperationTime = new DateTime(2022, 10, 1),
					Nomenclature = nomenclature,
					Amount = 6,
					ReceiptWarehouse = warehouse,
					Owner = owner
				};
				uow.Save(operationOwner1);
				
				var operationWithoutOwner = new WarehouseOperation {
					OperationTime = new DateTime(2022, 10, 2),
					Nomenclature = nomenclature,
					Amount = 10,
					ReceiptWarehouse = warehouse
				};
				uow.Save(operationWithoutOwner);
				
				var operationOwner2 = new WarehouseOperation {
					OperationTime = new DateTime(2022, 10, 3),
					Nomenclature = nomenclature,
					Amount = 50,
					ReceiptWarehouse = warehouse,
					Owner = owner2
				};
				uow.Save(operationOwner2);
				
				var operationOwner2_2 = new WarehouseOperation {
					OperationTime = new DateTime(2022, 10, 3),
					Nomenclature = nomenclature,
					Amount = 50,
					ReceiptWarehouse = warehouse,
					Owner = owner2
				};
				uow.Save(operationOwner2_2);
				
				uow.Commit();

				var balance = new StockRepository().StockBalances(uow, warehouse, new List<Nomenclature>{nomenclature}, new DateTime(2022, 10, 10));
				Assert.That(balance, Has.Count.EqualTo(3));
				var itemOwner1 = balance.First(x => x.Owner == owner);
				Assert.That(itemOwner1.Amount, Is.EqualTo(6));
				var itemOwner2 = balance.First(x => x.Owner == owner2);
				Assert.That(itemOwner2.Amount, Is.EqualTo(100));
				var itemWithoutSize = balance.First(x => x.Owner == null);
				Assert.That(itemWithoutSize.Amount, Is.EqualTo(10));
			}
		}
		
		[Test(Description = "Проверяем что правильно учитывается дата, на которую запрашиваются остатки")]
		[Category("Integrated")]
		[Category("Real case")]
		public void StockBalances_DateFilter()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				warehouse.Name = "Единственный";
				uow.Save(warehouse);

				var nomenclature = new Nomenclature {
					Name = "Тестовая номенклатура"
				};
				uow.Save(nomenclature);

				var operation1 = new WarehouseOperation {
					OperationTime = new DateTime(2022, 10, 1),
					Nomenclature = nomenclature,
					Amount = 10,
					ReceiptWarehouse = warehouse,
				};
				uow.Save(operation1);
				
				var operation2 = new WarehouseOperation {
					OperationTime = new DateTime(2024, 10, 2),
					Nomenclature = nomenclature,
					Amount = 5,
					ReceiptWarehouse = warehouse
				};
				uow.Save(operation2);
				
				var operation3 = new WarehouseOperation {
					OperationTime = new DateTime(2024, 10, 15),
					Nomenclature = nomenclature,
					Amount = 2,
					ExpenseWarehouse = warehouse
				};
				uow.Save(operation3);

				uow.Commit();

				var balance1 = new StockRepository().StockBalances(uow, warehouse, new List<Nomenclature>{nomenclature}, new DateTime(2024, 10, 1));
				var item1 = balance1.First();
				Assert.That(item1.Amount, Is.EqualTo(10));
				var balance2 = new StockRepository().StockBalances(uow, warehouse, new List<Nomenclature>{nomenclature}, new DateTime(2024, 10, 2));
				var item2 = balance2.First();
				Assert.That(item2.Amount, Is.EqualTo(15));
				var balance3 = new StockRepository().StockBalances(uow, warehouse, new List<Nomenclature>{nomenclature}, new DateTime(2024, 10, 3));
				var item3 = balance3.First();
				Assert.That(item3.Amount, Is.EqualTo(15));
				var balance4 = new StockRepository().StockBalances(uow, warehouse, new List<Nomenclature>{nomenclature}, new DateTime(2024, 10, 17));
				var item4 = balance4.First();
				Assert.That(item4.Amount, Is.EqualTo(13));
			}
		}
				
		[Test(Description = "Проверяем что исключаются операции, которые не нужно учитывать")]
		[Category("Integrated")]
		[Category("Real case")]
		public void StockBalances_ExcludeOperation()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				warehouse.Name = "Единственный";
				uow.Save(warehouse);

				var nomenclature = new Nomenclature {
					Name = "Тестовая номенклатура"
				};
				uow.Save(nomenclature);

				var operation1 = new WarehouseOperation {
					OperationTime = new DateTime(2024, 10, 1),
					Nomenclature = nomenclature,
					Amount = 1,
					ReceiptWarehouse = warehouse,
				};
				uow.Save(operation1);
				
				var operation2 = new WarehouseOperation {
					OperationTime = new DateTime(2024, 10, 2),
					Nomenclature = nomenclature,
					Amount = 3,
					ReceiptWarehouse = warehouse
				};
				uow.Save(operation2);
				
				var operation3 = new WarehouseOperation {
					OperationTime = new DateTime(2024, 10, 3),
					Nomenclature = nomenclature,
					Amount = 5,
					ReceiptWarehouse = warehouse
				};
				uow.Save(operation3);

				uow.Commit();

				var balance1 = new StockRepository().StockBalances(uow, warehouse, new List<Nomenclature>{nomenclature}, new DateTime(2024, 10, 5));
				Assert.That(balance1.First().Amount, Is.EqualTo(9));
				
				var balance2 = new StockRepository().StockBalances(uow, warehouse, new List<Nomenclature>{nomenclature}, new DateTime(2024, 10, 5),
					new List<WarehouseOperation>(){operation1});
				Assert.That(balance2.First().Amount, Is.EqualTo(8));
				
				var balance3 = new StockRepository().StockBalances(uow, warehouse, new List<Nomenclature>{nomenclature}, new DateTime(2024, 10, 5),
					new List<WarehouseOperation>(){operation1,operation2,operation3});
				Assert.That(balance3.FirstOrDefault(), Is.Null);
			}
		}
	}
}
