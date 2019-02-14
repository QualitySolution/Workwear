using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
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
		#region RecalculateDatesOfIssueOperation

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

			Assert.That(issue.ExpiryByNorm, Is.EqualTo(new DateTime(2018, 3, 1)));
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

			Assert.That(issue.ExpiryByNorm, Is.EqualTo(new DateTime(2018, 4, 25)));
		}

		#endregion

		#region CalculatePercentWear

		[Test(Description = "5 дней из 10-и это 50 процентов.")]
		public void CalculatePercentWear_Writeoff_WearPercentTest_50Percent()
		{
			var issue = new EmployeeIssueOperation();
			issue.StartOfUse = new DateTime(2018, 1, 31);
			issue.ExpiryByNorm = new DateTime(2018, 2, 10);

			var atDate = new DateTime(2018, 2, 5);
			var result = issue.CalculatePercentWear(atDate);
			Assert.That(result, Is.EqualTo(0.5m));
		}

		[Test(Description = "начальные 45% + 10%, проверяем что к начальным добавляется расчетный.")]
		public void CalculatePercentWear_Writeoff_WearPercentTest_StartPercentPlus10()
		{
			var issue = new EmployeeIssueOperation();
			issue.WearPercent = 0.45m;
			issue.StartOfUse = new DateTime(2018, 1, 1);
			issue.ExpiryByNorm = new DateTime(2018, 1, 11);

			var atDate = new DateTime(2018, 1, 2);
			var result = issue.CalculatePercentWear(atDate);
			Assert.That(result, Is.EqualTo(0.55m));
		}

		[Test(Description = "Не падаем в OverflowException при конвертировании в Decemal(реальный кейс при некоторых значениях)")]
		public void CalculatePercentWear_NotOverflowExceptionForDecimalConvert()
		{
			var result = EmployeeIssueOperation.CalculatePercentWear(new DateTime(2019, 1,1), new DateTime(2019, 1, 1), new DateTime(2019, 1, 1), 0);
		}

		#endregion
	}
}
