using System.Collections;
using NHibernate.Mapping;
using NUnit.Framework;
using QS.Testing.HistoryLog.Testing;

namespace WorkwearTest.HistoryLog
{
	[TestFixture]
	[Category("Доменная модель")]
	public class DomainModelTest : DomainModelTestBase
	{
		static DomainModelTest()
		{
			ConfigureOneTime.ConfigureNh();
		}

		public new static IEnumerable TrackedProperties => DomainModelTestBase.TrackedProperties;
		
		[Test, TestCaseSource(nameof(TrackedProperties))]
		public override void ExistPropertyNameTest(PersistentClass mapping, Property property)
		{
			base.ExistPropertyNameTest(mapping, property);
		}
	}
}