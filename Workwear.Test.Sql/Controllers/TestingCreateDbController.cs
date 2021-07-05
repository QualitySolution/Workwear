using System;
using QS.DBScripts.Models;
using QS.Dialog;
using Workwear.Test.Sql;

namespace QS.DBScripts.Controllers
{
	public class TestingCreateDbController : IDbCreateController
	{
		private readonly string server;
		private readonly string login;
		private readonly string password;
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public TestingCreateDbController(string server, string login, string password)
		{
			this.server = server;
			this.login = login;
			this.password = password;
			Progress = NSubstitute.Substitute.For<IProgressBarDisplayable>();
		}

		public bool StartCreation(DbSample sample)
		{
			var creationScript = new CreationScript(TestsConfiguration.MakeSQLScriptPath(sample.SqlFile), sample.TypedVersion);
			var createModel = new MySqlDbCreateModel(this, creationScript);
			return createModel.RunCreation(server, sample.DbName, login, password);
		}

		#region Взаимодействие с моделью
		public void WasError(string text)
		{
			throw new Exception(text);
		}

		public bool BaseExistDropIt(string dbname)
		{
			return true;
		}

		#region Свойства процесса
		public IProgressBarDisplayable Progress { get; }
		#endregion
		#endregion
	}
}