using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Testing.DB;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Repository.Company;
using workwear.Tools;

namespace WorkwearTest.Integration.Stock
{
	[TestFixture(TestOf = typeof(Writeoff), Description = "Списание с сотрудника")]
	public class WriteoffEmployeeIntegratedTest : InMemoryDBGlobalConfigTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init()
		{
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
		}

		[Test(Description = "Проверяем что процесс списания в целом работает")]
		[Category("Integrated")]
		public void WriteoffMainTest()
		{
			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);
			var baseParameters = Substitute.For<BaseParameters>();

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

				var protectionTools = new ProtectionTools();
				protectionTools.Name = "СИЗ для тестирования";
				protectionTools.AddNomeclature(nomenclature);
				uow.Save(protectionTools);

				var employee = new EmployeeCard();
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
				var item = expense.AddItem(position1, 3);

				//Обновление операций
				expense.UpdateOperations(uow, baseParameters, ask);
				uow.Save(expense);
				uow.Commit();

				var employeeRepository = new EmployeeRepository(uow);
				var balance = employeeRepository.ItemsBalance(employee, new DateTime(2018, 10, 30));
				Assert.That(balance.First().Amount, Is.EqualTo(3));

				//Списываем
				var writeoff = new Writeoff();
				writeoff.Date = new DateTime(2018, 10, 25);
				writeoff.AddItem(item.EmployeeIssueOperation, 1);

				//Обновление операций
				writeoff.UpdateOperations(uow);
				uow.Save(writeoff);
				uow.Commit();

				var balanceAfter = employeeRepository.ItemsBalance(employee, new DateTime(2018, 10, 30));
				Assert.That(balanceAfter.First().Amount, Is.EqualTo(2));

			}
		}
	}
}
