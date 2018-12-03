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
		[Test(Description = "Проверяем что при расчете количества за день, мы корректно можем исключить операцию выдачи. Случай с одинаковыми объектами но 0 id.")]
		public void AmountOfDay_ExcludeOperationInIssue_SameObjectZeroIdTest()
		{
			var startDate = new DateTime(2018, 1, 1);
			var issueOperation = Substitute.For<EmployeeIssueOperation>();
			issueOperation.OperationTime.Returns(startDate);
			issueOperation.Issued.Returns(5);
			var item = new GraphItem(issueOperation);

			Assert.That(item.AmountAtBeginOfDay(startDate.AddDays(1), issueOperation), Is.EqualTo(0));
			Assert.That(item.AmountAtEndOfDay(startDate.AddDays(1), issueOperation), Is.EqualTo(0));
		}

		[Test(Description = "Проверяем что при расчете количества за день, мы корректно можем исключить операцию выдачи. Случай с одинаковыми id но разными объектами")]
		public void AmountOfDay_ExcludeOperationInIssue_DiffObjectEqualIdTest()
		{
			var startDate = new DateTime(2018, 1, 1);
			var issueOperation = Substitute.For<EmployeeIssueOperation>();
			issueOperation.Id.Returns(144);
			issueOperation.OperationTime.Returns(startDate);
			issueOperation.Issued.Returns(5);
			var item = new GraphItem(issueOperation);

			var issueOperation2 = Substitute.For<EmployeeIssueOperation>();
			issueOperation2.Id.Returns(144);
			issueOperation2.OperationTime.Returns(startDate);
			issueOperation2.Issued.Returns(5);


			Assert.That(item.AmountAtBeginOfDay(startDate.AddDays(1), issueOperation2), Is.EqualTo(0));
			Assert.That(item.AmountAtEndOfDay(startDate.AddDays(1), issueOperation2), Is.EqualTo(0));
		}

		[Test(Description = "Проверяем что не исключаем операции с нулевыми id, так как они одинаковые.")]
		public void AmountOfDay_ExcludeOperationInIssue_ZeroIdIsNotEqualTest()
		{
			var startDate = new DateTime(2018, 1, 1);
			var issueOperation = Substitute.For<EmployeeIssueOperation>();
			issueOperation.Id.Returns(0);
			issueOperation.OperationTime.Returns(startDate);
			issueOperation.Issued.Returns(5);
			var item = new GraphItem(issueOperation);

			var issueOperation2 = Substitute.For<EmployeeIssueOperation>();
			issueOperation2.Id.Returns(0);
			issueOperation2.OperationTime.Returns(startDate);
			issueOperation2.Issued.Returns(5);

			Assert.That(item.AmountAtBeginOfDay(startDate.AddDays(1), issueOperation2), Is.EqualTo(5));
			Assert.That(item.AmountAtEndOfDay(startDate.AddDays(1), issueOperation2), Is.EqualTo(5));
		}

		[Test(Description = "Проверяем что при расчете количества за день, мы корректно можем исключить операцию списания. Случай с одинаковыми объектами но 0 id.")]
		public void AmountOfDay_ExcludeOperationInWriteoff_SameObjectZeroIdTest()
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

			//10-2-1 = 7
			Assert.That(item.AmountAtBeginOfDay(startDate.AddDays(1), writeoff2), Is.EqualTo(7), "Количество на начало дня неверно.");
			Assert.That(item.AmountAtEndOfDay(startDate.AddDays(1), writeoff2), Is.EqualTo(7), "Количество на конец дня неверно.");
		}

		[Test(Description = "Проверяем что при расчете количества за день, мы корректно можем исключить операцию списания. Случай с одинаковыми id но разными объектами")]
		public void AmountOfDay_ExcludeOperationInWriteoff_DiffObjectEqualIdTest()
		{
			var startDate = new DateTime(2018, 1, 1);
			var issueOperation = Substitute.For<EmployeeIssueOperation>();
			issueOperation.OperationTime.Returns(startDate);
			issueOperation.Issued.Returns(10);

			var writeoff1 = Substitute.For<EmployeeIssueOperation>();
			writeoff1.Returned.Returns(2);

			var writeoff2 = Substitute.For<EmployeeIssueOperation>();
			writeoff2.Id.Returns(144);
			writeoff2.Returned.Returns(5);

			var writeoff3 = Substitute.For<EmployeeIssueOperation>();
			writeoff3.Returned.Returns(1);

			var item = new GraphItem(issueOperation);
			item.WriteOffOperations = new List<EmployeeIssueOperation> { writeoff1, writeoff2, writeoff3 };

			var writeoff2_copy = Substitute.For<EmployeeIssueOperation>();
			writeoff2_copy.Id.Returns(144);
			writeoff2_copy.Returned.Returns(5);

			//10-2-1 = 7
			Assert.That(item.AmountAtBeginOfDay(startDate.AddDays(1), writeoff2_copy), Is.EqualTo(7), "Количество на начало дня неверно.");
			Assert.That(item.AmountAtEndOfDay(startDate.AddDays(1), writeoff2_copy), Is.EqualTo(7), "Количество на конец дня неверно.");
		}

		[Test(Description = "Проверяем что не исключаем операции с нулевыми id, так как они одинаковые.")]
		public void AmountOfDay_ExcludeOperationInWriteoff_ZeroIdIsNotEqualTest()
		{
			var startDate = new DateTime(2018, 1, 1);
			var issueOperation = Substitute.For<EmployeeIssueOperation>();
			issueOperation.OperationTime.Returns(startDate);
			issueOperation.Issued.Returns(10);

			var writeoff1 = Substitute.For<EmployeeIssueOperation>();
			writeoff1.Id.Returns(0);
			writeoff1.Returned.Returns(2);

			var item = new GraphItem(issueOperation);
			item.WriteOffOperations = new List<EmployeeIssueOperation> { writeoff1 };

			var writeoff1_copy = Substitute.For<EmployeeIssueOperation>();
			writeoff1_copy.Id.Returns(0);
			writeoff1_copy.Returned.Returns(2);

			//10-2 = 8
			Assert.That(item.AmountAtBeginOfDay(startDate.AddDays(1), writeoff1_copy), Is.EqualTo(8), "Количество на начало дня неверно.");
			Assert.That(item.AmountAtEndOfDay(startDate.AddDays(1), writeoff1_copy), Is.EqualTo(8), "Количество на конец дня неверно.");
		}
	}
}
