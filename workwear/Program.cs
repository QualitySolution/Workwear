using System;
using System.Collections.Generic;
using Gtk;
using NLog;
using QSProjectsLib;

namespace workwear
{
	partial class MainClass
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		public static MainWindow MainWin;

		public static void Main (string[] args)
		{
			Application.Init ();
			QSMain.SubscribeToUnhadledExceptions ();
			QSMain.GuiThread = System.Threading.Thread.CurrentThread;
			QSSupportLib.MainSupport.Init ();
			CreateProjectParam ();
			// Создаем окно входа
			Login LoginDialog = new Login ();
			LoginDialog.Logo = Gdk.Pixbuf.LoadFromResource ("workwear.icon.logo.png");
			LoginDialog.SetDefaultNames ("workwear");
			LoginDialog.DefaultLogin = "demo";
			LoginDialog.DefaultServer = "demo.qsolution.ru";
			LoginDialog.DefaultConnection = "Демонстрационная база";
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
			QSSaaS.Session.StartSessionRefresh ();

			//Прописываем системную валюту
			CurrencyWorks.CurrencyShortFomat = "{0:C}";
			CurrencyWorks.CurrencyShortName = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;

			//Настройка базы
			CreateBaseConfig ();

			//Настрока удаления
			ConfigureDeletion ();

			//Запускаем программу
			MainWin = new MainWindow ();
			QSMain.ErrorDlgParrent = MainWin;
			if (QSMain.User.Login == "root")
				return;
			MainWin.Show ();
			Application.Run ();
			QSSaaS.Session.StopSessionRefresh ();
		}

		static void CreateProjectParam ()
		{
			// Создаем параметы пользователей
			QSMain.ProjectPermission = new Dictionary<string, UserPermission> ();
			//QSMain.ProjectPermission.Add ("edit_slips", new UserPermission("edit_slips", "Изменение кассы задним числом",
			//                                                             "Пользователь может изменять или добавлять кассовые документы задним числом."));

			//Настраиваем обновления
			QSUpdater.DB.DBUpdater.AddMicroUpdate (
				new Version (1, 0),
				new Version (1, 0, 4),
				"workwear.Updates.1.0.4.sql");
			QSUpdater.DB.DBUpdater.AddMicroUpdate (
				new Version (1, 0, 4),
				new Version (1, 0, 5),
				"workwear.Updates.1.0.5.sql");
			QSUpdater.DB.DBUpdater.AddUpdate (
				new Version (1, 0),
				new Version (1, 1),
				"workwear.Updates.Update to 1.1.sql");
			QSUpdater.DB.DBUpdater.AddUpdate (
				new Version (1, 1),
				new Version (1, 2),
				"workwear.Updates.Update to 1.2.sql");			
			
		}
	}
}
