using System;
using System.Collections.Generic;
using Gtk;
using NLog;
using QS.Updater.DB;
using QSMachineConfig;
using QSProjectsLib;
using QSSupportLib;
using QSTelemetry;

namespace workwear
{
	partial class MainClass
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		public static MainWindow MainWin;

		[STAThread]
		public static void Main (string[] args)
		{
			try
			{
				WindowStartupFix.WindowsCheck();
				Application.Init();
				QSMain.SubscribeToUnhadledExceptions();
				QSMain.GuiThread = System.Threading.Thread.CurrentThread;
				MainSupport.Init();
			}
			catch (Exception falalEx)
			{
				if (WindowStartupFix.IsWindows)
					WindowStartupFix.DisplayWindowsOkMessage(falalEx.ToString(), "Критическая ошибка");
				else
					Console.WriteLine(falalEx);

				logger.Fatal(falalEx);
				return;
			}

			CreateProjectParam ();
			// Создаем окно входа
			Login LoginDialog = new Login ();
			LoginDialog.Logo = Gdk.Pixbuf.LoadFromResource ("workwear.icon.logo.png");
			LoginDialog.SetDefaultNames ("workwear");
			LoginDialog.DefaultLogin = "demo";
			LoginDialog.DefaultServer = "demo.qsolution.ru";
			LoginDialog.DefaultConnection = "Демонстрационная база";
            Login.ApplicationDemoServer = "demo.qsolution.ru";
			LoginDialog.DemoMessage = "Для подключения к демострационному серверу используйте следующие настройки:\n" +
			"\n" +
			"<b>Сервер:</b> demo.qsolution.ru\n" +
			"<b>Пользователь:</b> demo\n" +
			"<b>Пароль:</b> demo\n" +
			"\n" +
			"Для установки собственного сервера обратитесь к документации.";
			Login.CreateDBHelpTooltip = "Инструкция по установке сервера MySQL";
			Login.CreateDBHelpUrl = "http://workwear.qsolution.ru/?page_id=168&utm_source=qs&utm_medium=app_workwear&utm_campaign=connection_editor";

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
			Configure.ConfigureDeletion ();

            //Иницициализируем телеметрию
            MainTelemetry.Product = MainSupport.ProjectVerion.Product;
            MainTelemetry.Edition = MainSupport.ProjectVerion.Edition;
            MainTelemetry.Version = MainSupport.ProjectVerion.Version.ToString();
            MainTelemetry.IsDemo = Login.ApplicationDemoServer == QSMain.connectionDB.DataSource;
			var appConfig = MachineConfig.ConfigSource.Configs["Application"];
			if (appConfig != null)
				MainTelemetry.DoNotTrack = appConfig.GetBoolean("DoNotTrack", false);

			MainTelemetry.StartUpdateByTimer(600);

			//Запускаем программу
			MainWin = new MainWindow ();
			QSMain.ErrorDlgParrent = MainWin;
			if (QSMain.User.Login == "root")
				return;
			MainWin.Show ();
			Application.Run ();

			if (!MainTelemetry.SendingError)
			{
				MainTelemetry.SendTimeout = TimeSpan.FromSeconds(2);
				MainTelemetry.SendTelemetry();
			}
			QSSaaS.Session.StopSessionRefresh ();
		}

		static void CreateProjectParam ()
		{
			// Создаем параметы пользователей
			QSMain.ProjectPermission = new Dictionary<string, UserPermission> ();
			//QSMain.ProjectPermission.Add ("edit_slips", new UserPermission("edit_slips", "Изменение кассы задним числом",
			//                                                             "Пользователь может изменять или добавлять кассовые документы задним числом."));

			//Скрипты создания базы
			DBCreator.AddBaseScript (
				new Version(2, 1),
				"Чистая база",
				"workwear.Updates.new_2.1.sql"
			);

			//Настраиваем обновления
			DBUpdater.AddMicroUpdate (
				new Version (1, 0),
				new Version (1, 0, 4),
				"workwear.Updates.1.0.4.sql");
			DBUpdater.AddMicroUpdate (
				new Version (1, 0, 4),
				new Version (1, 0, 5),
				"workwear.Updates.1.0.5.sql");
			DBUpdater.AddUpdate (
				new Version (1, 0),
				new Version (1, 1),
				"workwear.Updates.Update to 1.1.sql");
			DBUpdater.AddUpdate (
				new Version (1, 1),
				new Version (1, 2),
				"workwear.Updates.Update to 1.2.sql");			
			DBUpdater.AddMicroUpdate (
				new Version (1, 2),
				new Version (1, 2, 1),
				"workwear.Updates.1.2.1.sql");
			DBUpdater.AddMicroUpdate (
				new Version (1, 2, 1),
				new Version (1, 2, 2),
				"workwear.Updates.1.2.2.sql");
			DBUpdater.AddMicroUpdate (
				new Version (1, 2, 2),
				new Version (1, 2, 4),
				"workwear.Updates.1.2.4.sql");
			DBUpdater.AddUpdate (
				new Version (1, 2),
				new Version (2, 0),
				"workwear.Updates.2.0.sql");
			DBUpdater.AddMicroUpdate(
				new Version(2, 0),
				new Version(2, 0, 2),
				"workwear.Updates.2.0.2.sql");
			DBUpdater.AddUpdate(
				new Version(2, 0),
				new Version(2, 1),
				"workwear.Updates.2.1.sql");

		}
	}
}
