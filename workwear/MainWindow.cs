using System;
using System.Collections.Generic;
using Gtk;
using NLog;
using QSBusinessCommon.Domain;
using QSOrmProject;
using QSProjectsLib;
using QSSupportLib;
using QSUpdater;
using workwear;
using workwear.Domain;
using workwear.Domain.Stock;

public partial class MainWindow: Gtk.Window
{	
	private static Logger logger = LogManager.GetCurrentClassLogger();

	public MainWindow(): base (Gtk.WindowType.Toplevel)
	{
		Build();
		
		//Передаем лебл
		QSMain.StatusBarLabel = labelStatus;
		this.Title = MainSupport.GetTitle();
		QSMain.MakeNewStatusTargetForNlog();

		MainSupport.LoadBaseParameters ();

		MainUpdater.RunCheckVersion (true, true, true);

		if(QSMain.User.Login == "root")
		{
			string Message = "Вы зашли в программу под администратором базы данных. У вас есть только возможность создавать других пользователей.";
			MessageDialog md = new MessageDialog ( this, DialogFlags.DestroyWithParent,
			                                      MessageType.Info, 
			                                      ButtonsType.Ok,
			                                      Message);
			md.Run ();
			md.Destroy();
			Users WinUser = new Users();
			WinUser.Show();
			WinUser.Run ();
			WinUser.Destroy ();
			return;
		}

		if(QSMain.connectionDB.DataSource == "demo.qsolution.ru")
		{
			string Message = "Вы подключились к демонстрационному серверу. Сервер предназначен для оценки " +
				"возможностей программы, не используйте его для работы, так как ваши данные будут доступны " +
				"любому пользователю через интернет.\n\nДля полноценного использования программы вам необходимо " +
				"установить собственный сервер. Для его установки обратитесь к документации.\n\nЕсли у вас возникнут " +
				"вопросы вы можете обратится в нашу тех. поддержку.";
			MessageDialog md = new MessageDialog ( this, DialogFlags.DestroyWithParent,
			                                      MessageType.Info, 
			                                      ButtonsType.Ok,
			                                      Message);
			md.Run ();
			md.Destroy();
			dialogAuthenticationAction.Sensitive = false;
		}

		UsersAction.Sensitive = QSMain.User.Admin;
		labelUser.LabelProp = QSMain.User.Name;

		//Настраиваем новости
		MainNewsFeed.NewsFeeds = new List<NewsFeed>(){
			new NewsFeed("workwearnews", "Новости программы", "http://news.qsolution.ru/workwear.atom")
			};
		MainNewsFeed.LoadReadFeed ();
		var newsmenu = new NewsMenuItem ();
		menubar1.Add (newsmenu);
		newsmenu.LoadFeed ();

		PrepareObject();
		PrepareCards();
		PrepareStock();
		UpdateObject();
		notebookMain.CurrentPage = 0;
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		a.RetVal = true;
		Application.Quit();
	}
	
	protected void OnDialogAuthenticationActionActivated(object sender, EventArgs e)
	{
		QSMain.User.ChangeUserPassword (this);
	}
	
	protected void OnUsersActionActivated(object sender, EventArgs e)
	{
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
		var refWin = new OrmReference (typeof(MeasurementUnits));
		var dialog = new OneWidgetDialog (refWin);
		dialog.Show ();
		dialog.Run ();
		dialog.Destroy ();
	}

	protected void OnAction8Activated(object sender, EventArgs e)
	{
		var refWin = new OrmReference (typeof(Post));
		var dialog = new OneWidgetDialog (refWin);
		dialog.Show ();
		dialog.Run ();
		dialog.Destroy ();
	}
	
	protected void OnAction9Activated(object sender, EventArgs e)
	{
		var refWin = new OrmReference (typeof(Leader));
		var dialog = new OneWidgetDialog (refWin);
		dialog.Show ();
		dialog.Run ();
		dialog.Destroy ();
	}
	
	protected void OnAction5Activated(object sender, EventArgs e)
	{
		var refWin = new OrmReference (typeof(ItemsType));
		var dialog = new OneWidgetDialog (refWin);
		dialog.Show ();
		dialog.Run ();
		dialog.Destroy ();
	}

