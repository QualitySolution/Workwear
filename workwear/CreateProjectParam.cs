using QSBusinessCommon.Domain;
using QSOrmProject;
using QSOrmProject.Domain;
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

			//Настраиваем классы сущьностей

			OrmMain.AddObjectDescription(MeasurementUnits.GetOrmMapping());
			//Спецодежда
			OrmMain.AddObjectDescription<Norm>().Dialog<NormDlg>();
			OrmMain.AddObjectDescription<ItemsType>().Dialog<ItemTypeDlg> ().DefaultTableView ().SearchColumn ("Наименование", i => i.Name).OrderAsc (i => i.Name).End ();
			OrmMain.AddObjectDescription<Facility>().Dialog<ObjectDlg> ().DefaultTableView ().SearchColumn ("Название", e => e.Name).SearchColumn ("Адрес", e => e.Address).End ();
			OrmMain.AddObjectDescription<Post>().DefaultTableView ().SearchColumn ("Название", e => e.Name).End ();
			OrmMain.AddObjectDescription<Leader>().DefaultTableView ().SearchColumn ("Имя", e => e.Name).End ();
			OrmMain.AddObjectDescription<EmployeeCard>().Dialog<EmployeeCardDlg>().UseSlider(false).DefaultTableView ().SearchColumn ("Имя", e => e.FullName).End ();
			//Общее
			OrmMain.AddObjectDescription<User>().DefaultTableView ().Column ("Имя", e => e.Name).End ();
			//Склад
			OrmMain.AddObjectDescription<Nomenclature>().Dialog<NomenclatureDlg> ().DefaultTableView ()
					.SearchColumn ("Наименование", i => i.Name)
					.SearchColumn ("Размер", i => i.Size)
					.SearchColumn ("Рост", i => i.WearGrowth).OrderAsc (i => i.Name).End ();
			OrmMain.AddObjectDescription<Income>().Dialog<IncomeDocDlg>();
			OrmMain.AddObjectDescription<Expense>().Dialog<ExpenseDocDlg>();
			OrmMain.AddObjectDescription<Writeoff>().Dialog<WriteOffDocDlg>();
		}
	}
}
