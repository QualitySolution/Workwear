using System;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Testing.DB;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;

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

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				uow.Save(nomenclature);

				var position1 = new StockPosition(nomenclature, null, null, 0);

				var subdivision = new Subdivision();
				subdivision.Name = "Тествотове подразделение";
				uow.Save(subdivision);

				var place = new SubdivisionPlace();
				place.Name = "Тествое место";
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
				expense.AddItem(position1, 1);

				//Обновление операций
				expense.UpdateOperations(uow, ask);
				uow.Save(expense);
				uow.Commit();

				//FixMe Желательно здесь проверить баланс но сейчас такого кода просто нет для подразделений.
			}
		}
	}
}