	protected void OnAction6Activated(object sender, EventArgs e)
	{
		var refWin = new OrmReference (typeof(Nomenclature));
		var dialog = new OneWidgetDialog (refWin);
		dialog.Show ();
		dialog.Run ();
		dialog.Destroy ();
	}

	protected void OnAboutActionActivated(object sender, EventArgs e)
	{
		QSMain.RunAboutDialog();
	}
	
	protected void OnButtonRefreshClicked(object sender, EventArgs e)
	{
		switch (notebookMain.CurrentPage) {
			case 0:
				UpdateObject();
				break;
				case 1:
				UpdateCards();
				break;
				case 2:
				UpdateStock();
				break;
		case 3:
			stockbalanceview1.RefreshView ();
			break;
		}
	}

	protected void OnButtonAddClicked(object sender, EventArgs e)
	{
		switch (notebookMain.CurrentPage) {
			case 0:
				ObjectDlg winObject = new ObjectDlg();
				winObject.NewItem = true;
				winObject.Show();
				winObject.Run();
				winObject.Destroy();
				UpdateObject();
				break;
			case 1:
				EmployeeCardDlg winWearCard = new EmployeeCardDlg();
				winWearCard.Show();
				winWearCard.Run();
				winWearCard.Destroy();
				UpdateCards();
				break;
				case 2:
				switch (notebookStock.CurrentPage)
				{
					case 0:
						IncomeDocDlg winIncome = new IncomeDocDlg();
						winIncome.Show();
						winIncome.Run();
						winIncome.Destroy();
						break;
					case 1:
						ExpenseDocDlg winExpense = new ExpenseDocDlg();
						winExpense.Show();
						winExpense.Run();
						winExpense.Destroy();
						break;
					case 2:
						WriteOffDocDlg winWriteOff = new WriteOffDocDlg();
						winWriteOff.Show();
						winWriteOff.Run();
						winWriteOff.Destroy();
						break;
				}
				UpdateStock();
				break;
		}

	}

	protected void OnButtonEditClicked(object sender, EventArgs e)
	{
		TreeIter iter;
		int itemid;
		ResponseType result;

		switch (notebookMain.CurrentPage) {
			case 0:
				treeviewObjects.Selection.GetSelected(out iter);
				itemid = (int) ObjectFilter.GetValue(iter,0);
				ObjectDlg winObject = new ObjectDlg();
				winObject.Fill(itemid);
				winObject.Show();
				result = (ResponseType)winObject.Run();
				winObject.Destroy();
				if(result == ResponseType.Ok)
					UpdateObject();
				break;
			case 1:
				treeviewCards.Selection.GetSelected(out iter);
				itemid = (int) CardsFilter.GetValue(iter,0);
				EmployeeCardDlg winWearCadr = new EmployeeCardDlg(itemid);
				winWearCadr.Show();
				result = (ResponseType)winWearCadr.Run();
				winWearCadr.Destroy();
				if(result == ResponseType.Ok)
					UpdateCards();
				break;
				case 2:
				switch (notebookStock.CurrentPage)
				{
					case 0:
						treeviewIncome.Selection.GetSelected(out iter);
						itemid = (int)IncomeFilter.GetValue(iter, 0);
						IncomeDocDlg winIncome = new IncomeDocDlg(itemid);
						winIncome.Show();
						result = (ResponseType) winIncome.Run();
						winIncome.Destroy();
						break;
					case 1:
						treeviewExpense.Selection.GetSelected(out iter);
						itemid = (int)ExpenseFilter.GetValue(iter, 0);
						ExpenseDocDlg winExpense = new ExpenseDocDlg(itemid);
						winExpense.Show();
						result = (ResponseType) winExpense.Run();
						winExpense.Destroy();
						break;
					case 2:
						treeviewWriteOff.Selection.GetSelected(out iter);
						itemid = (int)WriteOffFilter.GetValue(iter, 0);
						WriteOffDocDlg winWriteOff = new WriteOffDocDlg(itemid);
						winWriteOff.Show();
						result = (ResponseType) winWriteOff.Run();
						winWriteOff.Destroy();
						break;
					default:
						result = ResponseType.Reject;
						break;
				}
				if(result == ResponseType.Ok)
					UpdateStock();
				break;
		}

	}

