using System;
using Autofac;
using Gtk;
using NLog;
using QS.Configuration;
using QS.DBScripts.Controllers;
using QS.Dialog;
using QS.ErrorReporting;
using QS.Project.Versioning;
using QSProjectsLib;
using QSTelemetry;
using Workwear;
using Workwear.Tools;

namespace workwear
{
	static partial class MainClass
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		public static MainWindow MainWin;

		[STAThread]
		public static void Main (string[] args)
		{
			UnhandledExceptionHandler unhandledExceptionHandler = new UnhandledExceptionHandler();
			
			try
			{
				WindowStartupFix.WindowsCheck();
				Application.Init();
				QSMain.GuiThread = System.Threading.Thread.CurrentThread;
				GtkGuiDispatcher.GuiThread = System.Threading.Thread.CurrentThread;
				
				var builder = new ContainerBuilder();
				AutofacStartupConfig(builder);
				startupContainer = builder.Build();
				unhandledExceptionHandler.UpdateDependencies(startupContainer);
				unhandledExceptionHandler.SubscribeToUnhandledExceptions();

			} catch(MissingMethodException ex) when (ex.Message.Contains("System.String System.String.Format")) {
				WindowStartupFix.DisplayWindowsOkMessage("Версия .Net Framework должна быть не ниже 4.6.1. Установите более новую платформу.", "Старая версия .Net");
				return;
			}
			catch (Exception fallEx) {
				if (WindowStartupFix.IsWindows)
					WindowStartupFix.DisplayWindowsOkMessage(fallEx.ToString(), "Критическая ошибка");
				else
					Console.WriteLine(fallEx);

				logger.Fatal(fallEx);
				return;
			}
			
			ILifetimeScope scopeLoginTime = startupContainer.BeginLifetimeScope();
			var configuration = scopeLoginTime.Resolve<IChangeableConfiguration>();
			// Создаем окно входа
			Login LoginDialog = new Login (configuration);
			LoginDialog.Logo = Gdk.Pixbuf.LoadFromResource ("Workwear.icon.logo.png");
			Login.ApplicationDemoServer = "demo.qsolution.ru";
			Login.ApplicationDemoAccount = "demo";
			LoginDialog.DemoMessage = "Для входа в демонстрационную базу используйте следующие данные:\n" +
			"\n" +
			"<b>Пользователь:</b> demo\n" +
			"<b>Пароль:</b> demo\n" +
			"\n" +
			"Для установки собственного сервера обратитесь к документации.";
			Login.CreateDBHelpTooltip = "Инструкция по установке сервера MySQL";
			Login.CreateDBHelpUrl = "https://doc.qsolution.ru/workwear/" + new ApplicationVersionInfo().Version.ToString(2) +"/install.html#InstallDBServer";
			LoginDialog.GetDBCreator = scopeLoginTime.Resolve<IDBCreator>;
			Login.MakeDefaultConnections = () => new Connection[] {
				new Connection(
					ConnectionType.SaaS,
					"Демонстрационная(текущая)",
					"current",
					user: "demo",
					account: "demo"
				),
				new Connection(
					ConnectionType.SaaS,
					"Демонстрационная(стабильная)",
					"stable",
					user: "demo",
					account: "demo"
				)
			};

			LoginDialog.UpdateFromGConf ();

			ResponseType LoginResult;
			LoginResult = (ResponseType)LoginDialog.Run ();
			if (LoginResult == ResponseType.DeleteEvent || LoginResult == ResponseType.Cancel)
				return;

			bool isDemo = LoginDialog.ConnectedTo.IsDemo;
			LoginDialog.Destroy ();
			scopeLoginTime.Dispose();

			QSSaaS.Session.StartSessionRefresh ();

			//Прописываем системную валюту
			CurrencyWorks.CurrencyShortFomat = "{0:C}";
			CurrencyWorks.CurrencyShortName = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
			
			CreateBaseConfig (); //Настройка базы
			AppDIContainer = startupContainer.BeginLifetimeScope(c => AutofacClassConfig(c, isDemo)); //Создаем постоянный контейнер
			unhandledExceptionHandler.UpdateDependencies(AppDIContainer);
			BusinessLogicGlobalEventHandler.Init(AppDIContainer);

			//Настройка удаления
			Configure.ConfigureDeletion();
#if !DEBUG
			//Инициализируем телеметрию
			var applicationInfo = new ApplicationVersionInfo();
			MainTelemetry.Product = applicationInfo.ProductName;
            MainTelemetry.Edition = applicationInfo.Modification;
            MainTelemetry.Version = applicationInfo.Version.ToString();
            MainTelemetry.IsDemo = Login.ApplicationDemoServer == QSMain.connectionDB.DataSource;
            MainTelemetry.DoNotTrack = configuration["Application:DoNotTrack"] == "true";
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
			AppDIContainer.Dispose();
			startupContainer.Dispose();
		}
	}
}
