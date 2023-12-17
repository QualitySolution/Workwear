using System;
using System.Data.Common;
using System.Reflection;
using MySqlConnector;
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
				new Version(2, 8)
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
				"Workwear.Sql.Scripts.2.4.1.sql", 
				delegate (DbConnection connection) {
					DropForeignKeyIfExist(connection, "stock_transfer_detail", "fk_stock_transfer_detail_1"); 
					DropForeignKeyIfExist(connection, "stock_transfer_detail", "fk_stock_transfer_detail_2"); 
					DropForeignKeyIfExist(connection, "stock_transfer_detail", "fk_stock_transfer_detail_3"); 
					DropIndexIfExist(connection, "stock_transfer_detail", "fk_stock_transfer_detail_1_idx");
					DropIndexIfExist(connection, "stock_transfer_detail", "fk_stock_transfer_detail_2_idx");
					DropIndexIfExist(connection, "stock_transfer_detail", "fk_stock_transfer_detail_3_idx");
				});
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
			configuration.AddUpdate(
				new Version(2, 6),
				new Version(2, 6, 1),
				"Workwear.Sql.Scripts.2.6.1.sql");
			configuration.AddUpdate(
				new Version(2, 6, 1),
				new Version(2, 7),
				"Workwear.Sql.Scripts.2.7.sql");
			configuration.AddUpdate(
				new Version(2, 7),
				new Version(2, 7, 1),
				"Workwear.Sql.Scripts.2.7.1.sql");
			configuration.AddUpdate(
				new Version(2, 7, 1),
				new Version(2, 8),
				"Workwear.Sql.Scripts.2.8.sql");
			configuration.AddUpdate(
				new Version(2, 8),
				new Version(2, 8, 1),
				"Workwear.Sql.Scripts.2.8.1.sql"
			);
			configuration.AddUpdate(
				new Version(2, 8, 1),
				new Version(2, 8, 4),
				"Workwear.Sql.Scripts.2.8.4.sql"
			);
			configuration.AddUpdate(
				new Version(2, 8, 4),
				new Version(2, 8, 8),
				"Workwear.Sql.Scripts.2.8.8.sql"
			);
			configuration.AddUpdate(
				new Version(2, 8, 8),
				new Version(2, 8, 9),
				"Workwear.Sql.Scripts.2.8.9.sql"
			);
			configuration.AddUpdate(
            	new Version(2, 8, 9),
            	new Version(2, 8, 10),
            	"Workwear.Sql.Scripts.2.8.10.sql"
            );
			configuration.AddUpdate(
				new Version(2, 8, 10),
				new Version(2, 8, 11),
				"Workwear.Sql.Scripts.2.8.11.sql"
			);
			configuration.AddUpdate(
				new Version(2, 8, 11),
				new Version(2, 8, 12),
				"Workwear.Sql.Scripts.2.8.12.sql"
			);
			configuration.AddUpdate(
            	new Version(2, 8, 12),
            	new Version(2, 8, 13),
            	"Workwear.Sql.Scripts.2.8.13.sql"
            );
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
			catch(MySqlException ex) when(ex.Number == 1091) { }
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
			catch(MySqlException ex) when(ex.Number == 1091) {}
		}
	}
}
