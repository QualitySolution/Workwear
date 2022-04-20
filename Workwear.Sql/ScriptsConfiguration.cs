using System;
using System.Data.Common;
using System.Reflection;
using QS.DBScripts.Models;
using QS.Updater.DB;

namespace Workwear.Sql
{
	public class ScriptsConfiguration
	{
		public static CreationScript MakeCreationScript()
		{
			return new CreationScript(
				Assembly.GetAssembly(typeof(ScriptsConfiguration)),
				"Workwear.Sql.Scripts.new_empty.sql",
				new Version(2, 6)
			);
		}

		public static UpdateConfiguration MakeUpdateConfiguration()
		{
			var configuration = new UpdateConfiguration();

			configuration.AddUpdate (
				new Version (1, 0),
				new Version (1, 0, 4),
				"Workwear.Sql.Scripts.1.0.4.sql");
			configuration.AddUpdate (
				new Version (1, 0, 4),
				new Version (1, 0, 5),
				"Workwear.Sql.Scripts.1.0.5.sql");
			configuration.AddUpdate (
				new Version (1, 0, 5),
				new Version (1, 1),
				"Workwear.Sql.Scripts.1.1.sql");
			configuration.AddUpdate (
				new Version (1, 1),
				new Version (1, 2),
				"Workwear.Sql.Scripts.1.2.sql");
			configuration.AddUpdate (
				new Version (1, 2),
				new Version (1, 2, 1),
				"Workwear.Sql.Scripts.1.2.1.sql");
			configuration.AddUpdate (
				new Version (1, 2, 1),
				new Version (1, 2, 2),
				"Workwear.Sql.Scripts.1.2.2.sql");
			configuration.AddUpdate (
				new Version (1, 2, 2),
				new Version (1, 2, 4),
				"Workwear.Sql.Scripts.1.2.4.sql");
			configuration.AddUpdate (
				new Version (1, 2, 4),
				new Version (2, 0),
				"Workwear.Sql.Scripts.2.0.sql");
			configuration.AddUpdate(
				new Version(2, 0),
				new Version(2, 0, 2),
				"Workwear.Sql.Scripts.2.0.2.sql");
			configuration.AddUpdate(
				new Version(2, 0, 2),
				new Version(2, 1),
				"Workwear.Sql.Scripts.2.1.sql");
			configuration.AddUpdate(
				new Version(2, 1),
				new Version(2, 1, 1),
				"Workwear.Sql.Scripts.2.1.1.sql");
			configuration.AddUpdate(
				new Version(2, 1, 1),
				new Version(2, 2),
				"Workwear.Sql.Scripts.2.2.sql");
			configuration.AddUpdate(
				new Version(2, 2),
				new Version(2, 3),
				"Workwear.Sql.Scripts.2.3.sql");
			configuration.AddUpdate(
				new Version(2, 3),
				new Version(2, 3, 3),
				"Workwear.Sql.Scripts.2.3.3.sql");
			configuration.AddUpdate(
				new Version(2, 3, 3),
				new Version(2, 4),
				"Workwear.Sql.Scripts.2.4.sql");
			configuration.AddUpdate(
				new Version(2, 4),
				new Version(2, 4, 1),
				"Workwear.Sql.Scripts.2.4.1.sql");
			configuration.AddUpdate(
				new Version(2, 4, 1),
				new Version(2, 4, 3),
				"Workwear.Sql.Scripts.2.4.3.sql");
			configuration.AddUpdate(
				new Version(2, 4, 3),
				new Version(2, 5),
				"Workwear.Sql.Scripts.2.5.sql",
				//Необходимо только потому что MySQL не поддерживает синтаксис ADD INDEX IF NOT EXISTS
				delegate (DbConnection connection) {
					DropForeignKeyIfExist(connection, "operation_issued_by_employee", "fk_operation_issued_by_employee_4"); 
					DropIndexIfExist(connection, "operation_issued_by_employee", "fk_operation_issued_by_employee_4_idx");
					DropIndexIfExist(connection, "operation_issued_by_employee", "fk_operation_issued_by_employee_6_idx");
				});
			configuration.AddUpdate(
				new Version(2, 5),
				new Version(2, 5, 1),
				"Workwear.Sql.Scripts.2.5.1.sql");
			configuration.AddUpdate(
				new Version(2,5, 1),
				new Version(2,6),
				"Workwear.Sql.Scripts.2.6.sql");

			return configuration;
		}

		private static void DropIndexIfExist(DbConnection connection, string tableName, string indexName)
		{
			try {
				string sql = $"ALTER TABLE `{tableName}` DROP INDEX `{indexName}`;";
				var cmd = connection.CreateCommand();
				cmd.CommandText = sql;
				cmd.ExecuteNonQuery();
			}
			//При отсутствии индекса будет ошибка. Мы на это рассчитываем.
			catch(MySql.Data.MySqlClient.MySqlException ex) when(ex.Number == 1091) { }
		}
		
		private static void DropForeignKeyIfExist(DbConnection connection, string tableName, string indexName)
		{
			try {
				string sql = $"ALTER TABLE `{tableName}` DROP FOREIGN KEY `{indexName}`;";
				var cmd = connection.CreateCommand();
				cmd.CommandText = sql;
				cmd.ExecuteNonQuery();
			}
			//При отсутствии индекса будет ошибка. Мы на это рассчитываем.
			catch(MySql.Data.MySqlClient.MySqlException ex) when(ex.Number == 1091) {}
		}
	}
}