using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using workwear.Domain.Operations;
using workwear.Domain.Operations.Graph;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;

namespace WorkwearTest.Operations
{
	[TestFixture(TestOf = typeof(EmployeeIssueOperation))]
	public class EmployeeIssueOperationTest
	{
		[Test(Description = "Дата начала использование сдвигается на первую дырку в интервалах, с меньшим чем в норме количетвом. Проверяем дату износа через месяц.")]
		public void RecalculateDatesOfIssueOperation_MoveFirstLessNormTest()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2018, 1, 1));
			operation1.AutoWriteoffDate.Returns(new DateTime(2018, 2, 1));
			operation1.Issued.Returns(2);

			var norm = Substitute.For<NormItem>();
			norm.Amount.Returns(2);
			norm.PeriodInMonths.Returns(1);
			norm.CalculateExpireDate(new DateTime(2018, 2, 1)).Returns(new DateTime(2018, 3, 1));

			var nomenclature = Substitute.For<Nomenclature>();
			nomenclature.TypeName.Returns("fake");

			var operation2 = Substitute.For<EmployeeIssueOperation>();
			operation2.OperationTime.Returns(new DateTime(2018, 2, 1));

			var operations = new List<EmployeeIssueOperation>() {operation1, operation2};

			var graph = new IssueGraph(operations);
			var issue = new EmployeeIssueOperation();
			issue.Nomenclature = nomenclature;
			issue.NormItem = norm;
			issue.OperationTime = new DateTime(2018, 1, 15);
			issue.Issued = 2;

			issue.RecalculateDatesOfIssueOperation(graph, s => true);

			Assert.That(issue.ExpenseByNorm, Is.EqualTo(new DateTime(2018, 3, 1)));
		}

		[Test(Description = "Проверяем пропорциональное увеличение периода использовния.")]
		public void RecalculateDatesOfIssueOperation_LifeTimeAppendProportionalTest()
		{
			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.OperationTime.Returns(new DateTime(2018, 1, 1));
			operation1.AutoWriteoffDate.Returns(new DateTime(2018, 2, 1));
			operation1.Issued.Returns(2);

			var norm = new NormItem();
			norm.Amount = 2;
			norm.PeriodCount = 1;
			norm.NormPeriod = NormPeriodType.Month;

			var nomenclature = Substitute.For<Nomenclature>();
			nomenclature.TypeName.Returns("fake");

			var operations = new List<EmployeeIssueOperation>() { operation1 };

			var graph = new IssueGraph(operations);
			var issue = new EmployeeIssueOperation();
			issue.Nomenclature = nomenclature;
			issue.NormItem = norm;
			issue.OperationTime = new DateTime(2018, 3, 10);
			issue.Issued = 3;

			issue.RecalculateDatesOfIssueOperation(graph, s => true);

			Assert.That(issue.ExpenseByNorm, Is.EqualTo(new DateTime(2018, 4, 25)));
		}
	}
}
