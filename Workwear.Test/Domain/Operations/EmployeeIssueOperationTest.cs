﻿using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Extensions.Observable.Collections.List;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Tools;

namespace Workwear.Test.Domain.Operations
{
	[TestFixture(TestOf = typeof(EmployeeIssueOperation))]
	public class EmployeeIssueOperationTest {
		#region RecalculateDatesOfIssueOperation

		[Test(Description =
			"Дата начала использования сдвигается на первую дырку в интервалах, с меньшим чем в норме количеством. Проверяем дату износа через месяц.")]
		public void RecalculateDatesOfIssueOperation_MoveFirstLessNormTest() {
			var employee = Substitute.For<EmployeeCard>();

			var protectionTools = Substitute.For<ProtectionTools>();

			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.ProtectionTools.Returns(protectionTools);
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
			operation2.ProtectionTools.Returns(protectionTools);
			operation2.OperationTime.Returns(new DateTime(2018, 2, 1));

			var operations = new List<EmployeeIssueOperation>() { operation1, operation2 };

			var graph = new IssueGraph(operations);
			var issue = new EmployeeIssueOperation();
			issue.ProtectionTools = protectionTools;
			issue.Employee = employee;
			issue.Nomenclature = nomenclature;
			issue.NormItem = norm;
			issue.OperationTime = new DateTime(2018, 1, 15);
			issue.Issued = 2;

			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);

			issue.RecalculateDatesOfIssueOperation(graph, baseParameters, ask);

			Assert.That(issue.ExpiryByNorm, Is.EqualTo(new DateTime(2018, 3, 1)));
		}

		[Test(Description = "Проверяем пропорциональное увеличение периода использования.")]
		public void RecalculateDatesOfIssueOperation_LifeTimeAppendProportionalTest() {
			var employee = Substitute.For<EmployeeCard>();

			var protectionTools = Substitute.For<ProtectionTools>();

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
			issue.Employee = employee;
			issue.Nomenclature = nomenclature;
			issue.ProtectionTools = protectionTools;
			issue.NormItem = norm;
			issue.OperationTime = new DateTime(2018, 3, 10);
			issue.Issued = 3;

			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);

			issue.RecalculateDatesOfIssueOperation(graph, baseParameters, ask);

