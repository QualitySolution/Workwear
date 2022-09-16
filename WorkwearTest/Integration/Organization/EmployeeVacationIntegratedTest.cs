using System;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Testing.DB;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Tools;
using Workwear.Domain.Stock.Documents;

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

		[Test(Description = "Пересчитываем правильно сроки после создания отпуска. Реальный баг.")]
		[Category("real case")]
		[Category("Integrated")]
		public void UpdateOperations_RecalculeteDatesAfterCreateVacation()
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
				nomenclature.Type = nomenclatureType;
				uow.Save(nomenclature);

				var position1 = new StockPosition(nomenclature, 0, null, null);

				var nomenclature2 = new Nomenclature();
				nomenclature2.Type = nomenclatureType;
				uow.Save(nomenclature2);

				var protectionTools = new ProtectionTools();
				protectionTools.AddNomeclature(nomenclature);
				protectionTools.AddNomeclature(nomenclature2);
				protectionTools.Name = "СИЗ для тестирования";
				uow.Save(protectionTools);

				var position2 = new StockPosition(nomenclature2, 0, null, null);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Year;
				normItem.PeriodCount = 1;
				uow.Save(norm);

				var employee = new EmployeeCard();
				employee.AddUsedNorm(norm);
				uow.Save(employee);
				uow.Commit();

				var income = new Income();
				income.Warehouse = warehouse;
				income.Date = new DateTime(2017, 1, 1);
				income.Operation = IncomeOperations.Enter;
				var incomeItem1 = income.AddItem(nomenclature);
				incomeItem1.Amount = 10;
				var incomeItem2 = income.AddItem(nomenclature2);
				incomeItem2.Amount = 5;
				income.UpdateOperations(uow, ask);
				uow.Save(income);

				var expense = new Expense();
				expense.Warehouse = warehouse;
				expense.Employee = employee;
				expense.Date = new DateTime(2018, 5, 10);
				expense.Operation = ExpenseOperations.Employee;
				expense.AddItem(position1, 1);
				expense.AddItem(position2, 1);

				//Обновление операций
				expense.UpdateOperations(uow, baseParameters, ask);
				uow.Save(expense);
				uow.Commit();

				expense.UpdateEmployeeWearItems();
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
				vacation.UpdateRelatedOperations(uow, new Workwear.Repository.Operations.EmployeeIssueRepository(), baseParameters, ask);
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
