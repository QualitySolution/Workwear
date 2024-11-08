using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using QS.Extensions.Observable.Collections.List;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Models.Analytics;
using Workwear.Tools;

namespace Workwear.Test.Models.Analytics {
	[TestFixture(TestOf = typeof(FutureIssueModel))]
	public class FutureIssueModelTest {
		[Category("Real case")]
		[Test(Description = "Убеждаемся что конкретный случай расчета полностью просроченной частичной выдачи считается правильно.")]
		public void CalculateIssues_AllExpiredPartialIssue_Calculated()
		{
			// arrange
			var baseParameters = Substitute.For<BaseParameters>();
			var model = new FutureIssueModel(baseParameters);
			var protectionTools = new ProtectionTools {
				Nomenclatures = new ObservableList<Nomenclature>() {
					new Nomenclature()
				}
			};

			var norm = new Norm();
			
			var normItem = new NormItem {
				Norm = norm,
				ProtectionTools = protectionTools,
				Amount = 2,
				NormPeriod = NormPeriodType.Year,
				PeriodCount = 4
			};
			
			var operations = new List<EmployeeIssueOperation>() {
				new EmployeeIssueOperation {
					OperationTime = new DateTime(2022, 10, 5),
					Issued = 1,
					AutoWriteoffDate = new DateTime(2024, 10, 5),
					ExpiryByNorm = new DateTime(2024, 10, 5)
				}
			};
			
			var employeeItems = new List<EmployeeCardItem> {
				new EmployeeCardItem {
					EmployeeCard = new EmployeeCard(),
					ActiveNormItem = normItem,
					ProtectionTools = protectionTools,
					NextIssue = new DateTime(2022, 10, 5),
					Graph = new IssueGraph(operations)
				}
			};

			// act
			var result = model.CalculateIssues(new DateTime(2024, 11, 5), new DateTime(2024, 12, 31), true, employeeItems);

			// assert
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result[0].OperationDate, Is.EqualTo(new DateTime(2024, 11, 5)));
			Assert.That(result[0].DelayIssueDate, Is.EqualTo(new DateTime(2022, 10, 5)));
			Assert.That(result[0].Amount, Is.EqualTo(2));
		}
		
		[Test(Description = "Проверяем стандартное прогнозирование, выдачу перчаток каждый 3 месяца")]
		public void CalculateIssues_StandardForecast_Calculated()
		{
			// arrange
			var baseParameters = Substitute.For<BaseParameters>();
			var model = new FutureIssueModel(baseParameters);
			var protectionTools = new ProtectionTools {
				Nomenclatures = new ObservableList<Nomenclature>() {
					new Nomenclature()
				}
			};

			var norm = new Norm();
			
			var normItem = new NormItem {
				Norm = norm,
				ProtectionTools = protectionTools,
				Amount = 12,
				NormPeriod = NormPeriodType.Month,
				PeriodCount = 3
			};
			
			var operations = new List<EmployeeIssueOperation>();
			
			var employeeItems = new List<EmployeeCardItem> {
				new EmployeeCardItem {
					EmployeeCard = new EmployeeCard(),
					ActiveNormItem = normItem,
					ProtectionTools = protectionTools,
					NextIssue = new DateTime(2024, 1, 5),
					Graph = new IssueGraph(operations)
				}
			};

			// act
			var result = model.CalculateIssues(new DateTime(2024, 1, 1), new DateTime(2024, 12, 31), false, employeeItems);

			// assert
			Assert.That(result.Count, Is.EqualTo(4));
			
			Assert.That(result[0].OperationDate, Is.EqualTo(new DateTime(2024, 1, 5)));
			Assert.That(result[0].Amount, Is.EqualTo(12));
			
			Assert.That(result[1].OperationDate, Is.EqualTo(new DateTime(2024, 4, 5)));
			Assert.That(result[1].Amount, Is.EqualTo(12));
			
			Assert.That(result[2].OperationDate, Is.EqualTo(new DateTime(2024, 7, 5)));
			Assert.That(result[2].Amount, Is.EqualTo(12));
			
			Assert.That(result[3].OperationDate, Is.EqualTo(new DateTime(2024, 10, 5)));
			Assert.That(result[3].Amount, Is.EqualTo(12));
		}
	}
}
