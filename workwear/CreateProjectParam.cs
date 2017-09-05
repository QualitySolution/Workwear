using System;
using QSBusinessCommon;
using QSBusinessCommon.Domain;
using QSOrmProject;
using QSOrmProject.DomainMapping;
using QSProjectsLib;
using workwear.Domain;
using workwear.Domain.Stock;

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
				System.Reflection.Assembly.GetAssembly (typeof(MeasurementUnits)),
			});

			OrmMain.ClassMappingList = new System.Collections.Generic.List<IOrmObjectMapping> {
				MeasurementUnits.GetOrmMapping (),
				OrmObjectMapping<Norm>.Create ().Dialog<NormDlg> (),
				OrmObjectMapping<ItemsType>.Create ().Dialog<ItemTypeDlg> ().DefaultTableView ().SearchColumn ("Наименование", i => i.Name).OrderAsc (i => i.Name).End (),
				OrmObjectMapping<Facility>.Create ().Dialog<ObjectDlg> ().DefaultTableView ().SearchColumn ("Название", e => e.Name).SearchColumn ("Адрес", e => e.Address).End (),
				OrmObjectMapping<Post>.Create ().DefaultTableView ().SearchColumn ("Название", e => e.Name).End (),
				OrmObjectMapping<Leader>.Create ().DefaultTableView ().SearchColumn ("Имя", e => e.Name).End (),
				OrmObjectMapping<User>.Create ().DefaultTableView ().Column ("Имя", e => e.Name).End (),
				OrmObjectMapping<EmployeeCard>.Create ().Dialog<EmployeeCardDlg>().UseSlider(false).DefaultTableView ().SearchColumn ("Имя", e => e.FullName).End (),
				//Склад
				OrmObjectMapping<Nomenclature>.Create ().Dialog<NomenclatureDlg> ().DefaultTableView ()
					.SearchColumn ("Наименование", i => i.Name)
					.SearchColumn ("Размер", i => i.Size)
					.SearchColumn ("Рост", i => i.WearGrowth).OrderAsc (i => i.Name).End (),
				OrmObjectMapping<Income>.Create().Dialog<IncomeDocDlg>(),
				OrmObjectMapping<Expense>.Create().Dialog<ExpenseDocDlg>(),
				OrmObjectMapping<Writeoff>.Create().Dialog<WriteOffDocDlg>(),
			};
		}
	}
}
