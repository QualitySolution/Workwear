using System;
using NUnit.Framework;
using workwear.Models.Company;

namespace WorkwearTest.Models.Company
{
	[TestFixture(TestOf = typeof(PersonNames))]
	public class PersonNamesTest
	{
		[Test(Description = "В данных отсутствую одинаковые имена, необходимое условие для работы механизма выставление пола в карточке сотрудника. Иначе будут ложные переключения.")]
		public void NamesAreUnique()
		{
			var personNames = new PersonNames();
			foreach(string manName in personNames.MaleNames)
				Assert.That(personNames.FemaleNames.Contains(manName), Is.False, $"Имя {manName} присутствует в обоих списках");

		}
	}
}
