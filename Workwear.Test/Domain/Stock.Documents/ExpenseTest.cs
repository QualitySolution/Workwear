using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Repository.Operations;
using Workwear.Tools;

namespace Workwear.Test.Domain.Stock.Documents
{
	[TestFixture(TestOf =typeof(Expense))]
	public class ExpenseTest
	{
		#region UpdateOperations
		[Test(Description = "Мы должны проигнорировать собственную операцию при расчете и не предлагать пользователю сдвинуть дату начала использования.")]
		public void UpdateOperations_IgnoreSelfOperationsWhenChangeDateOfDocument()
		{
			var uow = Substitute.For<IUnitOfWork>();
			var employee = Substitute.For<EmployeeCard>();
			var norm = Substitute.For<NormItem>();
			norm.Amount.Returns(1);
			var incomeOperation = Substitute.For<EmployeeIssueOperation>();
			var nomenclature = Substitute.For<Nomenclature>();

			var operation = new EmployeeIssueOperation();
			operation.OperationTime = new DateTime(2019, 1, 15);
			operation.NormItem = norm;
			operation.IssuedOperation = incomeOperation;

			IssueGraph.MakeIssueGraphTestGap = (e, t) => new IssueGraph(new List<EmployeeIssueOperation>() { operation });

			var expenseItem = new ExpenseItem();
			expenseItem.Nomenclature = nomenclature;
			expenseItem.EmployeeIssueOperation = operation;
			expenseItem.Amount = 1;
			var expense = new Expense();
			expense.Employee = employee;
			expense.Date = new DateTime(2019, 1, 15);
			expense.Operation = ExpenseOperations.Employee;
			expense.Items.Add(expenseItem);
			expenseItem.ExpenseDoc = expense;

			var ask = Substitute.For<IInteractiveQuestion>();
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);

			//Выполняем
			expense.UpdateOperations(uow, baseParameters, ask);

			//В данном сценарии мы не должны ничего спрашивать у пользователя. Предполагается что мы могли попросить передвинуть дату начала, если бы не проигнорировали свою же операцию.
			ask.DidNotReceiveWithAnyArgs().Question(string.Empty);

			Assert.That(expense.Items[0].EmployeeIssueOperation.OperationTime,
				Is.EqualTo(new DateTime(2019, 1, 15))
			);
		}

		[Test(Description = "Мы не должны передвигать дату начала использования, если в последнем интервале СИЗ еще числится в достаточном количестве потому что нет автосписания.")]
		public void UpdateOperations_DontMoveDateIfAtLastDateAmountEnough()
		{
			var uow = Substitute.For<IUnitOfWork>();
			var employee = Substitute.For<EmployeeCard>();
			var norm = Substitute.For<NormItem>();
			norm.Amount.Returns(1);
			var nomenclature = Substitute.For<Nomenclature>();
			var operationBeforeAndEnough = Substitute.For<EmployeeIssueOperation>();
			operationBeforeAndEnough.OperationTime.Returns(new DateTime(2018, 10, 15));
			operationBeforeAndEnough.Issued.Returns(2);

			var operation = new EmployeeIssueOperation();
			operation.OperationTime = new DateTime(2019, 1, 15);
			operation.NormItem = norm;

			IssueGraph.MakeIssueGraphTestGap = (e, t) => new IssueGraph(new List<EmployeeIssueOperation>() { operation, operationBeforeAndEnough });

			var expenseItem = new ExpenseItem();
			expenseItem.Nomenclature = nomenclature;
			expenseItem.EmployeeIssueOperation = operation;
			expenseItem.Amount = 1;
			var expense = new Expense();
			expense.Employee = employee;
			expense.Date = new DateTime(2019, 1, 15);
			expense.Operation = ExpenseOperations.Employee;
			expense.Items.Add(expenseItem);
			expenseItem.ExpenseDoc = expense;

			var ask = Substitute.For<IInteractiveQuestion>();
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);

			//Выполняем
			expense.UpdateOperations(uow, baseParameters, ask);

			//В данном сценарии мы не должны ничего спрашивать у пользователя. Предпологается что мы могли попросить передвинуть дату начала, не учитывая что на конец интервала количество все равно достаточное.
			ask.DidNotReceiveWithAnyArgs().Question(string.Empty);

