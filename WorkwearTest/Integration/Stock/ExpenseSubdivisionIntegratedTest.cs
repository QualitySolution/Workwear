using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Testing.DB;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using workwear.Domain.Stock;
using workwear.Repository;
using workwear.Tools;
using workwear.Repository.Company;

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

				var sizeType = new SizeType();
				uow.Save(sizeType);

				var nomenclatureType = new ItemsType {
					Name = "Тестовый тип номенклатуры",
					SizeType = sizeType
				};
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature {
					Name = "Тестовая номеклатура",
					Type = nomenclatureType
				};
				uow.Save(nomenclature);

				var size = new Size {SizeType = sizeType};
				var height = new Size();
				uow.Save(size);
				uow.Save(height);

				var position1 = new StockPosition(nomenclature, 0, size, height);

				var subdivision = new Subdivision {
					Name = "Тестовое подразделение"
				};
				uow.Save(subdivision);

				var place = new SubdivisionPlace {
					Name = "Тестовое место",
					Subdivision = subdivision
				};
				uow.Save(place);

				var income = new Income {
					Warehouse = warehouse,
					Date = new DateTime(2017, 1, 1),
					Operation = IncomeOperations.Enter
				};
				var incomeItem1 = income.AddItem(nomenclature);
				incomeItem1.Amount = 10;
				income.UpdateOperations(uow, ask);
				uow.Save(income);

				var expense = new Expense {
					Operation = ExpenseOperations.Object,
					Warehouse = warehouse,
					Subdivision = subdivision,
					Date = new DateTime(2018, 10, 22)
				};
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
