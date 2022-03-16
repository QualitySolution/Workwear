using NUnit.Framework;
using QS.Testing.Updater.Testing;
using QS.Updater.DB;
using Workwear.Sql;

namespace WorkwearTest.Sql
{
	[TestFixture(TestOf = typeof(ScriptsConfiguration))]
	public class ScriptsConfigurationTest : DbConfigurationTestBase
	{
		[Test(Description = "Проверка целостности последовательности обновлений")]
		public void SequenceCheckTest()
		{
			base.SequenceCheckTest(ScriptsConfiguration.MakeUpdateConfiguration());
		}
	}
}