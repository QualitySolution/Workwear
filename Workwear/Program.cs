using System;
using Gtk;
using NLog;
using QS.ErrorReporting;
using QS.Project.DB;
using QS.Project.Repositories;
using QS.Updater;
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
				QSMain.GuiThread = System.Threading.Thread.CurrentThread;
				#if DEBUG
				var errorSettings = new ErrorReportingSettings(false, true, false, null);
				#else
				var errorSettings = new ErrorReportingSettings(true, false, true, 300);
				#endif
				UnhandledExceptionHandler.SubscribeToUnhadledExceptions(errorSettings);
				UnhandledExceptionHandler.GuiThread = System.Threading.Thread.CurrentThread;
				UnhandledExceptionHandler.ApplicationInfo = new ApplicationVersionInfo();
				//Настройка обычных обработчиков ошибок.
				UnhandledExceptionHandler.CustomErrorHandlers.Add(CommonErrorHandlers.MySqlException1055OnlyFullGroupBy);
				UnhandledExceptionHandler.CustomErrorHandlers.Add(CommonErrorHandlers.MySqlException1366IncorrectStringValue);
				UnhandledExceptionHandler.CustomErrorHandlers.Add(CommonErrorHandlers.NHibernateFlushAfterException);

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

			RegisterSQLScripts ();
			AutofacClassConfig();

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
			UnhandledExceptionHandler.DataBaseInfo = new NhDataBaseInfo();
			using(var uow = QS.DomainModel.UoW.UnitOfWorkFactory.CreateWithoutRoot()) {
				UnhandledExceptionHandler.User = UserRepository.GetCurrentUser(uow);
			}

			//Настрока удаления
			Configure.ConfigureDeletion();
#if !DEBUG
			//Иницициализируем телеметрию
			MainTelemetry.Product = MainSupport.ProjectVerion.Product;
            MainTelemetry.Edition = MainSupport.ProjectVerion.Edition;
            MainTelemetry.Version = MainSupport.ProjectVerion.Version.ToString();
            MainTelemetry.IsDemo = Login.ApplicationDemoServer == QSMain.connectionDB.DataSource;
			var appConfig = MachineConfig.ConfigSource.Configs["Application"];
			if (appConfig != null)
				MainTelemetry.DoNotTrack = appConfig.GetBoolean("DoNotTrack", false);

			MainTelemetry.StartUpdateByTimer(600);
#else
			MainTelemetry.DoNotTrack = true;
#endif
			//Запускаем программу
			MainWin = new MainWindow ();
			MainWin.Title += string.Format(" (БД: {0})", LoginDialog.SelectedConnection);
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

		static void RegisterSQLScripts ()
		{
			//Скрипты создания базы
			DBCreator.AddBaseScript (
				new Version(2, 3),
				"Чистая база",
				"workwear.Updates.new_empty.sql"
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
			DBUpdater.AddMicroUpdate(
				new Version(2, 1),
				new Version(2, 1, 1),
				"workwear.Updates.2.1.1.sql");
			DBUpdater.AddUpdate(
				new Version(2, 1),
				new Version(2, 2),
				"workwear.Updates.2.2.sql");
			DBUpdater.AddUpdate(
				new Version(2, 2),
				new Version(2, 3),
				"workwear.Updates.2.3.sql");
			DBUpdater.AddMicroUpdate(
				new Version(2, 3),
				new Version(2, 3, 3),
				"workwear.Updates.2.3.3.sql");

		}
	}
}
