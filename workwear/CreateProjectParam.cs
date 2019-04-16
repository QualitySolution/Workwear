using QS.Project.DB;
using QS.Project.Domain;
using QSBusinessCommon.Domain;
using QSOrmProject;
using QSProjectsLib;
using workwear.Dialogs.Organization;
using workwear.Dialogs.Regulations;
using workwear.Domain.Organization;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Domain.Users;

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

			OrmConfig.ConfigureOrm (db, new System.Reflection.Assembly[] {
				System.Reflection.Assembly.GetAssembly (typeof(MainClass)),
				System.Reflection.Assembly.GetAssembly (typeof(MeasurementUnits)),
			});

			//Настраиваем классы сущьностей

			OrmMain.AddObjectDescription(MeasurementUnits.GetOrmMapping());
			//Спецодежда
			OrmMain.AddObjectDescription<RegulationDoc>().Dialog<RegulationDocDlg>().DefaultTableView().SearchColumn("Документ", i => i.Title).OrderAsc(i => i.Name).End();
			OrmMain.AddObjectDescription<Norm>().Dialog<NormDlg>();
			OrmMain.AddObjectDescription<ItemsType>().Dialog<ItemTypeDlg> ().DefaultTableView ().SearchColumn ("Наименование", i => i.Name).OrderAsc (i => i.Name).End ();
			OrmMain.AddObjectDescription<Post>().DefaultTableView ().SearchColumn ("Название", e => e.Name).End ();
			//Организация
			OrmMain.AddObjectDescription<EmployeeCard>().Dialog<EmployeeCardDlg>().UseSlider(false).DefaultTableView ().SearchColumn ("Имя", e => e.FullName).End ();
			OrmMain.AddObjectDescription<EmployeeVacation>().Dialog<EmployeeVacationDlg>();
			OrmMain.AddObjectDescription<Facility>().Dialog<ObjectDlg> ().DefaultTableView ().SearchColumn ("Название", e => e.Name).SearchColumn ("Адрес", e => e.Address).End ();
			OrmMain.AddObjectDescription<Leader>().DefaultTableView ().SearchColumn ("Имя", e => e.Name).End ();
			OrmMain.AddObjectDescription<VacationType>().Dialog<VacationTypeDlg>().DefaultTableView().SearchColumn("Название", e => e.Name).Column("Исключать из носки", e => e.ExcludeFromWearing ? "Да" : "Нет").SearchColumn("Комментарий", e => e.Comments).End();
			//Общее
			OrmMain.AddObjectDescription<UserBase>().DefaultTableView ().Column ("Имя", e => e.Name).End ();
			OrmMain.AddObjectDescription<UserSettings>();
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
