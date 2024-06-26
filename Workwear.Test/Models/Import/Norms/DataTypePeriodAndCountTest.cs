using System;
using NSubstitute;
using NUnit.Framework;
using Workwear.Domain.Regulations;
using Workwear.Models.Import.Norms.DataTypes;
using Workwear.ViewModels.Import;

namespace Workwear.Test.Models.Import.Norms
{
	[TestFixture(TestOf = typeof(DataTypePeriodAndCount))]
	public class DataTypePeriodAndCountTest
	{
		[Test(Description = "Проверка парсинга колонки количество и период нормы")]
		//Варианты выгрузки Восток-Сервис
		[TestCase("Брюки на утепляющей прокладке (1 в 36 месяцев)", 1, 36, NormPeriodType.Month, false, true)]
		[TestCase("Моющее средство (3 в 1 месяц)", 3, 1, NormPeriodType.Month, false, true)]
		[TestCase("Жилет сигнальный (1 в 24 месяца)", 1, 24, NormPeriodType.Month, false, true)]
		[TestCase("Щиток защитный сварщика (До износа)", 1, 0, NormPeriodType.Wearout, false, true)]
		//Варианты выгрузки Агроном-Сад
		[TestCase("1 пара", 1, 1, NormPeriodType.Year, false, true)]
		[TestCase("1 шт на 2 года", 1, 2, NormPeriodType.Year, false, true)]
		[TestCase("1 пара на 1,5 года", 1, 18, NormPeriodType.Month, false, true)]//FIXME: исправить после реализации дробного периода
		[TestCase("до износа", 1, 0, NormPeriodType.Wearout, false, true)]
		[TestCase("дежурные", 1, 0, NormPeriodType.Duty, false, true)]
		[TestCase("дежурный", 1, 0, NormPeriodType.Duty, false, true)]
		[TestCase("1", 1, 1, NormPeriodType.Year, false, true)]
		[TestCase("6 пар", 6, 1, NormPeriodType.Year, false, true)]
		[TestCase("4 пары", 4, 1, NormPeriodType.Year, false, true)]
		[TestCase("1 шт.", 1, 1, NormPeriodType.Year, false, true)]
		[TestCase("1шт", 1, 1, NormPeriodType.Year, false, true)]
		[TestCase("1пара 1,5 года", 1, 18, NormPeriodType.Month, false, true)]//FIXME: исправить после реализации дробного периода
		[TestCase("1шт на 2 года", 1, 2, NormPeriodType.Year, false, true)]
		[TestCase("1шт на 2года", 1, 2, NormPeriodType.Year, false, true)]
		[TestCase("2 комплекта", 2, 1, NormPeriodType.Year, false, true)]
		[TestCase("по поясам на 2,5 года", 0, 30, NormPeriodType.Month, true, true)]
		[TestCase("До износа", 1, 0, NormPeriodType.Wearout, false, true)]
		//Черноземье
		[TestCase("1 комплект.", 1, 1, NormPeriodType.Year, false, true)]
		[TestCase("1 комплект", 1, 1, NormPeriodType.Year, false, true)]
		[TestCase("2 года", 1, 2, NormPeriodType.Year, false, true)]
		[TestCase("1 пара на год.", 1, 1, NormPeriodType.Year, false, true)]
		[TestCase("5 шт на год", 5, 1, NormPeriodType.Year, false, true)]
		[TestCase("3 года", 1, 3, NormPeriodType.Year, false, true)]
		[TestCase("12 пар\nдо износа", 12, 0, NormPeriodType.Wearout, false, true)]
		//Сибур
		[TestCase("2 компл.", 2, 1, NormPeriodType.Year, false, true)]
		[TestCase("2", 2, 1, NormPeriodType.Year, false, true)]
		[TestCase("1 пара на 9 месяцев", 1, 9, NormPeriodType.Month, false, true)]
		[TestCase("1 на 9 месяцев", 1, 9, NormPeriodType.Month, false, true)]
		[TestCase("2 комплекта на 18 мес. ", 2, 18, NormPeriodType.Month, false, true)]
		[TestCase("2 на 18 мес.", 2, 18, NormPeriodType.Month, false, true)]
		[TestCase("2 комп на 2 г.", 2, 2, NormPeriodType.Year, false, true)] 
		[TestCase("2 кмп на 2 г.", 2, 2, NormPeriodType.Year, false, true)]
		[TestCase("дежурная", 1, 0, NormPeriodType.Duty, false, true)]
		[TestCase("деж.", 1, 0, NormPeriodType.Duty, false, true)]
		[TestCase("2 кмп", 2, 1, NormPeriodType.Year, false, true)]
		[TestCase("1пара в год", 1, 1, NormPeriodType.Year, false, true)]
		[TestCase("1на9месяцев", 1, 9, NormPeriodType.Month, false, true)]
		[TestCase("2 комп.", 2, 1, NormPeriodType.Year, false, true)]
		[TestCase("4       пары", 4, 1, NormPeriodType.Year, false, true)]
		[TestCase("2 кмп на 18", 2, 18, NormPeriodType.Month, false, false)] //Здесь по-моему не очевидно что значит 18
		[TestCase("До замены", 1, 0, NormPeriodType.Wearout, false, true)]
		[TestCase("1 пара на месяц", 1, 1, NormPeriodType.Month, false, true)]
		// Нецелые значения количества на год.
		[TestCase("0.5", 1, 2, NormPeriodType.Year, false, true)]
		[TestCase("0,5", 1, 2, NormPeriodType.Year, false, true)]
		[TestCase("0.33", 1, 3, NormPeriodType.Year, false, true)]
		[TestCase("0,33333", 1, 3, NormPeriodType.Year, false, true)]
		public void TryParsePeriodAndCount_Test(string inputString, int expectedAmount, int expectedCount, NormPeriodType expectedPeriod, bool withWarning, bool expectedResult)
		{
			var settings = Substitute.For<IImportNormSettings>();
			var dataType = new DataTypePeriodAndCount(settings);
			var result = dataType.TryParsePeriodAndCount(inputString, out int actualAmount, out int actualCount, out NormPeriodType actualPeriod, out string warning);
			Assert.AreEqual(expectedResult, result);
			if(expectedResult)
			{
				Assert.AreEqual(expectedAmount, actualAmount);
				Assert.AreEqual(expectedPeriod, actualPeriod);
				Assert.AreEqual(expectedCount, actualCount);
				Assert.AreEqual(!String.IsNullOrEmpty(warning), withWarning);
			}
		}
	}
}
