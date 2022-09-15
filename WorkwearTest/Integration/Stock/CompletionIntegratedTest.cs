using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QS.Testing.DB;
using Workwear.Domain.Stock;
using NSubstitute;
using QS.Dialog;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock.Documents;
using Workwear.Repository.Stock;

namespace WorkwearTest.Integration.Stock
{
    [TestFixture(TestOf = typeof(Expense), Description = "Комплектация товара")]
    public class CompletionIntegratedTest : InMemoryDBGlobalConfigTestFixtureBase
    {
        [OneTimeSetUp]
        public void Init()
        {
            ConfigureOneTime.ConfigureNh();
            InitialiseUowFactory();
        }
        [Test(Description = "Прогоняем базовый сценарий")]
        [Category("Integrated")]
        public void ViewModelStandartScriptTest()
        {
            var interactive = Substitute.For<IInteractiveQuestion>();
            interactive.Question(string.Empty).ReturnsForAnyArgs(true);
            
            using (var uow = UnitOfWorkFactory.CreateWithoutRoot())
            {
                //Ложим на склад первоначальные остатки;
                var warehouseSource = new Warehouse {Name = "Склад комплектующих"};
                uow.Save(warehouseSource);
                var warehouseResult = new Warehouse {Name = "Склад получение"};
                uow.Save(warehouseResult);
                var sizeType = new SizeType();
                uow.Save(sizeType);
                var nomenclatureType = new ItemsType {Name = "Тестовый тип номенклатуры", SizeType = sizeType};
                uow.Save(nomenclatureType);

                var nomenclature1 = new Nomenclature {Type = nomenclatureType, Name = "Комплектующий"};
                uow.Save(nomenclature1);
                var nomenclature2 = new Nomenclature {Type = nomenclatureType, Name = "Результат"};
                uow.Save(nomenclature2);
                
                var sizeX = new Size {Name = "X", SizeType = sizeType};
                uow.Save(sizeX);
                var sizeXl = new Size {Name = "XL", SizeType = sizeType};
                uow.Save(sizeXl);

                var income1 = new Income {
                    Warehouse = warehouseSource,
                    Date = new DateTime(2017, 1, 1),
                    Operation = IncomeOperations.Enter
                };
                var incomeItem1 = income1.AddItem(nomenclature1);
                incomeItem1.WearSize = sizeX;
                incomeItem1.Amount = 10;
                income1.UpdateOperations(uow, interactive);
                uow.Save(income1);

                var income2 = new Income {
                    Warehouse = warehouseResult,
                    Date = new DateTime(2017, 1, 1),
                    Operation = IncomeOperations.Enter
                };
                var income2Item1 = income2.AddItem(nomenclature2);
                income2Item1.WearSize = sizeXl;
                income2Item1.Amount = 7;
                income2.UpdateOperations(uow, interactive);
                uow.Save(income2);
                uow.Commit();
                
                var stockRepository = new StockRepository();
                var stock1 = stockRepository
                    .StockBalances(uow, warehouseSource, new List<Nomenclature> { nomenclature1 }, new DateTime(2017, 1, 2));
                Assert.That(stock1.Where(x => x.WearSize == sizeX).Sum(x => x.Amount), Is.EqualTo(10));
                
                var stock2 = stockRepository
                    .StockBalances(uow, warehouseResult, new List<Nomenclature> { nomenclature2 }, new DateTime(2017, 1, 2));
                Assert.That(stock2.Where(x => x.WearSize == sizeXl).Sum(x => x.Amount), Is.EqualTo(7));
                
                //Создаём комплектацию 5 ед. комплектующих в 5 ед. результата
                var completion = new Completion {Date = new DateTime(2017, 1, 3), 
                    ResultWarehouse = warehouseResult};
                completion.AddSourceItem(stock1.First(x => x.WearSize == sizeX).StockPosition, warehouseSource, 5);
                var itemResult = completion.AddResultItem(nomenclature2);
                itemResult.WearSize = sizeXl;
                itemResult.Amount = 5;
                completion.UpdateItems();
                uow.Save(completion);
                uow.Commit();
                
                var stockSource = stockRepository
                    .StockBalances(uow, warehouseSource, new List<Nomenclature> { nomenclature1 }, new DateTime(2017, 1, 4));
                Assert.That(stockSource.Where(x => x.WearSize == sizeX).Sum(x => x.Amount), Is.EqualTo(5));
                
                var stockResult = stockRepository
                    .StockBalances(uow, warehouseResult, new List<Nomenclature> { nomenclature2 }, new DateTime(2017, 1, 4));
                Assert.That(stockResult.Where(x => x.WearSize == sizeXl).Sum(x => x.Amount), Is.EqualTo(12));
            }
        }
    }
}
