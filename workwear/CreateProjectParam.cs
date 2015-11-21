using System;
using System.Collections.Generic;
using QSOrmProject;
using QSOrmProject.DomainMapping;
using QSProjectsLib;
using workwear.Domain;

namespace workwear
{
	partial class MainClass
	{
		static void CreateBaseConfig ()
		{
			logger.Info ("Настройка параметров базы...");

			// Настройка ORM
			var db = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
				.ConnectionString (QSMain.ConnectionString)
				.ShowSql ()
				.FormatSql ();

			OrmMain.ConfigureOrm (db, new System.Reflection.Assembly[] {
				System.Reflection.Assembly.GetAssembly (typeof(MainClass)),
			});
			OrmMain.ClassMappingList = new System.Collections.Generic.List<IOrmObjectMapping> {
				OrmObjectMapping<Facility>.Create ().Dialog<ObjectDlg> (),
				OrmObjectMapping<Post>.Create (),
				OrmObjectMapping<Leader>.Create (),
				OrmObjectMapping<User>.Create (),
				OrmObjectMapping<EmployeeCard>.Create ().Dialog<EmployeeCardDlg>()
			};
		}
	}
}
