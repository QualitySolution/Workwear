using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using NLog;
using NUnit.Framework;
using QS.DBScripts.Controllers;
using QS.DBScripts.Models;
using QS.Updater.DB;
using QS.Utilities.Text;
using Workwear.Sql;

namespace Workwear.Test.Sql
{
	[TestFixture]
	[NonParallelizable]
	public class UpdatesTests
	{
		private static readonly string currentDdName = "workwear_sqltest_current";

		private SqlServer RunningServer { get; set; }
		
		public static IEnumerable<object[]> DbSamples {
			get {
				var configuration = TestsConfiguration.Configuration;
				List<DbSample> samples = configuration.GetSection("Samples").Get<List<DbSample>>();
				
				foreach (var server in SqlServers) {
					foreach (var dbSample in samples) {
						if(!String.IsNullOrEmpty(dbSample.ForServerGroup) && !dbSample.ForServerGroup.Equals(server.Group))
							continue;

						yield return new object[] { server, dbSample };
					}
				}
			}
		}

		[OneTimeSetUp]
		public void Setup() {
			LogManager.Setup().LoadConfiguration(builder => {
				builder.ForLogger().FilterMinLevel(LogLevel.Info).WriteToConsole();
			});
		}
		
		[TestCaseSource(nameof(DbSamples))]
		public void ApplyUpdatesTest(SqlServer server, DbSample sample) {
			var updateConfiguration = ScriptsConfiguration.MakeUpdateConfiguration();
			//Проверяем нужно ли обновлять 
			if(!updateConfiguration.GetHopsToLast(sample.TypedVersion, false).Any())
				Assert.Ignore($"Образец базы {sample} версии пропущен. Так как версию базы {sample.Version} невозможно обновить.");

			//Загружаем образец базы на SQL сервер.
			StartSqlServer(server);
			TestContext.Progress.WriteLine($"Создаем базу {sample.DbName}");
			var creator = new TestingCreateDbController(server);
			var success = creator.StartCreation(sample);
			Assert.That(success, Is.True);
			//Выполняем обновление
			var builder = server.ConnectionStringBuilder;
			builder.Database = sample.DbName;
			var connectionString = builder.GetConnectionString(true);
			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				foreach(var hop in updateConfiguration.GetHopsToLast(sample.TypedVersion, false)) {
					TestContext.Progress.WriteLine(
						$"Выполняем скрипт {hop.Source.VersionToShortString()} → {hop.Destination.VersionToShortString()}");
					RunOneUpdate(connection, hop);
				}

				//Сравнение обновлённой базы и новой
				var builderCurrentDd = server.ConnectionStringBuilder;
				builderCurrentDd.Database = currentDdName;
				var connectionStringCurrentDd = builderCurrentDd.GetConnectionString(true);
				using(var connectionCurrentDd = new MySqlConnection(connectionStringCurrentDd)) {
					connectionCurrentDd.Open();
					ComparisonSchema(connectionCurrentDd, connection);
				}
			}

			//Сделал максимально просто. По хорошему объединить с настоящим обновлением.
			//Но это усложнит и так не простой код, может здесь вручную выполнять обновления даже лучше.
			void RunOneUpdate(MySqlConnection connection, UpdateHop updateScript) {
				if(updateScript.ExecuteBefore != null) {
					updateScript.ExecuteBefore(connection);
				}

				string sql;
				using(Stream stream = updateScript.Assembly.GetManifestResourceStream(updateScript.Resource)) {
					if(stream == null)
						throw new InvalidOperationException(String.Format("Ресурс {0} указанный в обновлениях не найден.",
							updateScript.Resource));
					StreamReader reader = new StreamReader(stream);
					sql = reader.ReadToEnd();
				}

				var script = new MySqlScript(connection, sql);
				string lastExecutedStatement = null;
				script.StatementExecuted += (sender, args) => lastExecutedStatement = $"[{args.Line}:{args.Position}]{args.StatementText}";
				try {
					script.Execute();
				}
				catch(Exception ex) {
					throw new Exception(
						$"Ошибка выполнения скрипта обновления {updateScript.Source.VersionToShortString()} → {updateScript.Destination.VersionToShortString()}\n" +
						$"Последнее выполненное выражение: {lastExecutedStatement}", ex);
				}

				var command = connection.CreateCommand();
				command.CommandText = "UPDATE base_parameters SET str_value = @version WHERE name = 'version'";
				command.Parameters.AddWithValue("version", updateScript.Destination.VersionToShortString());
				command.ExecuteNonQuery();
			}
		}

