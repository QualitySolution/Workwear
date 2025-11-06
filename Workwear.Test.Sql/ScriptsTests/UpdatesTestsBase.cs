using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MySqlConnector;
using NLog;
using NUnit.Framework;
using QS.DBScripts.Controllers;
using QS.Updater.DB;
using QS.Utilities.Text;
using Workwear.Sql;
using Workwear.Test.Sql.Models;

namespace Workwear.Test.Sql.ScriptsTests
{
	[TestFixture]
	public abstract class UpdatesTestsBase
	{
		public static readonly string CurrentDdName = "workwear_sqltest_current";
		protected SqlServer server;
		
		public static IEnumerable<object[]> DbSamples {
			get {
				var dumpsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dumps");
				if (!Directory.Exists(dumpsPath)) {
					dumpsPath = Path.Combine(Directory.GetCurrentDirectory(), "Dumps");
				}
				
				if (!Directory.Exists(dumpsPath)) {
					yield break;
				}

				var sqlFiles = Directory.GetFiles(dumpsPath, "*.sql", SearchOption.TopDirectoryOnly)
					.OrderBy(f => f);

				foreach (var sqlFile in sqlFiles) {
					var fileName = Path.GetFileName(sqlFile);
					// Извлекаем версию из имени файла (например, empty_2.8.sql -> 2.8)
					var versionPart = fileName.Replace("empty_", "").Replace(".sql", "");
					var dbName = $"workwear_sqltest_{versionPart.Replace(".", "_")}";

					var dbSample = new DbSample {
						SqlFile = sqlFile,
						Version = versionPart,
						DbName = dbName
					};

					yield return new object[] { dbSample };
				}
			}
		}

		[OneTimeSetUp]
		public async Task Setup() {
			LogManager.Setup().LoadConfiguration(builder => {
				builder.ForLogger().FilterMinLevel(LogLevel.Info).WriteToConsole();
			});
			await InitialiseContainer();
		}
		
		[OneTimeTearDown]
		public virtual async Task OneTimeTearDown() {
			await DisposeContainer();
		}

		protected abstract Task InitialiseContainer();
		protected abstract Task DisposeContainer();

		[TestCaseSource(nameof(DbSamples))]
		public void ApplyUpdatesTest(DbSample sample) {
			var updateConfiguration = ScriptsConfiguration.MakeUpdateConfiguration();
			//Проверяем нужно ли обновлять 
			if(!updateConfiguration.GetHopsToLast(sample.TypedVersion, false).Any())
				Assert.Ignore($"Образец базы {sample} версии пропущен. Так как версию базы {sample.Version} невозможно обновить.");

			//Загружаем образец базы на SQL сервер.
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

				//Проверяем наличие базы для сравнения
				var checkDbSql = $"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{CurrentDdName}'";
				var dbExists = connection.ExecuteScalar(checkDbSql);
				if(dbExists == null) {
					TestContext.Progress.WriteLine($"База {CurrentDdName} не найдена. Создаем её перед сравнением.");
					//Нужно для возможности запускать только выбранный тест обновления.
					CreateCurrentNewBaseTest();
				}
				
				//Сравнение обновлённой базы и новой
				ComparisonSchema(connection, CurrentDdName, sample.DbName);
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
		
		[Test(Description = "Проверяем что можно создать базу из текущего скрипта создания.")]
		[Order(1)] //Тесты с указанным порядком выполняются раннее других. Нужно для сравнения обновленных баз с чистой установкой.
		public void CreateCurrentNewBaseTest()
		{
			// Получаем и выводим версию сервера
			using(var connection = new MySqlConnection(server.ConnectionStringBuilder.ConnectionString)) {
				connection.Open();
				var serverVersion = connection.ServerVersion;
				TestContext.Progress.WriteLine($"Версия SQL сервера: {serverVersion}");
			}
			
			TestContext.Progress.WriteLine($"Создаем чистую базу {CurrentDdName} из текущего скрипта создания.");
			var creator = new TestingCreateDbController(server);
			var success = creator.StartCreation(ScriptsConfiguration.MakeCreationScript(), CurrentDdName);
			Assert.That(success, Is.True);
		}
		
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
				
				Assert.That(db1, Is.Not.Empty, $"Метаданные {schema.Name} в базе {dbname1} отсутствуют");
				Assert.That(db2, Is.Not.Empty, $"Метаданные {schema.Name} в базе {dbname2} отсутствуют");

				foreach(var row in db1) {
					if(db2.TryGetValue(row.Key, out var value)) {
						if(db1[row.Key].IsDiff(value, log)) {
							//детали переданы в log()
							result = false;
						}
					}
					else {
						log($"{schema.Name} — значение {row.Value.FullName}\n" +
						    $"  присутствует в    {dbname1}\n" +
						    $"  но отсутствует в  {dbname2}");
						result = false;
					}
				}
				
				foreach(var row in db2) {
					if(!db1.ContainsKey(row.Key)) {
						log($"{schema.Name} — значение {row.Value.FullName}\n" +
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

