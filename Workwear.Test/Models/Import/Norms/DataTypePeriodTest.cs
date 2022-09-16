using System;
using NUnit.Framework;
using Workwear.Domain.Regulations;
using Workwear.Models.Import.Norms.DataTypes;

namespace Workwear.Test.Models.Import.Norms
{
	[TestFixture(TestOf = typeof(DataTypePeriod))]
	public class DataTypePeriodTest
	{
		[Test(Description = "Проверка парсинга колонки c период нормы")]
		[TestCase("36 месяцев", 36, NormPeriodType.Month, false, true)]
		[TestCase("1 месяц", 1, NormPeriodType.Month, false, true)]
		[TestCase("24 месяца", 24, NormPeriodType.Month, false, true)]
		[TestCase("2 года", 2, NormPeriodType.Year, false, true)]
		[TestCase("1,5 года", 18, NormPeriodType.Month, false, true)]//FIXME: исправить после реализации дробного периода
		[TestCase("до износа", 0, NormPeriodType.Wearout, false, true)]
		// Из загрузки Водоканал Краснодар
		[TestCase("дежурные", 0, NormPeriodType.Duty, false, true)]
		[TestCase("дежурный", 0, NormPeriodType.Duty, false, true)]
		[TestCase("дежурная", 0, NormPeriodType.Duty, false, true)]
		[TestCase("Дежурные", 0, NormPeriodType.Duty, false, true)]
		[TestCase("12 мес.", 12, NormPeriodType.Month, false, true)]
		[TestCase("12мес", 12, NormPeriodType.Month, false, true)]
		[TestCase("36 мес", 36, NormPeriodType.Month, false, true)]

		public void TryParsePeriod_Test(string inputString, int expectedCount, NormPeriodType expectedPeriod, bool withWarning, bool expectedResult)
		{
			var result = DataTypePeriod.TryParsePeriod(inputString, out int actualCount, out NormPeriodType actualPeriod, out string warning);
			Assert.AreEqual(expectedResult, result);
			if(expectedResult)
			{
				Assert.AreEqual(expectedPeriod, actualPeriod);
				Assert.AreEqual(expectedCount, actualCount);
				Assert.AreEqual(!String.IsNullOrEmpty(warning), withWarning);
			}
		}
	}
}