		public static IEnumerable<SqlServer> SqlServers {
			get {
				var configuration = TestsConfiguration.Configuration;
				var currentVersion = ScriptsConfiguration.MakeCreationScript().Version;
				var servers = configuration.GetSection("SQLServers").Get<List<SqlServer>>();
				foreach(var server in servers) {
					if(!String.IsNullOrEmpty(server.UseBefore) 
					   && Version.TryParse(server.UseBefore, out Version useBeforeVersion) 
					   && currentVersion >= useBeforeVersion)
						continue;
					yield return server;
				}
			}
		}
		
		[Test(Description = "Проверяем что можно создать базу из текущего скрипта создания.")]
		[TestCaseSource(nameof(SqlServers))]
		[Order(1)] //Тесты с указанным порядком выполняются раннее других. Нужно для сравнения обновленных баз с чистой установкой.
		public void CreateCurrentNewBaseTest(SqlServer server)
		{
			StartSqlServer(server);
			//Создаем чистую базу
			TestContext.Progress.WriteLine($"Создаем чистую базу {currentDdName} на сервере {server.Name}");
			var creator = new TestingCreateDbController(server);
			var success = creator.StartCreation(ScriptsConfiguration.MakeCreationScript(), currentDdName);
			Assert.That(success, Is.True);
		}

		#region SQL Servers

		void StartSqlServer(SqlServer server) 
		{
			if(server.Equals(RunningServer))
				return;
			if(RunningServer != null) {
				TestContext.Progress.WriteLine($"Останавливаем сервер {RunningServer.Name}");
				RunningServer.Stop();
				RunningServer = null;
			}
			TestContext.Progress.WriteLine($"Запускаем сервер {server.Name}");
			server.Start();
			RunningServer = server;
		}

		[OneTimeTearDown]
		public void StopSqlServer()
		{
			if(RunningServer != null)
				RunningServer.Stop();
			RunningServer = null;
		}
		
		#endregion
		#region Compare DB
		private string GetVersion(MySqlConnection connection, string db) {
			var sql = $"SELECT `str_value` FROM {db}.`base_parameters` WHERE `name` = 'version';";
			return (string)connection.ExecuteScalar(sql);
		}

		private void ComparisonSchema(MySqlConnection connection1, MySqlConnection connection2) {
			TestContext.Progress.WriteLine($"Сравниваем схемы базы {connection1.Database} и {connection2.Database}.");
			var versionDb1 = GetVersion(connection1, connection1.Database);
			var versionDb2 = GetVersion(connection2, connection2.Database);
			Assert.That(versionDb1, Is.EqualTo(versionDb2), $"Версии у баз различаются. '{ versionDb1}' и '{versionDb2}'");

			var compareResult = DBSchemaEqual(connection1, connection2, true, Console.WriteLine);
			var reverseCompareResult = DBSchemaEqual(connection2, connection1, false, Console.WriteLine);
			Assert.That(compareResult && reverseCompareResult, Is.True,"Схемы баз отличаются");
		}

		private bool DBSchemaEqual(MySqlConnection connection1, MySqlConnection connection2, bool checkDiff, RowOfSchema.Log log) {
			bool result = true;

			foreach(string schema in new List<string> {"Tables", "Foreign Keys", "Indexes", "IndexColumns", "Columns"})  {
				var db1 = connection1.GetSchema(schema).Rows
					.Cast<DataRow>()
					.Select(x => new RowOfSchema(schema, connection1.Database, x))
					.ToDictionary(x => x.FullName, x => x);

				var db2 = connection2.GetSchema(schema).Rows
					.Cast<DataRow>()
					.Select(x => new RowOfSchema(schema, connection2.Database, x))
					.ToDictionary(x => x.FullName, x => x);

				foreach(var row in db1) {
					if(db2.ContainsKey(row.Key)) {
						if(checkDiff && db1[row.Key].IsDiff(db2[row.Key], log)) {
							//детали переданы в log()
							result = false;
						}
					}
					else {
						log($"{schema} — значение {row.Value.FullName}\n" +
						    $"  присутствует в    {connection1.Database}\n" +
						    $"  но отсутствует в  {connection2.Database}");
						result = false;
					}
				}
			}

			return result;
		}
		#endregion
	}
}

