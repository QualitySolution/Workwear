using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using workwear.Domain.Operations;
using workwear.Domain.Operations.Graph;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;

namespace WorkwearTest.Integration.EmployeeIssue
{
	[TestFixture(TestOf =typeof(Expense))]
	public class ExpenseTest
	{
		[Test(Description = "Мы должны проигнорировать собственную операцию при рассчете и не предлагать пользователю сдвинуть дату начала использования.")]
		public void UpdateOperations_IgnoreSelfOperationsWhenChangeDateOfDocument()
		{
			var uow = Substitute.For<IUnitOfWork>();
			var norm = Substitute.For<NormItem>();
			norm.Amount.Returns(1);
			var incomeOn = Substitute.For<IncomeItem>();
			var nomeclature = Substitute.For<Nomenclature>();

			var operation = new EmployeeIssueOperation();
			operation.OperationTime = new DateTime(2019, 1, 15);
			operation.NormItem = norm;

			IssueGraph.MakeIssueGraphTestGap = (e, t) => new IssueGraph(new List<EmployeeIssueOperation>() { operation });

			var expenseItem = new ExpenseItem();
			expenseItem.Nomenclature = nomeclature;
			expenseItem.EmployeeIssueOperation = operation;
			expenseItem.Amount = 1;
			expenseItem.IncomeOn = incomeOn;
			var expense = new Expense();
			expense.Date = new DateTime(2019, 1, 15);
			expense.Operation = ExpenseOperations.Employee;
			expense.Items.Add(expenseItem);
			expenseItem.ExpenseDoc = expense;

			//Выполняем
			expense.UpdateOperations(uow, s => { 
				Assert.Fail("В данном сценарии мы не должны ничего спрашивать у пользователя. Предпологается что мы могли попросить передвинуть дату начала, если бы не проигнорировали свою же операцию.");
				return true;
			});
			Assert.That(expense.Items[0].EmployeeIssueOperation.OperationTime,
				Is.EqualTo(new DateTime(2019, 1, 15))
			);
		}

		[Test(Description = "Мы не должны передвигать дату начала использования, если в последнем интервале СИЗ еще числится в достаточном количестве потому что нет автосписания.")]
		public void UpdateOperations_DontMoveDateIfAtLastDateAmountEnough()
		{
			var uow = Substitute.For<IUnitOfWork>();
			var norm = Substitute.For<NormItem>();
			norm.Amount.Returns(1);
			var incomeOn = Substitute.For<IncomeItem>();
			var nomeclature = Substitute.For<Nomenclature>();
			var operationBeforeAndEnough = Substitute.For<EmployeeIssueOperation>();
			operationBeforeAndEnough.OperationTime.Returns(new DateTime(2018, 10, 15));
			operationBeforeAndEnough.Issued.Returns(2);

			var operation = new EmployeeIssueOperation();
			operation.OperationTime = new DateTime(2019, 1, 15);
			operation.NormItem = norm;

			IssueGraph.MakeIssueGraphTestGap = (e, t) => new IssueGraph(new List<EmployeeIssueOperation>() { operation, operationBeforeAndEnough });

			var expenseItem = new ExpenseItem();
			expenseItem.Nomenclature = nomeclature;
			expenseItem.EmployeeIssueOperation = operation;
			expenseItem.Amount = 1;
			expenseItem.IncomeOn = incomeOn;
			var expense = new Expense();
			expense.Date = new DateTime(2019, 1, 15);
			expense.Operation = ExpenseOperations.Employee;
			expense.Items.Add(expenseItem);
			expenseItem.ExpenseDoc = expense;

			//Выполняем
			expense.UpdateOperations(uow, s => {
				Assert.Fail("В данном сценарии мы не должны ничего спрашивать у пользователя. Предпологается что мы могли попросить передвинуть дату начала, не учитывая что на конец интервала количество все равно достаточное.");
				return true;
			});
			Assert.That(expense.Items[0].EmployeeIssueOperation.OperationTime,
				Is.EqualTo(new DateTime(2019, 1, 15))
			);
		}

		[TearDown]
		public void RemoveStaticGaps()
		{
			IssueGraph.MakeIssueGraphTestGap = null;
		}
	}
}
