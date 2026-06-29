using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Gtk;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using NLog;
using QS.Dialog;
using QS.ErrorReporting;
using QS.Launcher;
using QS.Launcher.AppRunner;
using QS.Project.DB;
using QSProjectsLib;
using QSTelemetry;
using Workwear.Startup;

namespace workwear
{
	static partial class MainClass
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		public static MainWindow MainWin;

		[STAThread]
		public static void Main (string[] args)
		{
			string connectionString = Environment.GetEnvironmentVariable("QS_CONNECTION_STRING");
			string login = Environment.GetEnvironmentVariable("QS_LOGIN");
			string sessionId = Environment.GetEnvironmentVariable("QS_SessionId");
			string baseTitle = Environment.GetEnvironmentVariable("QS_BaseTitle");

			// Clear env vars after using for a little bit of security.
			// Process-level vars can't be cleared for the parent from here anyway.
			Environment.SetEnvironmentVariable("QS_CONNECTION_STRING", null, EnvironmentVariableTarget.Process);
			Environment.SetEnvironmentVariable("QS_LOGIN", null, EnvironmentVariableTarget.Process);
			Environment.SetEnvironmentVariable("QS_SessionId", null, EnvironmentVariableTarget.Process);
			Environment.SetEnvironmentVariable("QS_BaseTitle", null, EnvironmentVariableTarget.Process);

			UnhandledExceptionHandler unhandledExceptionHandler = new UnhandledExceptionHandler();
			
			try
			{
				WindowStartupFix.WindowsCheck();
				Application.Init();
				QSMain.GuiThread = System.Threading.Thread.CurrentThread;
				GtkGuiDispatcher.GuiThread = System.Threading.Thread.CurrentThread;

				bool startLauncher = String.IsNullOrEmpty(connectionString);
				var builder = new ContainerBuilder();
				AutofacStartupConfig(builder);
				if(startLauncher) {
					logger.Info("Параметры подключения не переданы. Настраиваем GTK лаунчер.");
					builder.UseInProcessRunner();
					var launcherServices = new ServiceCollection();
					launcherServices
						.AddWorkwearLauncherConfiguration(options => {
							// В InProcess режиме лаунчер не является standalone приложением.
							options.IsStandalone = false;
						})
						.AddLauncherDependencies()
						.AddLauncherViewModels()
						.AddLauncherViews();
					builder.Populate(launcherServices);
				}

				StartupContainer = builder.Build();
				unhandledExceptionHandler.UpdateDependencies(StartupContainer);
				unhandledExceptionHandler.SubscribeToUnhandledExceptions();

				if(startLauncher) {
					logger.Info("Запуск GTK лаунчера.");
					using(var launcherScope = StartupContainer.BeginLifetimeScope()) {
						var runner = launcherScope.Resolve<InProcessRunner>();
						runner.OnLogin = response => {
							login = response.Login;
							sessionId = response.Parameters.ContainsKey("SessionId")
								? response.Parameters["SessionId"]
								: null;
							baseTitle = response.Parameters.ContainsKey("BaseTitle")
								? response.Parameters["BaseTitle"]
								: null;
							connectionString = response.ConnectionString;
							//Строка соединения обязательно заполняется последней так как по ее заполненности проверяем нужно ли выходить из ожидания.
						};

						var loginView = launcherScope.Resolve<QS.Launcher.Views.MainWindow>();
						loginView.DeleteEvent += (o, a) => {
							Environment.Exit(0);
						};
						loginView.Show();

						var gui = launcherScope.Resolve<IGuiDispatcher>();
						gui.WaitInMainLoop(() => !String.IsNullOrEmpty(connectionString));
						loginView.Destroy();
					}
				}
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

			if(String.IsNullOrEmpty(connectionString))
				return;

			MySqlConnectionStringBuilder connectionStringBuilder = new MySqlConnectionStringBuilder(connectionString);
			IDatabaseConnectionSettings databaseConnectionSettings = new DatabaseConnectionSettings(connectionStringBuilder);

			//Запускаем программу
			Application.Invoke(delegate 
			{
				MainWin = new MainWindow();
				MainWin.Show ();
				MainWin.StartApplication(unhandledExceptionHandler, databaseConnectionSettings, login, sessionId, baseTitle);
			});
			Application.Run ();
			
			if (!MainTelemetry.SendingError)
			{
				MainTelemetry.SendTimeout = TimeSpan.FromSeconds(2);
				MainTelemetry.SendTelemetry();
			}
			AppDIContainer.Dispose();
			StartupContainer.Dispose();
		}
	}
}
