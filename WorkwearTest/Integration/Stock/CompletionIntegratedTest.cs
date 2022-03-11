using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using QS.Testing.DB;
using workwear.Domain.Stock;
using workwear.ViewModels.Stock;
using NSubstitute;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Validation;
using workwear.Repository.Stock;
using workwear.Tools;
using workwear.Tools.Features;

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
                var warehouseSource = new Warehouse() {Name = "Склад комплектующих"};
                uow.Save(warehouseSource);
                var warehouseResult = new Warehouse() {Name = "Склад получение"};
                uow.Save(warehouseResult);
                var nomenclatureType = new ItemsType {Name = "Тестовый тип номенклатуры"};
                uow.Save(nomenclatureType);

                var nomenclature1 = new Nomenclature {Type = nomenclatureType, Name = "Комплектующий"};
                uow.Save(nomenclature1);
                var nomenclature2 = new Nomenclature {Type = nomenclatureType, Name = "Результат"};
                uow.Save(nomenclature2);

                var income1 = new Income {
                    Warehouse = warehouseSource,
                    Date = new DateTime(2017, 1, 1),
                    Operation = IncomeOperations.Enter
                };
                var incomeItem1 = income1.AddItem(nomenclature1);
                incomeItem1.Size = "X";
                incomeItem1.Amount = 10;
                income1.UpdateOperations(uow, interactive);
                uow.Save(income1);

                var income2 = new Income {
                    Warehouse = warehouseResult,
                    Date = new DateTime(2017, 1, 1),
                    Operation = IncomeOperations.Enter
                };
                var income2Item1 = income2.AddItem(nomenclature2);
                income2Item1.Size = "X";
                income2Item1.Amount = 7;
                income2.UpdateOperations(uow, interactive);
                uow.Save(income2);
				
                uow.Commit();
                var stockRepository = new StockRepository();
                var stock1 = stockRepository.StockBalances(uow, warehouseSource, new List<Nomenclature> { nomenclature1 }, new DateTime(2017, 1, 2));
                Assert.That(stock1.Sum(x => x.Amount), Is.EqualTo(10));
                var stock2 = stockRepository.StockBalances(uow, warehouseResult, new List<Nomenclature> { nomenclature2 }, new DateTime(2017, 1, 2));
                Assert.That(stock2.Sum(x => x.Amount), Is.EqualTo(7));
                
                var completion = new Completion() {Date = new DateTime(2017, 1, 3), ResultWarehouse = warehouseResult};
                completion.AddSourceItem(stock1.First().StockPosition, warehouseSource, 5);
                var itemResult = completion.AddResultItem(nomenclature2);
                itemResult.Amount = 5;
                uow.Save(completion);
                completion.UpdateItems();
                uow.Commit();
                
                var stock3 = stockRepository.StockBalances(uow, warehouseSource, new List<Nomenclature> { nomenclature1 }, new DateTime(2017, 1, 4));
                Assert.That(stock3.Sum(x => x.Amount), Is.EqualTo(5));
                var stock4 = stockRepository.StockBalances(uow, warehouseResult, new List<Nomenclature> { nomenclature2 }, new DateTime(2017, 1, 4));
                Assert.That(stock4.Sum(x => x.Amount), Is.EqualTo(12));
            }
        }
    }
}