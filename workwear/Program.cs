using System;
using System.Collections.Generic;
using Gtk;
using NLog;
using QSProjectsLib;

namespace workwear
{
	class MainClass
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		public static MainWindow MainWin;

		public static void Main (string[] args)
		{
			Application.Init ();
			QSMain.SubscribeToUnhadledExceptions ();
			QSMain.GuiThread = System.Threading.Thread.CurrentThread;
			CreateProjectParam ();
			// Создаем окно входа
			Login LoginDialog = new QSProjectsLib.Login ();
			LoginDialog.Logo = Gdk.Pixbuf.LoadFromResource ("workwear.icon.logo.png");
			LoginDialog.SetDefaultNames ("workwear");
			LoginDialog.DefaultLogin = "demo";
			LoginDialog.DefaultServer = "demo.qsolution.ru";
			LoginDialog.DemoServer = "demo.qsolution.ru";
			LoginDialog.DemoMessage = "Для подключения к демострационному серверу используйте следующие настройки:\n" +
			"\n" +
			"<b>Сервер:</b> demo.qsolution.ru\n" +
			"<b>Пользователь:</b> demo\n" +
			"<b>Пароль:</b> demo\n" +
			"\n" +
			"Для установки собственного сервера обратитесь к документации.";
			LoginDialog.UpdateFromGConf ();

			ResponseType LoginResult;
			LoginResult = (ResponseType)LoginDialog.Run ();
			if (LoginResult == ResponseType.DeleteEvent || LoginResult == ResponseType.Cancel)
				return;

			LoginDialog.Destroy ();
			QSSaaS.Session.StartSessionRefresh();

			//Запускаем программу
			MainWin = new MainWindow ();
			if (QSMain.User.Login == "root")
				return;
			MainWin.Show ();
			Application.Run ();
			QSSaaS.Session.StopSessionRefresh();
		}

