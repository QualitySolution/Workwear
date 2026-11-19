using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using Workwear.Domain.Company;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Tools;

namespace Workwear.Test.Domain.Regulations {
	[TestFixture]
	public class DutyNormItemTest {
		[Test(Description = "Потребность по дежурной норме учитывает даты действия")]
		public void CalculateRequiredIssue_RespectsInclusiveNormDates() {
			var norm = new DutyNorm {
				DateFrom = new DateTime(2026, 11, 10),
				DateTo = new DateTime(2026, 11, 20)
			};
			var item = new DutyNormItem {
				DutyNorm = norm,
				ProtectionTools = new ProtectionTools { Type = new ItemsType() },
				Amount = 2,
				Graph = new IssueGraph(new List<IGraphIssueOperation>())
			};
			var parameters = Substitute.For<BaseParameters>();

			Assert.That(item.CalculateRequiredIssue(parameters, new DateTime(2026, 11, 9)), Is.Zero);
			Assert.That(item.CalculateRequiredIssue(parameters, new DateTime(2026, 11, 10)), Is.EqualTo(2));
			Assert.That(item.CalculateRequiredIssue(parameters, new DateTime(2026, 11, 20)), Is.EqualTo(2));
			Assert.That(item.CalculateRequiredIssue(parameters, new DateTime(2026, 11, 21)), Is.Zero);
		}

		[Test(Description = "Дата следующей выдачи сдвигается на начало действия нормы и не назначается после её окончания")]
		public void CalculateNextIssue_RespectsNormDates() {
			var norm = new DutyNorm {
				DateFrom = new DateTime(2026, 11, 10),
				DateTo = new DateTime(2026, 11, 20)
			};
			var item = new DutyNormItem {
				DutyNorm = norm,
				Amount = 1,
				Graph = new IssueGraph(new List<IGraphIssueOperation>())
			};

			Assert.That(item.CalculateNextIssue(), Is.EqualTo(norm.DateFrom));
			norm.DateTo = norm.DateFrom.Value.AddDays(-1);
			Assert.That(item.CalculateNextIssue(), Is.Null);
		}
	}
}
