using System;
using NUnit.Framework;
using QS.Testing.DB;
using workwear.Domain.Stock;
using workwear.Repository.Stock;

namespace WorkwearTest.Integration.Stock
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

		[Test(Description = "Если в справочнике складов более одного склада, мы не должны возвращать склад по умолчанию.")]
		[Category("Integrated")]
		public void GetDefaultWarehouse_ManyWarehousesTest()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse1 = new Warehouse();
				uow.Save(warehouse1);

				var warehouse2 = new Warehouse();
				uow.Save(warehouse2);
				uow.Commit();

				var defaultWarehouse = new StockRepository().GetDefaultWarehouse(uow);
				Assert.That(defaultWarehouse, Is.Null);
			}
		}

		[Test(Description = "Если в справочнике складов один склад, возвращаем его как склад по умолчанию.")]
		[Category("Integrated")]
		public void GetDefaultWarehouse_OneWarehouseTest()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse1 = new Warehouse();
				warehouse1.Name = "Единственный";
				uow.Save(warehouse1);
				uow.Commit();

				var defaultWarehouse = new StockRepository().GetDefaultWarehouse(uow);
				Assert.That(defaultWarehouse.Name, Is.EqualTo("Единственный"));
			}
		}
	}
}