		static void CreateProjectParam ()
		{
			// Создаем параметы пользователей
			QSMain.ProjectPermission = new Dictionary<string, UserPermission> ();
			//QSMain.ProjectPermission.Add ("edit_slips", new UserPermission("edit_slips", "Изменение кассы задним числом",
			//                                                             "Пользователь может изменять или добавлять кассовые документы задним числом."));

			QSMain.User = new UserInfo ();

			//Создаем параметры удаления
			QSMain.ProjectTables = new Dictionary<string, TableInfo> ();
			TableInfo PrepareTable;

			PrepareTable = new TableInfo ();
			PrepareTable.ObjectsName = "Объекты";
			PrepareTable.ObjectName = "объект";
			PrepareTable.SqlSelect = "SELECT name, id FROM objects ";
			PrepareTable.DisplayString = "Объект {0}";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys ("id");
			PrepareTable.DeleteItems.Add ("object_places", 
				new TableInfo.DeleteDependenceItem ("WHERE object_id = @id ", "", "@id"));
			PrepareTable.DeleteItems.Add ("stock_expense", 
				new TableInfo.DeleteDependenceItem ("WHERE object_id = @id ", "", "@id"));
			PrepareTable.DeleteItems.Add ("stock_income", 
				new TableInfo.DeleteDependenceItem ("WHERE object_id = @id ", "", "@id"));
			PrepareTable.ClearItems.Add ("wear_cards", 
				new TableInfo.ClearDependenceItem ("WHERE object_id = @id", "", "@id", "object_id"));
			QSMain.ProjectTables.Add ("objects", PrepareTable);

			PrepareTable = new TableInfo ();
			PrepareTable.ObjectsName = "Размещения в объекте";
			PrepareTable.ObjectName = "размещение";
			PrepareTable.SqlSelect = "SELECT name, id FROM object_places ";
			PrepareTable.DisplayString = "{0}";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys ("id");
			PrepareTable.ClearItems.Add ("stock_expense_detail", 
				new TableInfo.ClearDependenceItem ("WHERE object_place_id = @id", "", "@id", "object_place_id"));
			QSMain.ProjectTables.Add ("object_places", PrepareTable);

			PrepareTable = new TableInfo ();
			PrepareTable.ObjectsName = "Виды номенклатуры";
			PrepareTable.ObjectName = "вид номенклатуры";
			PrepareTable.SqlSelect = "SELECT name, id FROM item_types ";
			PrepareTable.DisplayString = "{0}";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys ("id");
			PrepareTable.DeleteItems.Add ("nomenclature", 
				new TableInfo.DeleteDependenceItem ("WHERE type_id = @id ", "", "@id"));
			QSMain.ProjectTables.Add ("item_types", PrepareTable);

			PrepareTable = new TableInfo ();
			PrepareTable.ObjectsName = "Единицы измерения";
			PrepareTable.ObjectName = "единицу измерения";
			PrepareTable.SqlSelect = "SELECT name, id FROM units ";
			PrepareTable.DisplayString = "ед. изм. {0}";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys ("id");
			PrepareTable.ClearItems.Add ("nomenclature", 
				new TableInfo.ClearDependenceItem ("WHERE units_id = @id", "", "@id", "units_id"));
			QSMain.ProjectTables.Add ("units", PrepareTable);

			PrepareTable = new TableInfo ();
			PrepareTable.ObjectsName = "Пользователи";
			PrepareTable.ObjectName = "пользователя";
			PrepareTable.SqlSelect = "SELECT name, id FROM users ";
			PrepareTable.DisplayString = "пользователя {0}";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys ("id");
			PrepareTable.ClearItems.Add ("wear_cards", 
				new TableInfo.ClearDependenceItem ("WHERE user_id = @id", "", "@id", "user_id"));
			PrepareTable.ClearItems.Add ("stock_write_off", 
				new TableInfo.ClearDependenceItem ("WHERE user_id = @id", "", "@id", "user_id"));
			PrepareTable.ClearItems.Add ("stock_expense", 
				new TableInfo.ClearDependenceItem ("WHERE user_id = @id", "", "@id", "user_id"));
			PrepareTable.ClearItems.Add ("stock_income", 
				new TableInfo.ClearDependenceItem ("WHERE user_id = @id", "", "@id", "user_id"));
			QSMain.ProjectTables.Add ("users", PrepareTable);

			PrepareTable = new TableInfo ();
			PrepareTable.ObjectsName = "Руководители";
			PrepareTable.ObjectName = "руководителя";
			PrepareTable.SqlSelect = "SELECT name, id FROM leaders ";
			PrepareTable.DisplayString = "Руководитель {0}";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys ("id");
			PrepareTable.ClearItems.Add ("wear_cards", 
				new TableInfo.ClearDependenceItem ("WHERE leader_id = @id", "", "@id", "leader_id"));
			QSMain.ProjectTables.Add ("leaders", PrepareTable);

			PrepareTable = new TableInfo ();
			PrepareTable.ObjectsName = "Должности";
			PrepareTable.ObjectName = "должность";
			PrepareTable.SqlSelect = "SELECT name, id FROM posts ";
			PrepareTable.DisplayString = "Должность {0}";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys ("id");
			PrepareTable.ClearItems.Add ("wear_cards", 
				new TableInfo.ClearDependenceItem ("WHERE post_id = @id", "", "@id", "post_id"));
			QSMain.ProjectTables.Add ("posts", PrepareTable);

			PrepareTable = new TableInfo ();
			PrepareTable.ObjectsName = "Карточки сотрудников";
			PrepareTable.ObjectName = "карточку";
			PrepareTable.SqlSelect = "SELECT last_name, first_name, patronymic_name, id FROM wear_cards ";
			PrepareTable.DisplayString = "Карточка {0} {1} {2}";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys ("id");
			PrepareTable.DeleteItems.Add ("stock_expense", 
				new TableInfo.DeleteDependenceItem ("WHERE wear_card_id = @id ", "", "@id"));
			PrepareTable.DeleteItems.Add ("stock_income", 
				new TableInfo.DeleteDependenceItem ("WHERE wear_card_id = @id ", "", "@id"));
			QSMain.ProjectTables.Add ("wear_cards", PrepareTable);

			PrepareTable = new TableInfo ();
			PrepareTable.ObjectsName = "Номенклатура";
			PrepareTable.ObjectName = "номенклатуру";
			PrepareTable.SqlSelect = "SELECT name, id FROM nomenclature ";
			PrepareTable.DisplayString = "{0}";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys ("id");
			PrepareTable.DeleteItems.Add ("stock_expense_detail", 
				new TableInfo.DeleteDependenceItem ("WHERE nomenclature_id = @id ", "", "@id"));
			PrepareTable.DeleteItems.Add ("stock_income_detail", 
				new TableInfo.DeleteDependenceItem ("WHERE nomenclature_id = @id ", "", "@id"));
			PrepareTable.DeleteItems.Add ("stock_write_off_detail", 
				new TableInfo.DeleteDependenceItem ("WHERE nomenclature_id = @id ", "", "@id"));
			QSMain.ProjectTables.Add ("nomenclature", PrepareTable);

			PrepareTable = new TableInfo ();
			PrepareTable.ObjectsName = "Приходные документы";
			PrepareTable.ObjectName = "документ прихода на склад";
			PrepareTable.SqlSelect = "SELECT id, date FROM stock_income ";
			PrepareTable.DisplayString = "Приход №{0} от {1:d}";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys ("id");
			PrepareTable.DeleteItems.Add ("stock_income_detail", 
				new TableInfo.DeleteDependenceItem ("WHERE stock_income_id = @id ", "", "@id"));
			QSMain.ProjectTables.Add ("stock_income", PrepareTable);

			PrepareTable = new TableInfo ();
			PrepareTable.ObjectsName = "Расходные документы";
			PrepareTable.ObjectName = "документ выдачи со склада";
			PrepareTable.SqlSelect = "SELECT id, date FROM stock_expense ";
			PrepareTable.DisplayString = "Выдача №{0} от {1:d}";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys ("id");
			PrepareTable.DeleteItems.Add ("stock_expense_detail", 
				new TableInfo.DeleteDependenceItem ("WHERE stock_expense_id = @id ", "", "@id"));
			QSMain.ProjectTables.Add ("stock_expense", PrepareTable);

			PrepareTable = new TableInfo ();
			PrepareTable.ObjectsName = "Акты списания";
			PrepareTable.ObjectName = "акт списания";
			PrepareTable.SqlSelect = "SELECT id, date FROM stock_write_off ";
			PrepareTable.DisplayString = "Акт списания №{0} от {1:d}";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys ("id");
			PrepareTable.DeleteItems.Add ("stock_write_off_detail", 
				new TableInfo.DeleteDependenceItem ("WHERE stock_write_off_id = @id ", "", "@id"));
			QSMain.ProjectTables.Add ("stock_write_off", PrepareTable);

			PrepareTable = new TableInfo ();
			PrepareTable.ObjectsName = "Строки приходного документа";
			PrepareTable.ObjectName = "строку приходного документа";
			PrepareTable.SqlSelect = "SELECT nomenclature.name, stock_income_detail.quantity, units.name, stock_income.date, stock_income_detail.id " +
			"FROM stock_income_detail " +
			"LEFT JOIN nomenclature ON nomenclature.id = stock_income_detail.nomenclature_id " +
			"LEFT JOIN units ON units.id = nomenclature.units_id " +
			"LEFT JOIN stock_income ON stock_income.id = stock_income_detail.stock_income_id ";
			PrepareTable.DisplayString = "Строка поступления на склад {0} в количестве {1} {2} от {3:d}";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys ("id");
			PrepareTable.DeleteItems.Add ("stock_expense_detail", 
				new TableInfo.DeleteDependenceItem ("WHERE stock_income_detail_id = @id ", "", "@id"));
			PrepareTable.DeleteItems.Add ("stock_write_off_detail", 
				new TableInfo.DeleteDependenceItem ("WHERE stock_income_detail_id = @id ", "", "@id"));
			QSMain.ProjectTables.Add ("stock_income_detail", PrepareTable);

			PrepareTable = new TableInfo ();
			PrepareTable.ObjectsName = "Строки расходного документа";
			PrepareTable.ObjectName = "строку расходного документа";
			PrepareTable.SqlSelect = "SELECT nomenclature.name, stock_expense_detail.quantity, units.name, stock_expense.date, stock_expense_detail.id " +
			"FROM stock_expense_detail " +
			"LEFT JOIN nomenclature ON nomenclature.id = stock_expense_detail.nomenclature_id " +
			"LEFT JOIN units ON units.id = nomenclature.units_id " +
			"LEFT JOIN stock_expense ON stock_expense.id = stock_expense_detail.stock_expense_id ";
			PrepareTable.DisplayString = "Строка выдачи со склада {0} в количестве {1} {2} от {3:d}";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys ("id");
			PrepareTable.DeleteItems.Add ("stock_income_detail", 
				new TableInfo.DeleteDependenceItem ("WHERE stock_expense_detail_id = @id ", "", "@id"));
			PrepareTable.DeleteItems.Add ("stock_write_off_detail", 
				new TableInfo.DeleteDependenceItem ("WHERE stock_expense_detail_id = @id ", "", "@id"));
			QSMain.ProjectTables.Add ("stock_expense_detail", PrepareTable);

			PrepareTable = new TableInfo ();
			PrepareTable.ObjectsName = "Строки акта списания";
			PrepareTable.ObjectName = "строку акта списания";
			PrepareTable.SqlSelect = "SELECT nomenclature.name, stock_write_off_detail.quantity, units.name, stock_write_off.date, stock_write_off_detail.id " +
			"FROM stock_write_off_detail " +
			"LEFT JOIN nomenclature ON nomenclature.id = stock_write_off_detail.nomenclature_id " +
			"LEFT JOIN units ON units.id = nomenclature.units_id " +
			"LEFT JOIN stock_write_off ON stock_write_off.id = stock_write_off_detail.stock_write_off_id ";
			PrepareTable.DisplayString = "Строка списания {0} в количестве {1} {2} от {3:d}";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys ("id");
			QSMain.ProjectTables.Add ("stock_write_off_detail", PrepareTable);
		}
	}
}
