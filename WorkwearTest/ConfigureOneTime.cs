using System;
using QS.BusinessCommon.Domain;
using QS.Project.DB;
using QS.Project.Domain;
using workwear;

namespace WorkwearTest
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
			var db_config = FluentNHibernate.Cfg.Db.MonoSqliteConfiguration.Standard.InMemory();

			Console.WriteLine("ORM");
			// Настройка ORM
			OrmConfig.ConfigureOrm(db_config, new System.Reflection.Assembly[] {
				System.Reflection.Assembly.GetAssembly (typeof(workwear.Domain.Users.UserSettings)),
				System.Reflection.Assembly.GetAssembly (typeof(MeasurementUnits)),
				System.Reflection.Assembly.GetAssembly (typeof(UserBase)),
			});

			NhConfigered = true;
		}

		public static void ConfigureDeletion()
		{
			if(DeletionConfigured)
				return;

			Console.WriteLine("Delete");
			Configure.ConfigureDeletion();
			DeletionConfigured = true;
		}
	}
}
