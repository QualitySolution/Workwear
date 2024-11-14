using NUnit.Framework;
using Workwear.Domain.Company;
using Workwear.Models.Company;

namespace Workwear.Test.Models.Company
{
	[TestFixture(TestOf = typeof(PersonNames))]
	public class PersonNamesTest
	{
		[Test(Description = "Корректное определение пола у имени Адель.")]
		public void DetermineGender() {
			var personNames = new PersonNames();
			Assert.That(personNames.GetSexByName("Адель", "Константиновна"), Is.EqualTo(Sex.F));
			Assert.That(personNames.GetSexByName("Адель", "Викторович"), Is.EqualTo(Sex.M));
		}
	}
}
