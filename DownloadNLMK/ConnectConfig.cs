using QS.BusinessCommon.Domain;
using QS.Dialog;
using QS.Project.DB;
using QS.Services;
using QSMachineConfig;

namespace DownloadNLMK
{
	public static class ConnectConfig
	{
		const string SectionName = "MainConnection";
		const string ParamServer = "Server";
		const string ParamDataBase = "DataBase";
		const string ParamUser = "User";
		const string ParamPassword = "Password";

		public static void InitConnection(IInteractiveService interactive)
		{
			var config = MachineConfig.ConfigSource.Configs[SectionName];
			if(config == null) {
				interactive.ShowMessage(ImportanceLevel.Error,
					$"Отсутствует настройка подключения к базе. Необходимо в файле {MachineConfig.ConfigSource.SavePath} " +
					"создать секцию:\n" +
					$"[{SectionName}]\n" +
					$"{ParamServer} = xxx\n" +
					$"{ParamDataBase} = workwear\n" +
					$"{ParamUser} = xxx\n" +
					$"{ParamPassword} = xxx");
				return;
			}
			if(!config.Contains(ParamServer)) {
				interactive.ShowMessage(ImportanceLevel.Error, $" В файле {MachineConfig.ConfigSource.SavePath}, в секции {SectionName}," +
						$"не указан параметр {ParamServer}.", "Не полная настройка подключения");
				return;
			}
			if(!config.Contains(ParamDataBase)) {
				interactive.ShowMessage(ImportanceLevel.Error, $" В файле {MachineConfig.ConfigSource.SavePath}, в секции {SectionName}," +
						$"не указан параметр {ParamDataBase}.", "Не полная настройка подключения");
				return;
			}
			if(!config.Contains(ParamUser)) {
				interactive.ShowMessage(ImportanceLevel.Error, $" В файле {MachineConfig.ConfigSource.SavePath}, в секции {SectionName}," +
						$"не указан параметр {ParamUser}.", "Не полная настройка подключения");
				return;
			}
			if(!config.Contains(ParamPassword)) {
				interactive.ShowMessage(ImportanceLevel.Error, $" В файле {MachineConfig.ConfigSource.SavePath}, в секции {SectionName}," +
						$"не указан параметр {ParamPassword}.", "Не полная настройка подключения");
				return;
			}

			var server = config.Get(ParamServer);
			var database = config.Get(ParamDataBase);
			var user = config.Get(ParamUser);
			var password = config.Get(ParamPassword);

			var db = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
				.ConnectionString($"server={server};port=3306;database={database};user id={user};sslmode=None; password={password}")
				.AdoNetBatchSize(100)
				//.ShowSql()
				.FormatSql();

			OrmConfig.ConfigureOrm(db, new System.Reflection.Assembly[] {
				System.Reflection.Assembly.GetAssembly (typeof(workwear.Domain.Users.UserSettings)),
				System.Reflection.Assembly.GetAssembly (typeof(MeasurementUnits)),
			});
		}
	}
}