			Assert.That(issue.ExpiryByNorm, Is.EqualTo(new DateTime(2018, 4, 25)));
		}

		[Test(Description =
			"Дата начала использования не должна сдвигаться если мы не просим ее сдвинуть.(реальный случай, пользователи не понимаю почему срок носки сдвинут относительно выдачи, если вдруг будут запросы на старое поведение, возможно нужно сделать настроку для этого, но по умолчанию оставить проверяемое этим тестом.)")]
		[Category("real case")]
		public void RecalculateDatesOfIssueOperation_DontMoveStartOfUseTest() {
			var employee = Substitute.For<EmployeeCard>();

			var protectionTools = Substitute.For<ProtectionTools>();

			var norm = new NormItem() {
				Amount = 1,
				NormPeriod = NormPeriodType.Month,
				PeriodCount = 24,
				ProtectionTools = protectionTools
			};

			var nomenclature = Substitute.For<Nomenclature>();
			nomenclature.TypeName.Returns("fake");

			var operation1 = Substitute.For<EmployeeIssueOperation>();
			operation1.ProtectionTools.Returns(protectionTools);
			operation1.OperationTime.Returns(new DateTime(2019, 4, 30));
			operation1.StartOfUse.Returns(new DateTime(2019, 4, 30));
			operation1.AutoWriteoffDate.Returns(new DateTime(2021, 4, 30));
			operation1.ExpiryByNorm.Returns(new DateTime(2021, 4, 30));
			operation1.Issued.Returns(1);

			var issue = new EmployeeIssueOperation {
				ProtectionTools = protectionTools,
				Employee = employee,
				Nomenclature = nomenclature,
				NormItem = norm,
				OperationTime = new DateTime(2021, 4, 20),
				StartOfUse = new DateTime(2021, 4, 20),
				AutoWriteoffDate = new DateTime(2023, 4, 20),
				ExpiryByNorm = new DateTime(2023, 4, 20),
				Issued = 1
			};

			var operations = new List<EmployeeIssueOperation>() { operation1, issue };
			var graph = new IssueGraph(operations);

			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(Arg.Any<string>()).ReturnsForAnyArgs(false);

			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(15);

			issue.RecalculateDatesOfIssueOperation(graph, baseParameters, ask);

			Assert.That(issue.StartOfUse, Is.EqualTo(new DateTime(2021, 4, 20)));
			Assert.That(issue.ExpiryByNorm, Is.EqualTo(new DateTime(2023, 4, 20)));
		}

		#region Отпуск

		[Test(Description = "Проверяем увеличение периода использования на время отпуска.")]
		public void RecalculateDatesOfIssueOperation_LifeTimeAppendOnVacationTest() {
			var vacationType = Substitute.For<VacationType>();
			vacationType.ExcludeFromWearing.Returns(true);

			var employee = Substitute.For<EmployeeCard>();
			var vacation = Substitute.For<EmployeeVacation>();
			vacation.VacationType.Returns(vacationType);
			vacation.Employee.Returns(employee);
			vacation.BeginDate.Returns(new DateTime(2019, 2, 1));
			vacation.EndDate.Returns(new DateTime(2019, 2, 10));
			var vacations = new ObservableList<EmployeeVacation> { vacation };
			employee.Vacations.Returns(vacations);

			var protectionTools = Substitute.For<ProtectionTools>();

			var norm = new NormItem();
			norm.Amount = 2;
			norm.PeriodCount = 3;
			norm.NormPeriod = NormPeriodType.Month;

			var nomenclature = Substitute.For<Nomenclature>();
			nomenclature.TypeName.Returns("fake");

			var operations = new List<EmployeeIssueOperation>() { };

			var graph = new IssueGraph(operations);
			var issue = new EmployeeIssueOperation();
			issue.Employee = employee;
			issue.Nomenclature = nomenclature;
			issue.ProtectionTools = protectionTools;
			issue.NormItem = norm;
			issue.OperationTime = new DateTime(2019, 1, 10);
			issue.Issued = 2;

			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);

			issue.RecalculateDatesOfIssueOperation(graph, baseParameters, ask);

			Assert.That(issue.ExpiryByNorm, Is.EqualTo(new DateTime(2019, 4, 20)));
		}

		[Test(Description =
			"Проверяем что правильно исключаем отпуск в случае наличие времени в операции выдачи. Реальный случай некорректного расчета.")]
		[Category("real case")]
		public void RecalculateDatesOfIssueOperation_LifeTimeAppendOnVacation_IgnoreTimeTest() {
			var vacationType = Substitute.For<VacationType>();
			vacationType.ExcludeFromWearing.Returns(true);

			var employee = Substitute.For<EmployeeCard>();
			var vacation = Substitute.For<EmployeeVacation>();
			vacation.VacationType.Returns(vacationType);
			vacation.Employee.Returns(employee);
			vacation.BeginDate.Returns(new DateTime(2019, 7, 19));
			vacation.EndDate.Returns(new DateTime(2019, 7, 23));

			var vacation2 = Substitute.For<EmployeeVacation>();
			vacation2.VacationType.Returns(vacationType);
			vacation2.Employee.Returns(employee);
			vacation2.BeginDate.Returns(new DateTime(2018, 7, 3));
			vacation2.EndDate.Returns(new DateTime(2018, 7, 5));
			var vacations = new ObservableList<EmployeeVacation> { vacation, vacation2 };
			employee.Vacations.Returns(vacations);

			var protectionTools = Substitute.For<ProtectionTools>();

			var norm = new NormItem();
			norm.Amount = 1;
			norm.PeriodCount = 1;
			norm.NormPeriod = NormPeriodType.Year;

			var nomenclature = Substitute.For<Nomenclature>();
			nomenclature.TypeName.Returns("fake");

			var operations = new List<EmployeeIssueOperation>() { };

			var graph = new IssueGraph(operations);
			var issue = new EmployeeIssueOperation();
			issue.Employee = employee;
			issue.Nomenclature = nomenclature;
			issue.ProtectionTools = protectionTools;
			issue.NormItem = norm;
			issue.OperationTime = new DateTime(2018, 11, 22, 15, 09, 23);
			issue.Issued = 1;

			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(true);

			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);

			issue.RecalculateDatesOfIssueOperation(graph, baseParameters, ask);

			Assert.That(issue.ExpiryByNorm, Is.EqualTo(new DateTime(2019, 11, 27)));
		}

		#endregion

		#endregion

		#region CalculatePercentWear

		[Test(Description = "Проверка расчётов процента тесткейсами.")]
		[TestCase("2018-2-5", "2018-1-31", "2018-2-10", 0, ExpectedResult = 0.50)]
		[TestCase("2018-1-2", "2018-1-1", "2018-1-7", 0.40, ExpectedResult = 0.50)]
		[TestCase("2018-2-5", "2018-1-31", "2018-2-27", 2.5, ExpectedResult = 2.5)]
		[TestCase("2018-1-30", "2018-1-1", "2018-1-20", 0.5, ExpectedResult = 1.26)]
		public decimal CalculatePercentWear_Writeoff_WearPercentTest (DateTime calcDate, DateTime? startOfUse, DateTime? expiryByNorm, decimal beginWearPercent)
		{
			var issue = new EmployeeIssueOperation();
			issue.WearPercent = beginWearPercent;
			issue.StartOfUse = startOfUse;
			issue.ExpiryByNorm = expiryByNorm;

			var atDate = calcDate;
			return issue.CalculatePercentWear(atDate);
		}

		[Test(Description = "Не падаем в OverflowException при конвертировании в Decimal(реальный кейс при некоторых значениях)")]
		public void CalculatePercentWear_NotOverflowExceptionForDecimalConvert()
		{
			var result = EmployeeIssueOperation.CalculatePercentWear(new DateTime(2019, 1,1), new DateTime(2019, 1, 1), new DateTime(2019, 1, 1), 0);
		}

		[Test(Description = "Не падаем при конвертировании в Decimal, полученной бесконечности(реальный кейс при некоторых значениях)")]
		public void CalculatePercentWear_InfinityWhenDecimalConvert()
		{
			var result = EmployeeIssueOperation.CalculatePercentWear(new DateTime(2019, 8, 2), new DateTime(2019, 7, 17), new DateTime(2019, 7, 17), 0);
			Assert.That(result, Is.EqualTo(0));
		}

		#endregion
	}
}
