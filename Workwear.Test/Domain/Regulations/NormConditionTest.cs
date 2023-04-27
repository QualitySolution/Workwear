using System;
using NUnit.Framework;
using Workwear.Domain.Regulations;

namespace Workwear.Test.Domain.Regulations {
	[TestFixture]
	public class NormConditionTest {
		#region Период внутри года (лето)
		//Внутри периода
		[TestCase("2001-04-01", "2001-10-01", "2023-05-10", "2023-04-01", "2023-10-01")]
		//Снаружи периода до
		[TestCase("2001-04-01", "2001-10-01", "2023-03-10", "2023-04-01", "2023-10-01")]
		//Снаружи периода до
		[TestCase("2001-04-01", "2001-10-01", "2023-03-10", "2023-04-01", "2023-10-01")]
		#endregion
		#region Период между годами (зима)
		//Внутри периода конец года
		[TestCase("2001-11-01", "2001-03-01", "2022-12-10", "2022-11-01", "2023-03-01")]
		//Внутри периода начало года
		[TestCase("2001-11-01", "2001-03-01", "2023-01-15", "2022-11-01", "2023-03-01")]
		//Снаружи периода
		[TestCase("2001-11-01", "2001-03-01", "2023-06-10", "2023-11-01", "2024-03-01")]
		#endregion
		public void CalculateCurrentPeriod_CasesTest(DateTime conditionStart, DateTime conditionEnd, DateTime atDate, DateTime resultStart,
			DateTime resultEnd) {
			var normCondition = new NormCondition {
				IssuanceStart = conditionStart,
				IssuanceEnd = conditionEnd
			};
			var range = normCondition.CalculateCurrentPeriod(atDate);
			Assert.That(range.Begin, Is.EqualTo(resultStart));
			Assert.That(range.End, Is.EqualTo(resultEnd));
		}
	}
}
