using System;
using QS.DBScripts.Models;
using QS.Dialog;
using Workwear.Test.Sql;

namespace QS.DBScripts.Controllers
{
	public class TestingCreateDbController : IDbCreateController
	{
		private readonly SqlServer server;
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public TestingCreateDbController(SqlServer server)
		{
			this.server = server;
			Progress = NSubstitute.Substitute.For<IProgressBarDisplayable>();
		}

		public bool StartCreation(DbSample sample)
		{
			var creationScript = new CreationScript(TestsConfiguration.MakeSQLScriptPath(sample.SqlFile), sample.TypedVersion);
			var createModel = new MySqlDbCreateModel(this, creationScript);
			return createModel.RunCreation(server.AddressAndPort, sample.DbName, server.Login, server.Password);
		}
		
		public bool StartCreation(CreationScript script, string dbname)
		{
			var createModel = new MySqlDbCreateModel(this, script);
			return createModel.RunCreation(server.AddressAndPort, dbname, server.Login, server.Password);
		}

		#region Взаимодействие с моделью
		public void WasError(string text)
		{
			throw new Exception(text);
		}

		public bool NeedDropDatabaseIfExists(string dbname)
		{
			return true;
		}

		#region Свойства процесса
		public IProgressBarDisplayable Progress { get; }
		#endregion
		#endregion
	}
}