	protected void OnButtonDeleteClicked(object sender, EventArgs e)
	{
		// Удаление
		TreeIter iter;
		int itemid;

		switch (notebookMain.CurrentPage) 
		{
			case 0:
				treeviewObjects.Selection.GetSelected(out iter);
				itemid = (int) ObjectFilter.GetValue(iter,0);
				if(OrmMain.DeleteObject<Facility> (itemid))
					UpdateObject();
				break;
			case 1:
				treeviewCards.Selection.GetSelected(out iter);
				itemid = (int) CardsFilter.GetValue(iter,0);
				if(OrmMain.DeleteObject<EmployeeCard> (itemid))
					UpdateCards();
				break;
			case 2:
				switch (notebookStock.CurrentPage)
				{
					case 0:
						treeviewIncome.Selection.GetSelected (out iter);
						itemid = (int)IncomeFilter.GetValue (iter, 0);
						OrmMain.DeleteObject<Income> (itemid);
						break;
					case 1:
						treeviewExpense.Selection.GetSelected(out iter);
						itemid = (int)ExpenseFilter.GetValue(iter, 0);
						OrmMain.DeleteObject<Expense> (itemid);
						break;
					case 2:
						treeviewWriteOff.Selection.GetSelected(out iter);
						itemid = (int)WriteOffFilter.GetValue(iter, 0);
						OrmMain.DeleteObject<Writeoff> (itemid);
						break;
				}
				UpdateStock();
				break;
		}
	}

	protected void OnNotebookMainSwitchPage(object o, SwitchPageArgs args)
	{
		buttonAdd.Visible = buttonDelete.Visible = buttonEdit.Visible = notebookMain.CurrentPage != 3;
		buttonRefresh.Click();
	}

	protected void OnNotebookStockSwitchPage(object o, SwitchPageArgs args)
	{
		buttonRefresh.Click();
	}

	protected void OnSelectStockDatesDatesChanged(object sender, EventArgs e)
	{
		buttonRefresh.Click();
	}

	protected void OnAction11Activated(object sender, EventArgs e)
	{
		ViewReportExt.Run("StockAllWear", "");
	}
	
	protected void OnAction10Activated(object sender, EventArgs e)
	{
		WearStatement WinStat = new WearStatement();
		WinStat.Show();
		WinStat.Run();
		WinStat.Destroy();
	}

	protected void OnAction12Activated(object sender, EventArgs e)
	{
		ViewReportExt.Run("ListBySize", "");
	}

	protected void OnHelpActionActivated(object sender, EventArgs e)
	{
		System.Diagnostics.Process.Start("workwear_ru.pdf");
	}

	protected void OnActionHistoryActivated (object sender, EventArgs e)
	{
		QSMain.RunChangeLogDlg (this);
	}

	protected void OnActionUpdateActivated (object sender, EventArgs e)
	{
		CheckUpdate.StartCheckUpdateThread (UpdaterFlags.ShowAnyway);
	}

	protected void OnActionSNActivated(object sender, EventArgs e)
	{
		EditSerialNumber.RunDialog();
	}

	protected void OnActionNormsActivated (object sender, EventArgs e)
	{
		var refWin = new ReferenceRepresentation (new workwear.ViewModel.NormVM ());
		var dialog = new OneWidgetDialog (refWin);
		dialog.Show ();
		dialog.Run ();
		dialog.Destroy ();
	}

	protected void OnAction13Activated (object sender, EventArgs e)
	{
		var dlg = new OnIssueStatement ();
		dlg.Show ();
		dlg.Run ();
		dlg.Destroy ();
	}

	protected void OnAction17Activated (object sender, EventArgs e)
	{
		var dlg = new NotIssuedSheetReportDlg ();
		dlg.Show ();
		dlg.Run ();
		dlg.Destroy ();
	}

	protected void OnActionYearRequestSheetActivated (object sender, EventArgs e)
	{
		ViewReportExt.Run("YearRequestSheet", "");
	}

	protected void OnAction21Activated (object sender, EventArgs e)
	{
		var dlg = new QuarterRequestSheetDlg ();
		dlg.Show ();
		dlg.Run ();
		dlg.Destroy ();
	}
}
