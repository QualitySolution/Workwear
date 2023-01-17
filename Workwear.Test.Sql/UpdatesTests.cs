using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Dapper;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
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
		private int showDiffLinesBefore = 30;
		private int showDiffLinesAfter = 5;
		
		private SqlServer RunningServer { get; set; }
		
		public static IEnumerable<object[]> DbSamples {
			get {
				var configuration = TestsConfiguration.Configuration;
				List<SqlServer> servers = configuration.GetSection("SQLServers").Get<List<SqlServer>>();
				List<DbSample> samples = configuration.GetSection("Samples").Get<List<DbSample>>();
				foreach (var server in servers) {
					foreach (var dbSample in samples) {
						if(!String.IsNullOrEmpty(dbSample.ForServerGroup) && !dbSample.ForServerGroup.Equals(server.Group))
							continue;
							
						yield return new object[] { server, dbSample };
					}
				}
			}
		}

		[TestCaseSource(nameof(DbSamples))]
		public void ApplyUpdatesTest(SqlServer server, DbSample sample) {
			var updateConfiguration = ScriptsConfiguration.MakeUpdateConfiguration();
			//Проверяем нужно ли обновлять 
			if(!updateConfiguration.GetHopsToLast(sample.TypedVersion).Any())
				Assert.Ignore($"Образец базы {sample} версии пропущен. Так как версию базы {sample.Version} невозможно обновить.");

			//Создаем чистую базу
			StartSqlServer(server);
			TestContext.Progress.WriteLine($"Создаем базу {sample.DbName}");
			var creator = new TestingCreateDbController(server);
			var success = creator.StartCreation(sample);
			Assert.That(success, Is.True);
			//Выполняем обновление
			var builder = server.ConnectionStringBuilder;
			builder.Database = sample.DbName;
			var connectionstring = builder.GetConnectionString(true);
			using(var connection = new MySqlConnection(connectionstring)) {
				connection.Open();
				foreach(var hop in updateConfiguration.GetHopsToLast(sample.TypedVersion)) {
					TestContext.Progress.WriteLine(
						$"Выполняем скрипт {hop.Source.VersionToShortString()} → {hop.Destination.VersionToShortString()}");
					RunOneUpdate(connection, hop);
				}

				//Сравнение обновлённой базы и новой
				var builderСurrentDd = server.ConnectionStringBuilder;
				builderСurrentDd.Database = currentDdName;
				var connectionstringСurrentDd = builder.GetConnectionString(true);
				using(var connectionСurrentDd = new MySqlConnection(connectionstringСurrentDd)) {
					connectionСurrentDd.Open();

					ComparisonSchema(connectionСurrentDd, connection);
				}
			}

			//Сделал максимально просто. По хорошему объединить с настоящим обновлением.
			//Но это усложнит и так не простой код, может здесь вручную выполнять обновления даже лучше.
			void RunOneUpdate(MySqlConnection connection, UpdateHop updateScript) {
				if(updateScript.ExcuteBefore != null) {
					updateScript.ExcuteBefore(connection);
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
				//script.StatementExecuted += Script_StatementExecuted;
				script.Execute();
				var command = connection.CreateCommand();
				command.CommandText = "UPDATE base_parameters SET str_value = @version WHERE name = 'version'";
				command.Parameters.AddWithValue("version", updateScript.Destination.VersionToShortString());
				command.ExecuteNonQuery();
			}
		}

		public static IEnumerable<SqlServer> SqlServers {
			get {
				var configuration = TestsConfiguration.Configuration;
				return configuration.GetSection("SQLServers").Get<List<SqlServer>>();
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
		#region Compare DB Text
		//TODO: Удааить метод и методы используемые только в нём, если ComparisonSchema будет нормально работать
		private void ComparisonSchemaText(MySqlConnection connection, string db1, string db2) {
			TestContext.Progress.WriteLine($"Сравниваем схемы базы {db1} и {db2}.");
			var versionDb1 = GetVersion(connection, db1);
			var versionDb2 = GetVersion(connection, db2);
			Assert.That(versionDb1, Is.EqualTo(versionDb2), "Версии у баз различаются.");
			string schema1 = GetSchema(connection, db1);
			string schema2 = GetSchema(connection, db2);

			if (schema1 != schema2) {
				var diff = InlineDiffBuilder.Diff(schema1, schema2);
				
				for (int i = 0; i < diff.Lines.Count; i++)
				{
					bool showLine = false;
					for (int x = Math.Max(0, i - showDiffLinesAfter); x < Math.Min(diff.Lines.Count, i + showDiffLinesBefore); x++)
						if (diff.Lines[x].Type == ChangeType.Inserted || diff.Lines[x].Type == ChangeType.Deleted) {
							showLine = true;
							break;
						}
					
					if(!showLine)
						continue;

					var line = diff.Lines[i];
					switch (line.Type)
					{
						case ChangeType.Inserted:
							Console.Write("+ ");
							break;
						case ChangeType.Deleted:
							Console.Write("- ");
							break;
						default:
							Console.Write("  ");
							break;
					}
					Console.WriteLine(line.Text);
				}
			}
			
			Assert.That(schema1, Is.EqualTo(schema2));
		}

		private string GetVersion(MySqlConnection connection, string db) {
			var sql = $"SELECT `str_value` FROM {db}.`base_parameters` WHERE `name` = 'version';";
			return (string)connection.ExecuteScalar(sql);
		}
		
		private string GetSchema(MySqlConnection connection, string db)
		{
			TestContext.Progress.WriteLine($"Чтение схемы {db}...");
			connection.ChangeDatabase(db);
			using MySqlBackup mb = new MySqlBackup(connection.CreateCommand());
			mb.ExportInfo.ExportRows = false;
			mb.ExportInfo.RecordDumpTime = false;
			mb.ExportInfo.ResetAutoIncrement = true;
			var result = mb.ExportToString();
			return result.Replace("  ", " "); //Особенности MySqlBackup в месте где удаляется AutoIncrement образуется двойной пробел.
		}
		#endregion
		
		#region Compare DB
		private void ComparisonSchema(MySqlConnection connection1, MySqlConnection connection2) {
			TestContext.Progress.WriteLine($"Сравниваем схемы базы {connection1.Database} и {connection2.Database}.");
			var versionDb1 = GetVersion(connection1, connection1.Database);
			var versionDb2 = GetVersion(connection2, connection2.Database);
			Assert.That(versionDb1, Is.EqualTo(versionDb2), "Версии у баз различаются.");

			Assert.That(DBSchemaCompare(connection1, connection2, Console.WriteLine), Is.True,"Схемы баз отличаются");
		}

		private bool DBSchemaCompare(MySqlConnection connection1, MySqlConnection connection2, ColumnOfTable.Log log)
		{
			bool result = true;

			var db1 = connection1.GetSchema("Columns").Rows
				.Cast<DataRow>()
				.Select(x => new ColumnOfTable ((string)x[2], (string)x[3], x))
				.ToDictionary(x => x.FullName, x => x);
            
			var db2 = connection2.GetSchema("Columns").Rows
				.Cast<DataRow>()
				.Select(x => new ColumnOfTable ((string)x[2], (string)x[3], x))
				.ToDictionary(x => x.FullName, x => x);
 
			foreach (var column in db1)
			{
				if (db2.ContainsKey(column.Key))
					db1[column.Key].IsDiff(db2[column.Key], log);
				else 
					log($" {column.Value} в базе {connection2.Database} отсутствует.");
			}

			return result;
		}
		#endregion
	}
}
