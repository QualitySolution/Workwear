using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using workwear.Domain.Operations;
using workwear.Domain.Operations.Graph;
using workwear.Domain.Organization;
using workwear.Domain.Regulations;

namespace WorkwearTest.Organization
{
	[TestFixture(TestOf = typeof(EmployeeCardItem))]
	public class EmployeeCardItemTests
	{
		[Test]
		public void UpdateNextIssue_AutoWriteoffCase()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2018, 1, 1));
			operation1.AutoWriteoffDate.Returns(new DateTime(2018, 2, 1));
			operation1.Issued.Returns(10);

			var list = new List<EmployeeIssueOperation>() { operation1 };
			var graph = new IssueGraph(list);

			var uow = Substitute.For<IUnitOfWork>();
			var employee = Substitute.For<EmployeeCard>();
			employee.Id.Returns(777); //Необходимо чтобы было более 0, для запроса имеющихся операций.

			var norm = Substitute.For<NormItem>();
			norm.Amount.Returns(10);

			var item = new EmployeeCardItemTested();
			item.EmployeeCard = employee;
			item.GetIssueGraphForItemFunc = () => graph;
			item.ActiveNormItem = norm;

			item.UpdateNextIssue(uow);
			Assert.That(item.NextIssue, Is.EqualTo(new DateTime(2018, 2, 1)));
		}

		[Test]
		public void UpdateNextIssue_NotWriteoffCase()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2018, 1, 1));
			operation1.ExpenseByNorm.Returns(new DateTime(2018, 3, 1));
			operation1.Issued.Returns(10);

			var list = new List<EmployeeIssueOperation>() { operation1 };
			var graph = new IssueGraph(list);

			var uow = Substitute.For<IUnitOfWork>();
			var employee = Substitute.For<EmployeeCard>();
			employee.Id.Returns(777); //Необходимо чтобы было более 0, для запроса имеющихся операций.

			var norm = Substitute.For<NormItem>();
			norm.Amount.Returns(10);

			var item = new EmployeeCardItemTested();
			item.EmployeeCard = employee;
			item.GetIssueGraphForItemFunc = () => graph;
			item.ActiveNormItem = norm;

			item.UpdateNextIssue(uow);
			Assert.That(item.NextIssue, Is.EqualTo(new DateTime(2018, 3, 1)));
		}

		[Test]
		public void UpdateNextIssue_FirstNotEnoughCase()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2018, 1, 1));
			operation1.AutoWriteoffDate.Returns(new DateTime(2018, 3, 1));
			operation1.Issued.Returns(10);

			var operation2 = Substitute.For<EmployeeIssueOperation>();
			operation2.OperationTime.Returns(new DateTime(2018, 1, 15));
			operation2.Returned.Returns(2);

			var list = new List<EmployeeIssueOperation>() { operation1, operation2 };
			var graph = new IssueGraph(list);

			var uow = Substitute.For<IUnitOfWork>();
			var employee = Substitute.For<EmployeeCard>();
			employee.Id.Returns(777); //Необходимо чтобы было более 0, для запроса имеющихся операций.

			var norm = Substitute.For<NormItem>();
			norm.Amount.Returns(10);

			var item = new EmployeeCardItemTested();
			item.EmployeeCard = employee;
			item.GetIssueGraphForItemFunc = () => graph;
			item.ActiveNormItem = norm;

			item.UpdateNextIssue(uow);
			Assert.That(item.NextIssue, Is.EqualTo(new DateTime(2018, 1, 15)));
		}
	}

	public class EmployeeCardItemTested : EmployeeCardItem
	{
		public Func<IssueGraph> GetIssueGraphForItemFunc;

		protected internal override IssueGraph GetIssueGraphForItem(IUnitOfWork uow)
		{
			return GetIssueGraphForItemFunc();
		}
	}
}
