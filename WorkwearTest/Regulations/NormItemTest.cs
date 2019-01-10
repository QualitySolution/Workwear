using NUnit.Framework;
using System;
using workwear.Domain.Regulations;

namespace WorkwearTest.Regulations
{
	[TestFixture(TestOf = typeof(NormItem))]
	public class NormItemTest
	{
		[Test]
		[TestCase(2, NormPeriodType.Year, 2, "2018-01-01", ExpectedResult = "2020-01-01")]
		[TestCase(1, NormPeriodType.Month, 3, "2018-01-10", ExpectedResult = "2018-04-10")]
		public DateTime CalculateExpireDateTest(int normAmount, NormPeriodType normPeriod, int periodCount, DateTime startdate)
		{
			var item = new NormItem();
			item.Amount = normAmount;
			item.NormPeriod = normPeriod;
			item.PeriodCount = periodCount;

			return item.CalculateExpireDate(startdate);
		}

		[Test]
		[TestCase(2, NormPeriodType.Year, 2, "2018-01-01", 3, ExpectedResult = "2021-01-01")]
		[TestCase(1, NormPeriodType.Month, 3, "2018-01-10", 2, ExpectedResult = "2018-07-10")]
		[TestCase(1, NormPeriodType.Year, 2, "2018-10-19", 1, ExpectedResult = "2020-10-19")]
		public DateTime CalculateExpireDateProportionalTest(int normAmount, NormPeriodType normPeriod, int periodCount, DateTime startdate, int amount)
		{
			var item = new NormItem();
			item.Amount = normAmount;
			item.NormPeriod = normPeriod;
			item.PeriodCount = periodCount;

			return item.CalculateExpireDate(startdate, amount);
		}
	}
}
