using NUnit.Framework;
using workwear.Models.Import;

namespace WorkwearTest.Models.Import {
	[TestFixture(TestOf = typeof(TextParser))]
	public class TextParserTest {
		[TestCase("Комбинезон хлопчатобумажный для защиты от общих производственных загрязнений механических воздействий", 
			ExpectedResult = "комбинезон хлопчатобумажный для защиты от общих производственных загрязнений механических воздействий", 
			Description = "Убираем заглавные буквы")]
		[TestCase("Пётр", ExpectedResult = "петр", Description = "Заменяем буквы 'ё' на 'е'")]
		[TestCase("Костюм хлопчатобумажный с водоотталкивающей   пропиткой, или костюм для защиты от воды из синтетической ткани с пленочным покрытием", 
			ExpectedResult = "костюм хлопчатобумажный с водоотталкивающей пропиткой, или костюм для защиты от воды из синтетической ткани с пленочным покрытием", 
			Description = "Заменяем двойные и более пробелы одинарными")]
		[TestCase("  Комбинезон хлопчатобумажный   ", 
			ExpectedResult = "комбинезон хлопчатобумажный", 
			Description = "Проверяем что убираем пробелы с перед и после текста")]
		public string PrepareForCompareCases(string input) {
			return TextParser.PrepareForCompare(input);
		}
	}
}
