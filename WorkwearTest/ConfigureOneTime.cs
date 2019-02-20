using System;
using QS.Project.DB;
using QSBusinessCommon.Domain;
using workwear;

namespace WorkwearTest.Deletion
{
	public static class ConfigureOneTime
	{
		static bool NhConfigered = false;
		static bool DeletionConfigured = false;

		public static void ConfigureNh()
		{
			if(NhConfigered)
				return;

			Console.WriteLine("Инициализация");
			var db_config = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
								.Dialect<NHibernate.Dialect.MySQL57Dialect>()
								.ConnectionString("server=vod.qsolution.ru;port=3306;database=test-test;user id=test_only;password=7qqKWuNugQF2Y2W1;sslmode=None;");

			Console.WriteLine("ORM");
			// Настройка ORM
			OrmConfig.ConfigureOrm(db_config, new System.Reflection.Assembly[] {
				System.Reflection.Assembly.GetAssembly (typeof(workwear.Domain.Users.UserSettings)),
				System.Reflection.Assembly.GetAssembly (typeof(MeasurementUnits)),
			});

			NhConfigered = true;
		}

		public static void ConfogureDeletion()
		{
			if(DeletionConfigured)
				return;

			Console.WriteLine("Delete");
			Configure.ConfigureDeletion();
			DeletionConfigured = true;
		}
	}
}
