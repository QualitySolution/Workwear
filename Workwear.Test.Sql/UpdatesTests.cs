using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using QS.DBScripts.Controllers;
using QS.DBScripts.Models;
using Workwear.Sql;

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
			var updateConfiguration = ScriptsConfiguration.MakeUpdateConfiguration();
			//Проверяем нужно ли обновлять 
			if(!updateConfiguration.GetHopsToLast(sample.TypedVersion).Any())
				Assert.Ignore($"Образец базы {sample} версии пропущен. Так как версию базы {sample.Version} невозможно обновить.");
			
			//Создаем чистую базу
			var configuration = TestsConfiguration.Configuration;
			var server = configuration.GetSection("SQLServer");
			var creator = new TestingCreateDbController(
				server.GetValue<string>("Address"),
				server.GetValue<string>("Login"),
				server.GetValue<string>("Password")
			);
			var success = creator.StartCreation(sample);
			Assert.That(success, Is.True);
			//Выполняем обновление
			//Может быть какие то проверки корректности
		}
		
		[Test(Description = "Проверяем что можно создать базу из текущего скрипта создания.")]
		public void CreateCurrentNewBaseTest()
		{
			//Создаем чистую базу
			var configuration = TestsConfiguration.Configuration;
			var server = configuration.GetSection("SQLServer");
			var creator = new TestingCreateDbController(
				server.GetValue<string>("Address"),
				server.GetValue<string>("Login"),
				server.GetValue<string>("Password")
			);
			var success = creator.StartCreation(ScriptsConfiguration.MakeCreationScript(), "workwear_sqltest_current");
			Assert.That(success, Is.True);
		}
	}
}