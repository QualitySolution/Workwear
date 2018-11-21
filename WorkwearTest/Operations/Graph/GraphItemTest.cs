using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using workwear.Domain.Operations;
using workwear.Domain.Operations.Graph;

namespace WorkwearTest.Operations.Graph
{
	[TestFixture(TestOf = typeof(GraphItem))]
	public class GraphItemTest
	{
		[Test(Description = "Проверяем что при расчете количества за день, мы корректно можем исключить операцию выдачи.")]
		public void AmountOfDay_ExcludeOperationInIssueTest()
		{
			var startDate = new DateTime(2018, 1, 1);
			var issueOperation = Substitute.For<EmployeeIssueOperation>();
			issueOperation.OperationTime.Returns(startDate);
			issueOperation.Issued.Returns(5);
			var item = new GraphItem(issueOperation);

			Assert.That(item.AmountAtBeginOfDay(startDate.AddDays(1), issueOperation), Is.EqualTo(0));
			Assert.That(item.AmountAtEndOfDay(startDate.AddDays(1), issueOperation), Is.EqualTo(0));
		}

		[Test(Description = "Проверяем что при расчете количества за день, мы корректно можем исключить операцию списания.")]
		public void AmountOfDay_ExcludeOperationInWriteoffTest()
		{
			var startDate = new DateTime(2018, 1, 1);
			var issueOperation = Substitute.For<EmployeeIssueOperation>();
			issueOperation.OperationTime.Returns(startDate);
			issueOperation.Issued.Returns(10);

			var writeoff1 = Substitute.For<EmployeeIssueOperation>();
			writeoff1.Returned.Returns(2);

			var writeoff2 = Substitute.For<EmployeeIssueOperation>();
			writeoff2.Returned.Returns(5);

			var writeoff3 = Substitute.For<EmployeeIssueOperation>();
			writeoff3.Returned.Returns(1);

			var item = new GraphItem(issueOperation);
			item.WriteOffOperations = new List<EmployeeIssueOperation> { writeoff1, writeoff2, writeoff3 };

			//10-2-1 = 8
			Assert.That(item.AmountAtBeginOfDay(startDate.AddDays(1), writeoff2), Is.EqualTo(7), "Количество на начало дня неверно.");
			Assert.That(item.AmountAtEndOfDay(startDate.AddDays(1), writeoff2), Is.EqualTo(7), "Количество на конец дня неверно.");
		}

	}
}
