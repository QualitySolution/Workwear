using System;
using Oracle.ManagedDataAccess.Client;
using QS.Dialog;
using QSMachineConfig;

namespace workwear.Tools.Oracle
{
	public class NLMKOracle
	{
		const string SectionName = "OracleConnection";
		const string ParamDataSource = "DataSource";
		const string ParamUser = "User";
		const string ParamPassword = "Password";

		public OracleConnection Connection;

		public NLMKOracle()
		{
		}

		public void Connect(IInteractiveMessage interactive)
		{
			var config = MachineConfig.ConfigSource.Configs[SectionName];
			if(config == null) {
				interactive.ShowMessage(ImportanceLevel.Error,
					$"Отсутствует настройка подключения к базе Oracle. Необходимо в файле {MachineConfig.ConfigSource.SavePath} " +
					"создать секцию:\n" +
					$"[{SectionName}]\n" +
					$"{ParamDataSource} = TOR.NLMK\n" +
					$"{ParamUser} = xxx\n" +
					$"{ParamPassword} = xxx");
				return;
			}
			if(!config.Contains(ParamDataSource)) {
				interactive.ShowMessage(ImportanceLevel.Error, $" В файле {MachineConfig.ConfigSource.SavePath}, в секции {SectionName}," +
						$"не указан параметр {ParamDataSource}.", "Не полная настройка подключения к Oracle");
				return;
			}
			if(!config.Contains(ParamUser)) {
				interactive.ShowMessage(ImportanceLevel.Error, $" В файле {MachineConfig.ConfigSource.SavePath}, в секции {SectionName}," +
						$"не указан параметр {ParamUser}.", "Не полная настройка подключения к Oracle");
				return;
			}
			if(!config.Contains(ParamPassword)) {
				interactive.ShowMessage(ImportanceLevel.Error, $" В файле {MachineConfig.ConfigSource.SavePath}, в секции {SectionName}," +
						$"не указан параметр {ParamPassword}.", "Не полная настройка подключения к Oracle");
				return;
			}

			var dataSource = config.Get(ParamDataSource);
			var user = config.Get(ParamUser);
			var password = config.Get(ParamPassword);

			var connectionString = $"Data Source={dataSource}; User Id={user};Password={password};";
			Connection = new OracleConnection(connectionString);
			try {
				Connection.Open();
			}
			catch(OracleException ex) when (ex.Number == 12545) {
				interactive.ShowMessage(ImportanceLevel.Error, "Не удалось подключится к базе НЛМК. Часть функциональности будет недоступна.");
			}
		}
	}
}
