using System.Collections.Generic;
using NUnit.Framework;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Visits;
using Workwear.Models.Visits;

namespace Workwear.Test.Models.Visits {
	[TestFixture(TestOf = typeof(IssuanceRequestModel))]
	public class IssuanceRequestModelTest {

		[Test(Description = "Все потребности закрыты выданным, строки без потребности не мешают.")]
		public void CalculateStatus_IssuedCase() {
			var request = new IssuanceRequest();
			request.CollectiveExpenses.Add(new CollectiveExpense());
			var needs = new List<IssuanceRequestNeed> {
				new IssuanceRequestNeed { Need = 2, Issued = 2 },
				new IssuanceRequestNeed { Need = 0, Issued = 0 },
			};
			Assert.That(IssuanceRequestModel.CalculateStatus(request, needs), Is.EqualTo(IssuanceRequestStatus.Issued));
		}

		[Test(Description = "Хотя бы одна потребность выдана не полностью.")]
		public void CalculateStatus_PartiallyIssuedCase() {
			var request = new IssuanceRequest();
			request.CollectiveExpenses.Add(new CollectiveExpense());
			var needs = new List<IssuanceRequestNeed> {
				new IssuanceRequestNeed { Need = 2, Issued = 2 },
				new IssuanceRequestNeed { Need = 3, Issued = 0 },
			};
			Assert.That(IssuanceRequestModel.CalculateStatus(request, needs), Is.EqualTo(IssuanceRequestStatus.PartiallyIssued));
		}
	}
}
