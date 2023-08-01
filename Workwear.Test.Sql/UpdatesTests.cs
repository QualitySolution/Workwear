using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using NLog;
using NUnit.Framework;
using QS.DBScripts.Controllers;
using QS.Updater.DB;
using QS.Utilities.Text;
using Workwear.Sql;
using Workwear.Test.Sql.Models;

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
			builder.AllowUserVariables = true;
			var connectionString = builder.ConnectionString;
			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				foreach(var hop in updateConfiguration.GetHopsToLast(sample.TypedVersion, false)) {
					TestContext.Progress.WriteLine(
						$"Выполняем скрипт {hop.Source.VersionToShortString()} → {hop.Destination.VersionToShortString()}");
					RunOneUpdate(connection, hop);
				}

				//Сравнение обновлённой базы и новой
				ComparisonSchema(connection, currentDdName, sample.DbName);
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

		private void ComparisonSchema(MySqlConnection connection, string db1, string db2) {
			TestContext.Progress.WriteLine($"Сравниваем схемы базы {db1} и {db2}.");
			var versionDb1 = GetVersion(connection, db1);
			var versionDb2 = GetVersion(connection, db2);
			Assert.That(versionDb1, Is.EqualTo(versionDb2), $"Версии у баз различаются. '{ versionDb1}' и '{versionDb2}'");

			var compareResult = DBSchemaEqual(connection, db1, db2, Console.WriteLine);
			Assert.That(compareResult, Is.True,"Схемы баз отличаются");
		}

		private static bool DBSchemaEqual(MySqlConnection connection, string dbname1, string dbname2, RowOfSchema.Log log) {
			bool result = true;

			foreach(var schema in SchemaInfo.Schemas) {
				var allRows = connection.GetSchema(schema.Name).Rows
					.Cast<DataRow>().ToList();
				var db1 = allRows	
					.Where(row => (string)row[row.Table.Columns.IndexOf(schema.DataBaseColumn)] == dbname1)
					.Select(x => new RowOfSchema(schema, x))
					.ToDictionary(x => x.FullName, x => x);
				var db2 = allRows
					.Where(row => (string)row[row.Table.Columns.IndexOf(schema.DataBaseColumn)] == dbname2)
					.Select(x => new RowOfSchema(schema, x))
					.ToDictionary(x => x.FullName, x => x);
				
				Assert.That(db1, Is.Not.Empty, $"Метаданные {schema} в базе {dbname1} отсутствуют");
				Assert.That(db2, Is.Not.Empty, $"Метаданные {schema} в базе {dbname2} отсутствуют");

				foreach(var row in db1) {
					if(db2.TryGetValue(row.Key, out var value)) {
						if(db1[row.Key].IsDiff(value, log)) {
							//детали переданы в log()
							result = false;
						}
					}
					else {
						log($"{schema} — значение {row.Value.FullName}\n" +
						    $"  присутствует в    {dbname1}\n" +
						    $"  но отсутствует в  {dbname2}");
						result = false;
					}
				}
				
				foreach(var row in db2) {
					if(!db1.ContainsKey(row.Key)) {
						log($"{schema} — значение {row.Value.FullName}\n" +
						    $"  присутствует в    {dbname2}\n" +
						    $"  но отсутствует в  {dbname1}");
						result = false;
					}
				}
			}

			return result;
		}
		#endregion
	}
}

