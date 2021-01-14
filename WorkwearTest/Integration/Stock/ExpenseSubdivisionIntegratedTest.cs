using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Testing.DB;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Repository;
using workwear.Tools;

namespace WorkwearTest.Integration.Stock
{
	[TestFixture(TestOf = typeof(Expense), Description = "Выдача сотруднику")]
	public class ExpenseSubdivisionIntegratedTest : InMemoryDBGlobalConfigTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init()
		{
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
		}

		[Test(Description = "Проверяем что в принцепе можем выдать номеклатуру на подразделение.")]
		[Category("Integrated")]
		public void IssuingToSubdivisionTest()
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
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Name = "Тестовая номеклатура";
				nomenclature.Type = nomenclatureType;
				uow.Save(nomenclature);

				var position1 = new StockPosition(nomenclature, null, null, 0);

				var subdivision = new Subdivision();
				subdivision.Name = "Тестовое подразделение";
				uow.Save(subdivision);

				var place = new SubdivisionPlace();
				place.Name = "Тестовое место";
				place.Subdivision = subdivision;
				uow.Save(place);

				var income = new Income();
				income.Warehouse = warehouse;
				income.Date = new DateTime(2017, 1, 1);
				income.Operation = IncomeOperations.Enter;
				var incomeItem1 = income.AddItem(nomenclature);
				incomeItem1.Amount = 10;
				income.UpdateOperations(uow, ask);
				uow.Save(income);

				var expense = new Expense();
				expense.Operation = ExpenseOperations.Object;
				expense.Warehouse = warehouse;
				expense.Subdivision = subdivision;
				expense.Date = new DateTime(2018, 10, 22);
				var item1 = expense.AddItem(position1, 1);
				item1.SubdivisionPlace = place;

				//Обновление операций
				expense.UpdateOperations(uow, baseParameters, ask);
				uow.Save(expense);
				uow.Commit();

				var listed = SubdivisionRepository.ItemsBalance(uow, subdivision);
				var balance = listed.First();
				Assert.That(balance.Amount, Is.EqualTo(1));
				Assert.That(balance.NomeclatureName, Is.EqualTo("Тестовая номеклатура"));
				Assert.That(balance.Place, Is.EqualTo("Тестовое место"));
				Assert.That(balance.WearPercent, Is.EqualTo(0m));
			}
		}
	}
}
