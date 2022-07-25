using NUnit.Framework;
using Workwear.Domain.Regulations;
using workwear.Models.Import;

namespace WorkwearTest.Models.Import
{
	[TestFixture(TestOf = typeof(DataParserNorm))]
	public class DataParserNormTest
	{
		[Test(Description = "Проверка парсинга колонки количество и период нормы")]
		//Варианты выгрузки Восток-Сервис
		[TestCase("Брюки на утепляющей прокладке (1 в 36 месяцев)", 1, 36, NormPeriodType.Month, true)]
		[TestCase("Моющее средство (3 в 1 месяц)", 3, 1, NormPeriodType.Month, true)]
		[TestCase("Жилет сигнальный (1 в 24 месяца)", 1, 24, NormPeriodType.Month, true)]
		[TestCase("Щиток защитный сварщика (До износа)", 1, 0, NormPeriodType.Wearout, true)]
		//Варианты выгрузки Агроном-Сад
		[TestCase("1 пара", 1, 1, NormPeriodType.Year, true)]
		[TestCase("1 шт на 2 года", 1, 2, NormPeriodType.Year, true)]
		[TestCase("1 пара на 1,5 года", 1, 18, NormPeriodType.Month, true)]//FIXME: исправить после реализации дробного периода
		[TestCase("до износа", 1, 0, NormPeriodType.Wearout, true)]
		[TestCase("дежурные", 1, 0, NormPeriodType.Duty, true)]
		[TestCase("дежурный", 1, 0, NormPeriodType.Duty, true)]
		[TestCase("1", 1, 1, NormPeriodType.Year, true)]
		[TestCase("6 пар", 6, 1, NormPeriodType.Year, true)]
		[TestCase("4 пары", 4, 1, NormPeriodType.Year, true)]
		[TestCase("1 шт.", 1, 1, NormPeriodType.Year, true)]
		[TestCase("1шт", 1, 1, NormPeriodType.Year, true)]
		[TestCase("1пара 1,5 года", 1, 18, NormPeriodType.Month, true)]//FIXME: исправить после реализации дробного периода
		[TestCase("1шт на 2 года", 1, 2, NormPeriodType.Year, true)]
		[TestCase("1шт на 2года", 1, 2, NormPeriodType.Year, true)]
		[TestCase("2 комплекта", 2, 1, NormPeriodType.Year, true)]
		[TestCase("по поясам на 2,5 года", 0, 30, NormPeriodType.Month, false)]//FIXME: когда поймем как реализовать пояса
		[TestCase("До износа", 1, 0, NormPeriodType.Wearout, true)]
		//Черноземье
		[TestCase("1 комплект.", 1, 1, NormPeriodType.Year, true)]
		[TestCase("1 комплект", 1, 1, NormPeriodType.Year, true)]
		[TestCase("2 года", 1, 2, NormPeriodType.Year, true)]
		[TestCase("1 пара на год.", 1, 1, NormPeriodType.Year, true)]
		[TestCase("5 шт на год", 5, 1, NormPeriodType.Year, true)]
		[TestCase("3 года", 1, 3, NormPeriodType.Year, true)]
		public void TryParsePeriodAndCount_Test(string inputString, int expectedAmount, int expectedCount, NormPeriodType expectedPeriod, bool expectedResult)
		{
			var result = DataParserNorm.TryParsePeriodAndCount(inputString, out int actualAmount, out int actualCount, out NormPeriodType actualPeriod);
			Assert.AreEqual(expectedResult, result);
			if(expectedResult)
			{
				Assert.AreEqual(expectedAmount, actualAmount);
				Assert.AreEqual(expectedPeriod, actualPeriod);
				Assert.AreEqual(expectedCount, actualCount);
			}
		}
	}
}
