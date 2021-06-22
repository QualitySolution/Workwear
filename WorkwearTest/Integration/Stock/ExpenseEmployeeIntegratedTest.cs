using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Testing.DB;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Tools;
using Workwear.Domain.Regulations;

namespace WorkwearTest.Integration.Stock
{
	[TestFixture(TestOf = typeof(Expense), Description = "Выдача сотруднику")]
	public class ExpenseEmployeeIntegratedTest : InMemoryDBGlobalConfigTestFixtureBase
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
		}

		[TearDown]
		public void TestTearDown()
		{
		}

		[Test(Description = "Корректно обрабатываем выдачу одной номенклатуры несколько раз за день. Реальный баг.")]
		[Category("real case")]
		[Category("Integrated")]
		public void UpdateOperations_IssuingMultipleRows_TwoNomeclatureSameNeedsTest()
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

				var nomenclature2 = new Nomenclature();
				nomenclature2.Type = nomenclatureType;
				uow.Save(nomenclature2);

				var position2 = new StockPosition(nomenclature2, null, null, 0);

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "СИЗ для тестирования";
				protectionTools.AddNomeclature(nomenclature);
				protectionTools.AddNomeclature(nomenclature2);
				uow.Save(protectionTools);

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
				expense.Operation = ExpenseOperations.Employee;
				expense.Warehouse = warehouse;
				expense.Employee = employee;
				expense.Date = new DateTime(2018, 10, 22);
				expense.AddItem(position1, 1);
				expense.AddItem(position2, 1);

				var baseParameters = Substitute.For<BaseParameters>();
				baseParameters.ColDayAheadOfShedule.Returns(0);

				//Обновление операций
				expense.UpdateOperations(uow, baseParameters, ask);
				uow.Save(expense);
				uow.Commit();

				expense.UpdateEmployeeWearItems();

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

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "СИЗ для тестирования";
				protectionTools.AddNomeclature(nomenclature);
				uow.Save(protectionTools);

				var position1 = new StockPosition(nomenclature, null, null, 0);
				var position2 = new StockPosition(nomenclature, "XL", null, 0);

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
				income.UpdateOperations(uow, ask);
				uow.Save(income);

				var income2 = new Income();
				income2.Warehouse = warehouse;
				income2.Date = new DateTime(2018, 1, 1);
				income2.Operation = IncomeOperations.Enter;
				var incomeItem2 = income2.AddItem(nomenclature);
				incomeItem2.Size = "XL";
				incomeItem2.Amount = 5;
				income2.UpdateOperations(uow, ask);
				uow.Save(income2);

				var expense = new Expense();
				expense.Operation = ExpenseOperations.Employee;
				expense.Warehouse = warehouse;
				expense.Employee = employee;
				expense.Date = new DateTime(2018, 10, 22);
				expense.AddItem(position1, 1);
				expense.AddItem(position2, 1);

				//Обновление операций
				expense.UpdateOperations(uow, baseParameters, ask); //Здесь 2020 
				uow.Save(expense);
				uow.Commit();

				expense.UpdateOperations(uow, baseParameters, ask); //Здесь 2022(неправильно)
				uow.Save(expense);
				uow.Commit();

				expense.UpdateOperations(uow, baseParameters, ask); //Здесь 2024(неправильно)
				uow.Save(expense);
				uow.Commit();

				employee.UpdateNextIssue(expense.Items.Select(x => x.ProtectionTools).ToArray());

				//Тут ожидаем предложение перенести дату использование второй номенклатуры на год.
				ask.ReceivedWithAnyArgs().Question(String.Empty);

				Assert.That(employee.WorkwearItems[0].NextIssue,
					Is.EqualTo(new DateTime(2020, 10, 22))
				);
			}
		}

		[Test(Description = "Убеждаемся что корректно рассчитываем дату следущей выдачи при норме в 1 месяц. При разных id. Реальный баг был втом что проверялись id не тех сущьностей, но вы тестах id одинаковые, поэтому тесты работали..")]
		[Category("real case")]
		[Category("Integrated")]
		public void UpdateEmployeeWearItems_NextIssueDiffIdsTest()
		{
			NewSessionWithSameDB();
			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var nomenclatureType = new ItemsType();
				nomenclatureType.Name = "Тестовый тип номенклатуры";
				uow.Save(nomenclatureType);

				//Поднимаем id номеклатуры до 2.
				uow.Save(new Nomenclature());

				var nomenclature = new Nomenclature();
				nomenclature.Type = nomenclatureType;
				uow.Save(nomenclature);

				var position1 = new StockPosition(nomenclature, null, null, 0);

				//Поднимаем id сиза до 3.
				uow.Save(new ProtectionTools { Name = "Id = 1" });
				uow.Save(new ProtectionTools { Name = "Id = 2" });

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "СИЗ для тестирования";
				protectionTools.AddNomeclature(nomenclature);
				uow.Save(protectionTools);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Month;
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
				income.UpdateOperations(uow, ask);
				uow.Save(income);

				var expense = new Expense();
				expense.Operation = ExpenseOperations.Employee;
				expense.Warehouse = warehouse;
				expense.Employee = employee;
				expense.Date = new DateTime(2018, 10, 22);
				expense.AddItem(position1, 1);

				var baseParameters = Substitute.For<BaseParameters>();
				baseParameters.ColDayAheadOfShedule.Returns(0);

				//Обновление операций
				expense.UpdateOperations(uow, baseParameters, ask);
				uow.Save(expense);
				uow.Commit();

				expense.UpdateEmployeeWearItems();
				uow.Commit();

				using(var uow2 = UnitOfWorkFactory.CreateWithoutRoot()) {
					var employeeTest = uow2.GetById<EmployeeCard>(employee.Id);
					Assert.That(employeeTest.WorkwearItems[0].NextIssue, Is.EqualTo(new DateTime(2018, 11, 22)));
				}
			}
		}

		[Test(Description = "Убеждаемся что на созданную выдачу можем добавить ведомость.")]
		[Category("Integrated")]
		public void IssueAndCreateIssuanceSheetTest()
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

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "СИЗ для тестирования";
				protectionTools.AddNomeclature(nomenclature);
				uow.Save(protectionTools);

				var position1 = new StockPosition(nomenclature, null, null, 0);

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
				income.UpdateOperations(uow, ask);
				uow.Save(income);

				var expense = new Expense();
				expense.Operation = ExpenseOperations.Employee;
				expense.Warehouse = warehouse;
				expense.Employee = employee;
				expense.Date = new DateTime(2018, 10, 22);
				var expenseItem = expense.AddItem(position1, 1);

				expense.CreateIssuanceSheet();

				//Обновление операций
				expense.UpdateOperations(uow, baseParameters, ask);
				expense.UpdateIssuanceSheet();
				uow.Save(expense.IssuanceSheet);
				uow.Save(expense);
				uow.Commit();
				expense.UpdateEmployeeWearItems();
				uow.Commit();

				Assert.That(expense.IssuanceSheet, Is.Not.Null);
				var issuanceItem = expense.IssuanceSheet.Items.First();
				Assert.That(issuanceItem.ExpenseItem, Is.EqualTo(expenseItem));
			}
		}

	}
}
