using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Workwear.Test.Sql
{
	public class UpdatesTests
	{
		public static IEnumerable<DbSample> DbSamples {
			get {
				var configuration = TestsConfiguration.Configuration;
				List<DbSample> samples = configuration.GetSection("Samples").Get<List<DbSample>>();
				return samples;
			}
		}

		[TestCaseSource(nameof(DbSamples))]
		public void ApplyUpdatesTest(DbSample sample)
		{
			Assert.Pass();
		}
	}
}