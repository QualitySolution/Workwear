using System.Threading.Tasks;
using MySqlConnector;
using NUnit.Framework;
using QS.Testing.DB;
using Workwear.Sql;

namespace Workwear.Test.Sql {
	[Parallelizable(ParallelScope.Fixtures)]
	public class WorkwearDbTestFixtureBase : MariaDbTestContainerSqlFixtureBase {
		protected string WorkwearDBName = "workwear";
		
		protected async Task PrepareWorkwearDatabase(string additionalSql = null, string dbName = null, MySqlConnection connection = null, bool isDefaultDb = true)
		{
			var script = ScriptsConfiguration.MakeCreationScript();
			var sqlScript = script.GetSqlScript();
			sqlScript = sqlScript.Replace("DELIMITER $$", "").Replace("DELIMITER ;", "").Replace("END$$", "END;");
			if(!string.IsNullOrWhiteSpace(additionalSql))
				sqlScript += "\n" + additionalSql;
			
			if(isDefaultDb)
				DefaultDbName = dbName ?? WorkwearDBName;

			await PrepareDatabase(sqlScript, connection: connection, dbName: dbName ?? WorkwearDBName);
		}
	}
}
