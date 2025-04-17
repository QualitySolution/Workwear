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
				StartupContainer = builder.Build();
				unhandledExceptionHandler.UpdateDependencies(StartupContainer);
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
			
			ILifetimeScope scopeLoginTime = StartupContainer.BeginLifetimeScope();
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
					"Демонстрационная Спецаутсорсинг",
					"sps\\demo",
					user: "demo",
					account: "demo"
				),
			};

			LoginDialog.UpdateFromGConf ();

			ResponseType LoginResult;
			LoginResult = (ResponseType)LoginDialog.Run ();
			if (LoginResult == ResponseType.DeleteEvent || LoginResult == ResponseType.Cancel)
				return;

			bool isDemo = LoginDialog.ConnectedTo.IsDemo;
			string baseName = LoginDialog.SelectedConnection;
			LoginDialog.Destroy ();
			scopeLoginTime.Dispose();

			//Запускаем программу
			Application.Invoke(delegate 
			{
				MainWin = new MainWindow (unhandledExceptionHandler, isDemo);
				MainWin.Title += $" (БД: {baseName})";
				if (QSMain.User.Login == "root")
					return;
				MainWin.Show ();
			});
			Application.Run ();
			
			if (!MainTelemetry.SendingError)
			{
				MainTelemetry.SendTimeout = TimeSpan.FromSeconds(2);
				MainTelemetry.SendTelemetry();
			}
			QSSaaS.Session.StopSessionRefresh ();
			AppDIContainer.Dispose();
			StartupContainer.Dispose();
		}
	}
}
