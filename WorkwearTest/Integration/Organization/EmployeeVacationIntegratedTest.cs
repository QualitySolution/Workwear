using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Testing.DB;
using System;
using System.Linq;
using workwear.Domain.Organization;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;

namespace WorkwearTest.Integration.Organization
{
	[TestFixture(TestOf = typeof(EmployeeVacation))]
	public class EmployeeVacationIntegratedTest : InMemoryDBGlobalConfigTestFixtureBase
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

		[Test(Description = "Пересчитываем сроки после создания отпуска. Реальный баг.")]
		[Category("real case")]
		[Category("Integrated")]
		public void UpdateOperations_RecalculeteDatesAfterCreateVacation()
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
				expense.Date = new DateTime(2018, 5, 10);
				expense.Operation = ExpenseOperations.Employee;
				expense.AddItem(incomeItem1, 1);
				expense.AddItem(incomeItem2, 1);

				//Обновление операций
				expense.UpdateOperations(uow, ask);
				uow.Save(expense);
				uow.Commit();

				expense.UpdateEmployeeNextIssue();
				uow.Commit();

				Assert.That(employee.WorkwearItems[0].NextIssue,
					Is.EqualTo(new DateTime(2020, 5, 10))
				);

				//Добавляем новый отпуск на 10 дней.
				var vacationType = new VacationType();
				vacationType.Name = "Тестовый отпуск";
				vacationType.ExcludeFromWearing = true;

				var vacation = new EmployeeVacation();
				vacation.BeginDate = new DateTime(2019, 2, 1);
				vacation.EndDate = new DateTime(2019, 2, 10);
				vacation.VacationType = vacationType;
				employee.AddVacation(vacation);
				vacation.UpdateRelatedOperations(uow, ask);
				uow.Save(vacationType);
				uow.Save(vacation);
				uow.Save(employee);

				Assert.That(employee.WorkwearItems[0].NextIssue,
					Is.EqualTo(new DateTime(2020, 5, 20))
				);
			}
		}
	}
}
