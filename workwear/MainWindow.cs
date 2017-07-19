using System;
using System.Collections.Generic;
using Gtk;
using NLog;
using QSBusinessCommon.Domain;
using QSOrmProject;
using QSProjectsLib;
using QSSupportLib;
using QSTDI;
using QSTelemetry;
using QSUpdater;
using workwear;
using workwear.Domain;
using workwear.Domain.Stock;
using workwear.JournalViewers;
using workwear.ViewModel;

public partial class MainWindow : Gtk.Window
{
	private static Logger logger = LogManager.GetCurrentClassLogger();

	public MainWindow() : base(Gtk.WindowType.Toplevel)
	{
		Build();

		//Передаем лебл
		QSMain.StatusBarLabel = labelStatus;
		this.Title = MainSupport.GetTitle();
		QSMain.MakeNewStatusTargetForNlog();

		QSMain.CheckServer(this); // Проверяем настройки сервера
		MainSupport.LoadBaseParameters();

		MainUpdater.RunCheckVersion(true, true, true);

		if (QSMain.User.Login == "root")
		{
			string Message = "Вы зашли в программу под администратором базы данных. У вас есть только возможность создавать других пользователей.";
			MessageDialog md = new MessageDialog(this, DialogFlags.DestroyWithParent,
												  MessageType.Info,
												  ButtonsType.Ok,
												  Message);
			md.Run();
			md.Destroy();
			Users WinUser = new Users();
			WinUser.Show();
			WinUser.Run();
			WinUser.Destroy();
			return;
		}

		if (QSMain.connectionDB.DataSource == "demo.qsolution.ru")
		{
			string Message = "Вы подключились к демонстрационному серверу. Сервер предназначен для оценки " +
				"возможностей программы, не используйте его для работы, так как ваши данные будут доступны " +
				"любому пользователю через интернет.\n\nДля полноценного использования программы вам необходимо " +
				"установить собственный сервер. Для его установки обратитесь к документации.\n\nЕсли у вас возникнут " +
				"вопросы вы можете обратится в нашу тех. поддержку.";
			MessageDialog md = new MessageDialog(this, DialogFlags.DestroyWithParent,
												  MessageType.Info,
												  ButtonsType.Ok,
												  Message);
			md.Run();
			md.Destroy();
			dialogAuthenticationAction.Sensitive = false;
		}

		UsersAction.Sensitive = QSMain.User.Admin;
		labelUser.LabelProp = QSMain.User.Name;

		//Настраиваем новости
		MainNewsFeed.NewsFeeds = new List<NewsFeed>(){
			new NewsFeed("workwearnews", "Новости программы", "http://news.qsolution.ru/workwear.atom")
			};
		MainNewsFeed.LoadReadFeed();
		var newsmenu = new NewsMenuItem();
		menubar1.Add(newsmenu);
		newsmenu.LoadFeed();
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		a.RetVal = true;
		Application.Quit();
	}

	protected void OnDialogAuthenticationActionActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("ChangeUserPassword");
		QSMain.User.ChangeUserPassword(this);
	}

	protected void OnUsersActionActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("OpenUsers");
		Users winUsers = new Users();
		winUsers.Show();
		winUsers.Run();
		winUsers.Destroy();
	}

	protected void OnQuitActionActivated(object sender, EventArgs e)
	{
		Application.Quit();
	}

	protected void OnAction7Activated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("MeasurementUnits");
		var refWin = new OrmReference(typeof(MeasurementUnits));
		var dialog = new OneWidgetDialog(refWin);
		dialog.Show();
		dialog.Run();
		dialog.Destroy();
	}

	protected void OnAction8Activated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("Post");
		var refWin = new OrmReference(typeof(Post));
		var dialog = new OneWidgetDialog(refWin);
		dialog.Show();
		dialog.Run();
		dialog.Destroy();
	}

	protected void OnAction9Activated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("Leader");
		var refWin = new OrmReference(typeof(Leader));
		var dialog = new OneWidgetDialog(refWin);
		dialog.Show();
		dialog.Run();
		dialog.Destroy();
	}

	protected void OnAction5Activated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("ItemsType");
		var refWin = new OrmReference(typeof(ItemsType));
		var dialog = new OneWidgetDialog(refWin);
		dialog.Show();
		dialog.Run();
		dialog.Destroy();
	}

	protected void OnAction6Activated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("Nomenclature");
		var refWin = new OrmReference(typeof(Nomenclature));
		var dialog = new OneWidgetDialog(refWin);
		dialog.Show();
		dialog.Run();
		dialog.Destroy();
	}

	protected void OnAboutActionActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("RunAboutDialog");
		QSMain.RunAboutDialog();
	}

	protected void OnAction11Activated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("ReportStockAllWear");
		ViewReportExt.Run("StockAllWear", "");
	}

	protected void OnAction10Activated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("ReportWearStatement");
		WearStatement WinStat = new WearStatement();
		WinStat.Show();
		WinStat.Run();
		WinStat.Destroy();
	}

	protected void OnAction12Activated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("ReportListBySize");
		ViewReportExt.Run("ListBySize", "");
	}

	protected void OnHelpActionActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("OpenDocumentation");
		System.Diagnostics.Process.Start("workwear_ru.pdf");
	}

	protected void OnActionHistoryActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("RunChangeLogDlg");
		QSMain.RunChangeLogDlg(this);
	}

	protected void OnActionUpdateActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("CheckUpdate");
		CheckUpdate.StartCheckUpdateThread(UpdaterFlags.ShowAnyway);
	}

	protected void OnActionSNActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("EditSerialNumber");
		EditSerialNumber.RunDialog();
	}

	protected void OnActionNormsActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("OpenNorms");
		var refWin = new ReferenceRepresentation(new workwear.ViewModel.NormVM());
		var dialog = new OneWidgetDialog(refWin);
		dialog.Show();
		dialog.Run();
		dialog.Destroy();
	}

	protected void OnAction13Activated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("ReportMonthIssueSheet");
		var dlg = new OnIssueStatement();
		dlg.Show();
		dlg.Run();
		dlg.Destroy();
	}

	protected void OnAction17Activated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("ReportNotIssuedSheet");
		var dlg = new NotIssuedSheetReportDlg();
		dlg.Show();
		dlg.Run();
		dlg.Destroy();
	}

	protected void OnActionYearRequestSheetActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("ReportYearRequestSheet");
		ViewReportExt.Run("YearRequestSheet", "");
	}

	protected void OnAction21Activated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("ReportMonthQuarterRequestSheet");
		var dlg = new QuarterRequestSheetDlg();
		dlg.Show();
		dlg.Run();
		dlg.Destroy();
	}

	protected void OnActionStockBalanceActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(TdiTabBase.GenerateHashName<StockBalanceView>(),
						() => new StockBalanceView()
					   );
	}

	protected void OnActionStockDocsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(TdiTabBase.GenerateHashName<StockDocumentsView>(),
				() => new StockDocumentsView()
			   );
	}

	protected void OnActionEmployeesActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			ReferenceRepresentation.GenerateHashName<EmployeesVM>(),
			() => new ReferenceRepresentation(new EmployeesVM())
		);
	}

	protected void OnActionObjectsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<Facility>(),
			() => new OrmReference(typeof(Facility))
		);
	}
}
