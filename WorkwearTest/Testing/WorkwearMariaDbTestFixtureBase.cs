using System.Threading.Tasks;
using NUnit.Framework;
using QS.Extensions.Observable.Collections.List;
using QS.Measurement.Domain;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Testing.DB;
using Workwear.HibernateMapping;

namespace WorkwearTest.Testing
{
	[NonParallelizable]
	public abstract class WorkwearMariaDbTestFixtureBase : MariaDbTestContainerFixtureBase
	{
		[OneTimeSetUp]
		public async Task InitMariaDb()
		{
			MappingParams.UseIdsForTest = true;
			await InitialiseMariaDb(
				System.Reflection.Assembly.GetAssembly(typeof(Workwear.Domain.Users.UserSettings)),
				System.Reflection.Assembly.GetAssembly(typeof(MeasurementUnit)),
				System.Reflection.Assembly.GetAssembly(typeof(UserBase))
			);
		}

		[SetUp]
		public async Task ResetMariaDbSchema()
		{
			await RecreateSchema();
		}

		[OneTimeTearDown]
		public async Task DisposeMariaDbContainer()
		{
			await DisposeMariaDb();
			ConfigureOneTime.ResetNhConfiguration();
		}

		protected override void ConfigureOrmBeforeConfigure()
		{
			OrmConfig.Conventions = new[] { new ObservableListConvention() };
		}
	}
}
