﻿using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Testing.DB;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Repository.Operations;
using Workwear.Tools;

namespace Workwear.Test.Integration.Stock
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
			var ask = Substitute.For<IInteractiveService>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);
			var baseParameters = Substitute.For<BaseParameters>();

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var nomenclatureType = new ItemsType {
					Name = "Тестовый тип номенклатуры"
				};
				uow.Save(nomenclatureType);

				var nomenclature = new Nomenclature {
					Type = nomenclatureType
				};
				uow.Save(nomenclature);

				var size = new Size();
				var height = new Size();
				uow.Save(size);
				uow.Save(height);

				var position1 = new StockPosition(nomenclature, 0, size, height, null);

				var protectionTools = new ProtectionTools {
					Name = "СИЗ для тестирования"
				};
				protectionTools.AddNomenclature(nomenclature);
				uow.Save(protectionTools);

				var employee = new EmployeeCard();
				uow.Save(employee);
				uow.Commit();

				var income = new Income {
					Warehouse = warehouse,
					Date = new DateTime(2017, 1, 1),
					Operation = IncomeOperations.Enter
				};
				var incomeItem1 = income.AddItem(nomenclature, ask);
				incomeItem1.Amount = 10;
				income.UpdateOperations(uow, ask);
				uow.Save(income);

				var expense = new Expense {
					Warehouse = warehouse,
					Employee = employee,
					Date = new DateTime(2018, 10, 22)
				};
				var item = expense.AddItem(position1, 3);

				//Обновление операций
				expense.UpdateOperations(uow, baseParameters, ask);
				uow.Save(expense);
				uow.Commit();

				var employeeRepository = new EmployeeIssueRepository(uow);
				var balance = employeeRepository.ItemsBalance(employee, new DateTime(2018, 10, 30));
				Assert.That(balance.First().Amount, Is.EqualTo(3));

				//Списываем
				var writeoff = new Writeoff {
					Date = new DateTime(2018, 10, 25)
				};
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
