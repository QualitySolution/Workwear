using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
	public class UpdatesTests
	{
		private static readonly string currentDdName = "workwear_sqltest_current";
		private int showDiffLinesBefore = 30;
		private int showDiffLinesAfter = 5;
		
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
			var builder = new MySqlConnectionStringBuilder();
			builder.Server = server.GetValue<string>("Address");
			builder.UserID = server.GetValue<string>("Login");
			builder.Password = server.GetValue<string>("Password");
			builder.Database = sample.DbName;
			var connectionstring = builder.GetConnectionString(true);
			using (var connection = new MySqlConnection(connectionstring))
			{
				connection.Open();
				foreach (var hop in updateConfiguration.GetHopsToLast(sample.TypedVersion)) {
					TestContext.Progress.WriteLine($"Выполняем скрипт {hop.Source.VersionToShortString()} → {hop.Destination.VersionToShortString()}");
					RunOneUpdate(connection, hop);
				}
				ComparisonSchema(connection, currentDdName, sample.DbName);
			}
		}
		
		//Сделал максимально просто. По хорошему объединить с настоящим обновлением.
		//Но это усложнит и так не простой код, может здесь вручную выполнять обновления даже лучше.
		void RunOneUpdate(MySqlConnection connection, UpdateHop updateScript)
		{
			if(updateScript.ExcuteBefore != null) {
				updateScript.ExcuteBefore(connection);
			}
			
			string sql;
			using (Stream stream = updateScript.Assembly.GetManifestResourceStream(updateScript.Resource)) {
				if (stream == null)
					throw new InvalidOperationException(String.Format("Ресурс {0} указанный в обновлениях не найден.", updateScript.Resource));
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

		[Test(Description = "Проверяем что можно создать базу из текущего скрипта создания.")]
		[Order(1)] //Тесты с указанным порядком выполняются раннее других. Нужно для сравнения обновленных баз с чистой установкой.
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
			var success = creator.StartCreation(ScriptsConfiguration.MakeCreationScript(), currentDdName);
			Assert.That(success, Is.True);
		}

		#region Compare DB
		private void ComparisonSchema(MySqlConnection connection, string db1, string db2) {
			TestContext.Progress.WriteLine($"Сравниваем схемы базы {db1} и {db2}.");
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
	}
}