			Assert.That(expense.Items[0].EmployeeIssueOperation.OperationTime,
				Is.EqualTo(new DateTime(2019, 1, 15))
			);
		}

		[Test(Description = "Мы не должны предлагать передвигать дату начала использования, на тот же день, если выдача сделана в тот же день что и списание. Реальный баг.")]
		public void UpdateOperations_DontMoveDateIfWriteofAtIssueDate()
		{
			var uow = Substitute.For<IUnitOfWork>();
			var employee = Substitute.For<EmployeeCard>();
			var norm = Substitute.For<NormItem>();
			norm.Amount.Returns(1);
			var nomenclature = Substitute.For<Nomenclature>();

			var operationIssue = Substitute.For<EmployeeIssueOperation>();
			operationIssue.Id.Returns(1);
			operationIssue.OperationTime.Returns(new DateTime(2019, 1, 15));
			operationIssue.Issued.Returns(2);
			var operationWriteoff = Substitute.For<EmployeeIssueOperation>();
			operationWriteoff.Id.Returns(2);
			operationWriteoff.IssuedOperation.Returns(operationIssue);
			operationWriteoff.OperationTime.Returns(new DateTime(2019, 1, 15));
			operationWriteoff.Returned.Returns(2);

			var operation = new EmployeeIssueOperation();
			operation.OperationTime = new DateTime(2019, 1, 15);
			operation.NormItem = norm;

			IssueGraph.MakeIssueGraphTestGap = (e, t) => new IssueGraph(new List<EmployeeIssueOperation>() { operation, operationIssue, operationWriteoff });

			var expenseItem = new ExpenseItem();
			expenseItem.Nomenclature = nomenclature;
			expenseItem.EmployeeIssueOperation = operation;
			expenseItem.Amount = 1;
			var expense = new Expense();
			expense.Employee = employee;
			expense.Date = new DateTime(2019, 1, 15);
			expense.Operation = ExpenseOperations.Employee;
			expense.Items.Add(expenseItem);
			expenseItem.ExpenseDoc = expense;

			var ask = Substitute.For<IInteractiveQuestion>();
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);

			//Выполняем
			expense.UpdateOperations(uow, baseParameters, ask);

			//В данном сценарии мы не должны ничего спрашивать у пользователя. Предполагается что мы могли попросить передвинуть дату начала, на тот же день, так как списание было в этот день. реальный случай.
			ask.DidNotReceiveWithAnyArgs().Question(string.Empty);

			Assert.That(expense.Items[0].EmployeeIssueOperation.OperationTime,
				Is.EqualTo(new DateTime(2019, 1, 15))
			);
		}

		[Test(Description = "Проверяем что в операцию действительно проставляется подпись с карты скуд.")]
		public void UpdateOperations_SignCardKeyTest()
		{
			var uow = Substitute.For<IUnitOfWork>();
			var employee = Substitute.For<EmployeeCard>();
			employee.CardKey.Returns("80313E3A437A04");
			var norm = Substitute.For<NormItem>();
			norm.Amount.Returns(1);
			var incomeOperation = Substitute.For<EmployeeIssueOperation>();
			var nomenclature = Substitute.For<Nomenclature>();

			var warehouse = Substitute.For<Warehouse>();

			IssueGraph.MakeIssueGraphTestGap = (e, t) => new IssueGraph(new List<EmployeeIssueOperation>() { });

			var expenseItem = new ExpenseItem();
			expenseItem.Nomenclature = nomenclature;
			expenseItem.Amount = 1;
			var expense = new Expense();
			expense.Employee = employee;
			expense.Date = new DateTime(2019, 1, 15);
			expense.Operation = ExpenseOperations.Employee;
			expense.Warehouse = warehouse;
			expense.Items.Add(expenseItem);
			expenseItem.ExpenseDoc = expense;

			var ask = Substitute.For<IInteractiveQuestion>();
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);

			//Выполняем
			expense.UpdateOperations(uow, baseParameters, ask, "80313E3A437A04");

			//В данном сценарии мы не должны ничего спрашивать у пользователя. Предполагается что мы могли попросить передвинуть дату начала, если бы не проигнорировали свою же операцию.
			ask.DidNotReceiveWithAnyArgs().Question(string.Empty);

			Assert.That(expense.Items[0].EmployeeIssueOperation.SignCardKey, Is.EqualTo("80313E3A437A04"));
			Assert.That(expense.Items[0].EmployeeIssueOperation.SignTimestamp, Is.Not.Null);
			Assert.That((expense.Items[0].EmployeeIssueOperation.SignTimestamp.Value - DateTime.Now).TotalMinutes, Is.LessThan(1));
		}
		#endregion

		#region FillCanWriteoffInfo
		[Test(Description = "Проверяем что устанавливаем IsEnableWriteOff правильно")]
		public void FillCanWriteoffInfo_CommonCase() {
			var uow = Substitute.For<IUnitOfWork>();
			var employee = Substitute.For<EmployeeCard>();
			var protectionTools = Substitute.For<ProtectionTools>();
			protectionTools.Id.Returns(55);
			var protectionTools2 = Substitute.For<ProtectionTools>();
			protectionTools2.Id.Returns(77);

			var repository = Substitute.For<EmployeeIssueRepository>(uow);
			repository.ItemsBalance(employee, new DateTime(2022, 11, 11), Arg.Any<int[]>()).Returns(new List<EmployeeRecivedInfo> {
				new EmployeeRecivedInfo {
					Amount = 1,
					LastReceive = new DateTime(2022, 9, 1),
					ProtectionToolsId = 55
				}
			});
			
			var expense = new Expense {
				Date = new DateTime(2022, 11, 11),
				Employee = employee
			};
			var expenseItemCanWriteoff = new ExpenseItem {
				ProtectionTools = protectionTools,
				ExpenseDoc = expense,
			};
			expense.Items.Add(expenseItemCanWriteoff);
			
			var expenseItemCanNotWriteoff = new ExpenseItem {
				ProtectionTools = protectionTools2,
				ExpenseDoc = expense
			};
			expense.Items.Add(expenseItemCanNotWriteoff);

			expense.FillCanWriteoffInfo(repository);
			
			Assert.That(expenseItemCanWriteoff.IsEnableWriteOff, Is.True);
			Assert.That(expenseItemCanNotWriteoff.IsEnableWriteOff, Is.False);
		}
		#endregion
		
		[TearDown]
		public void RemoveStaticGaps()
		{
			IssueGraph.MakeIssueGraphTestGap = null;
		}
	}
}
