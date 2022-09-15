using NUnit.Framework;
using Workwear.Models.Company;
using workwear.Models.Import.Employees.DataTypes;

namespace WorkwearTest.Models.Import.Employees {
	[TestFixture(TestOf = typeof(DataTypeFio))]
	public class DataTypeFioTest {

		[Test(Description = "Тестируем варианты подбора колонок по имени. Примеры в нижнем регистре специально!")]
		[TestCase("фамилия, имя, отчество", ExpectedResult = true)]
		[TestCase("фамилия имя отчество", ExpectedResult = true)]
		[TestCase("фио", ExpectedResult = true)]
		[TestCase("ф.и.о.", ExpectedResult = true)]
		public bool ColumnNameMatch_Cases(string columnName) {
			var dataType = new DataTypeFio(new PersonNames());
			return dataType.ColumnNameMatch(columnName);
		}
	}
}
