using System;
using System.Collections.Generic;
using Gtk;
using QSProjectsLib;

namespace workwear
{
	class MainClass
	{
		public static Label StatusBarLabel;
		public static MainWindow MainWin;
		
		public static void Main(string[] args)
		{
			Application.Init();
			AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e) 
			{
				QSMain.ErrorMessage(MainWin, (Exception) e.ExceptionObject);
			};
			CreateProjectParam();
			//Настраиваем общую билиотеку
			QSMain.NewStatusText += delegate(object sender, QSProjectsLib.QSMain.NewStatusTextEventArgs e) 
			{
				StatusMessage (e.NewText);
			};
			// Создаем окно входа
			Login LoginDialog = new QSProjectsLib.Login ();
			LoginDialog.Logo = Gdk.Pixbuf.LoadFromResource ("workwear.icon.logo.png");
			LoginDialog.SetDefaultNames ("workwear");
			LoginDialog.DefaultLogin = "admin";
			LoginDialog.DefaultServer = "localhost";
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
			LoginResult = (ResponseType) LoginDialog.Run();
			if (LoginResult == ResponseType.DeleteEvent || LoginResult == ResponseType.Cancel)
				return;

			LoginDialog.Destroy ();

			//Запускаем программу
			MainWin = new MainWindow ();
			if(QSMain.User.Login == "root")
				return;
			MainWin.Show ();
			Application.Run ();
		}

		static void CreateProjectParam()
		{
			QSMain.AdminFieldName = "admin";
			QSMain.ProjectPermission = new Dictionary<string, UserPermission>();
			//QSMain.ProjectPermission.Add ("edit_slips", new UserPermission("edit_slips", "Изменение кассы задним числом",
			//                                                             "Пользователь может изменять или добавлять кассовые документы задним числом."));

			QSMain.User = new UserInfo();
		}
		
		public static void StatusMessage(string message)
		{
			StatusBarLabel.Text = message;
			Console.WriteLine (message);
			while (GLib.MainContext.Pending())
			{
   				Gtk.Main.Iteration();
			}
		}

	}
}
