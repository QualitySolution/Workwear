using System;
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
				OrmObjectMapping<Facility>.Create ().Dialog<ObjectDlg> ().SimpleDisplay ().Column ("Название", e => e.Name).Column ("Адрес", e => e.Address).End (),
				OrmObjectMapping<Post>.Create ().SimpleDisplay ().Column ("Название", e => e.Name).End (),
				OrmObjectMapping<Leader>.Create ().SimpleDisplay ().Column ("Имя", e => e.Name).End (),
				OrmObjectMapping<User>.Create ().SimpleDisplay ().Column ("Имя", e => e.Name).End (),
				OrmObjectMapping<EmployeeCard>.Create ().Dialog<EmployeeCardDlg>().SimpleDisplay ().Column ("Имя", e => e.FullName).End ()
			};
		}
	}
}
