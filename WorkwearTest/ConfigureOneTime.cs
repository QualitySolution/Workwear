using System;
using QS.BusinessCommon.Domain;
using QS.Project.DB;
using QS.Project.Domain;
using workwear;
using workwear.HibernateMapping;

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

			//Использование другого способа формировать id объектов, чтобы они не пересекались.
			//По умолчанию все первые объекты каждого типа были 1, а вторые 2. Если сравнивать объекты по id, то не всегда можно
			//действительно проверить что id правильный. Что в некторых случаях приводило к некоректной проверки в тестах. Когда сравнивались id, разных объектов,
			//при этом тест проходил так как номера их совпадали.
			MappingParams.UseIdsForTest = true;
			Console.WriteLine("Инициализация");
			var db_config = FluentNHibernate.Cfg.Db.MonoSqliteConfiguration.Standard.InMemory();

			Console.WriteLine("ORM");
			// Настройка ORM
			OrmConfig.ConfigureOrm(db_config, new System.Reflection.Assembly[] {
				System.Reflection.Assembly.GetAssembly (typeof(Workwear.Domain.Users.UserSettings)),
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
