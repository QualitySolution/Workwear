using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Testing.DB;
using System;
using System.Linq;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;

namespace WorkwearTest.Integration.Stock
{
	[TestFixture(TestOf = typeof(Expense))]
	public class ExpenseIntegratedTest : InMemoryDBGlobalConfigTestFixtureBase
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
			//NewSessionWithSameDB();
		}

		[TearDown]
		public void TestTearDown()
		{
			//NewSessionWithNewDB();
		}


		[Test(Description = "Корректно обрабатываем выдачу одной номенклатуры несколько раз за день. Реальный баг.")]
		[Category("real case")]
		[Category("Integrated")]
		public void UpdateOperations_IssuingMultipleRows_TwoNomeclatureSameTypeTest()
		{
			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				uow.Save(nomenclature);

				var nomenclature2 = new Nomenclature();
				nomenclature2.Type = nomenclatureType;
				uow.Save(nomenclature2);

				var norm = new Norm();
				var normItem = norm.AddItem(nomenclatureType);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Year;
				normItem.PeriodCount = 1;
				uow.Save(norm);

				var employee = new EmployeeCard();
				employee.AddUsedNorm(norm);
				uow.Save(employee);
				uow.Commit();

				var income = new Income();
				income.Date = new DateTime(2017, 1, 1);
				income.Operation = IncomeOperations.Enter;
				var incomeItem1 = income.AddItem(nomenclature);
				incomeItem1.Amount = 10;

				var incomeItem2 = income.AddItem(nomenclature2);
				incomeItem2.Amount = 5;
				uow.Save(income);

				var expense = new Expense();
				expense.Employee = employee;
				expense.Date = new DateTime(2018, 10, 22);
				expense.Operation = ExpenseOperations.Employee;
				expense.AddItem(incomeItem1, 1);
				expense.AddItem(incomeItem2, 1);

				//Обновление операций
				expense.UpdateOperations(uow, ask);
				uow.Save(expense);
				uow.Commit();

				expense.UpdateEmployeeNextIssue();

				//Тут ожидаем предложение перенести дату использование второй номенклатуры на год.
				ask.ReceivedWithAnyArgs().Question(String.Empty);

				Assert.That(employee.WorkwearItems[0].NextIssue,
					Is.EqualTo(new DateTime(2020, 10, 22))
				);
			}
		}

		[Test(Description = "Не увеличивать дату при повторных вызовах. Реальный баг.")]
		[Category("real case")]
		[Category("Integrated")]
		public void UpdateOperations_DoNotIncreaseDateWhenRepeatedCallsTest()
		{
			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				uow.Save(nomenclature);

				var norm = new Norm();
				var normItem = norm.AddItem(nomenclatureType);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Year;
				normItem.PeriodCount = 1;
				uow.Save(norm);

				var employee = new EmployeeCard();
				employee.AddUsedNorm(norm);
				uow.Save(employee);
				uow.Commit();

				var income = new Income();
				income.Date = new DateTime(2017, 1, 1);
				income.Operation = IncomeOperations.Enter;
				var incomeItem1 = income.AddItem(nomenclature);
				incomeItem1.Amount = 10;
				income.UpdateOperations(uow, ask);
				uow.Save(income);

				var income2 = new Income();
				income2.Date = new DateTime(2018, 1, 1);
				income2.Operation = IncomeOperations.Enter;
				var incomeItem2 = income2.AddItem(nomenclature);
				incomeItem2.Amount = 5;
				income2.UpdateOperations(uow, ask);
				uow.Save(income2);

				var expense = new Expense();
				expense.Employee = employee;
				expense.Date = new DateTime(2018, 10, 22);
				expense.Operation = ExpenseOperations.Employee;
				expense.AddItem(position1, 1);
				expense.AddItem(position1, 1);

				//Обновление операций
				expense.UpdateOperations(uow, ask); //Здесь 2020 
				uow.Save(expense);
				uow.Commit();

				expense.UpdateOperations(uow, ask); //Здесь 2022(неправильно)
				uow.Save(expense);
				uow.Commit();

				expense.UpdateOperations(uow, ask); //Здесь 2024(неправильно)
				uow.Save(expense);
				uow.Commit();

				employee.UpdateNextIssue(expense.Items.Select(x => x.Nomenclature.Type).ToArray());

				//Тут ожидаем предложение перенести дату использование второй номенклатуры на год.
				ask.ReceivedWithAnyArgs().Question(String.Empty);

				Assert.That(employee.WorkwearItems[0].NextIssue,
					Is.EqualTo(new DateTime(2020, 10, 22))
				);
			}
		}
	}
}
