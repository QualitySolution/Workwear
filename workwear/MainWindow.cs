using System;
using Gtk;
using workwear;
using QSProjectsLib;
using QSSupportLib;
using MySql.Data.MySqlClient;

public partial class MainWindow: Gtk.Window
{	
	public MainWindow(): base (Gtk.WindowType.Toplevel)
	{
		Build();
		
		//Передаем лебл
		MainClass.StatusBarLabel = labelStatus;
		Reference.RunReferenceItemDlg += OnRunReferenceItemDialog;
		QSMain.ReferenceUpdated += OnReferenceUpdate;

		try
		{
			MainSupport.BaseParameters = new BaseParam(QSMain.connectionDB);
		}
		catch(MySqlException e)
		{
			Console.WriteLine(e.Message);
			MessageDialog BaseError = new MessageDialog ( this, DialogFlags.DestroyWithParent,
	                                      MessageType.Warning, 
	                                      ButtonsType.Close, 
	                                      "Не удалось получить информацию о базе данных.");
			BaseError.Run();
			BaseError.Destroy();
			Environment.Exit(0);
		}
		
		MainSupport.ProjectVerion = new AppVersion(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.ToString(),
		 	"gpl",
		 	System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
		MainSupport.TestVersion(this);

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
				"вопросы вы можете задать их на форуме программы: https://groups.google.com/forum/?fromgroups#!forum/bazarsoft " +
				"или обратится в нашу тех. поддержку.";
			MessageDialog md = new MessageDialog ( this, DialogFlags.DestroyWithParent,
			                                      MessageType.Info, 
			                                      ButtonsType.Ok,
			                                      Message);
			md.Run ();
			md.Destroy();
			dialogAuthenticationAction.Sensitive = false;
		}

		//Загружаем информацию о пользователе
		if(QSMain.User.TestUserExistByLogin (true))
			QSMain.User.UpdateUserInfoByLogin ();
		UsersAction.Sensitive = QSMain.User.admin;
		labelUser.LabelProp = QSMain.User.Name;

		PrepareObject();
		PrepareCards();
		PrepareStock();
		UpdateObject();
		notebookMain.CurrentPage = 0;
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Application.Quit();
		a.RetVal = true;
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
	
	protected void OnReferenceUpdate(object sender, QSMain.ReferenceUpdatedEventArgs e)
	{
	/*	switch (e.ReferenceTable) {
		case "doc_types":
			ComboWorks.ComboFillReference (comboDocType, "doc_types", 0);
		break;
		} */
	}
	
	protected void OnRunReferenceItemDialog(object sender, Reference.RunReferenceItemDlgEventArgs e)
	{
		ResponseType Result;
		switch (e.TableName)
		{
		case "item_types":
			ItemType ItemTypeEdit = new ItemType();
			if(e.NewItem)
				ItemTypeEdit.NewItem = true;
			else 
				ItemTypeEdit.Fill(e.ItemId);
			ItemTypeEdit.Show();
			Result = (ResponseType)ItemTypeEdit.Run();
			ItemTypeEdit.Destroy();
			break;
		case "nomenclature":
			Nomenclature NomenclatureEdit = new Nomenclature();
			if(e.NewItem)
				NomenclatureEdit.NewItem = true;
			else 
				NomenclatureEdit.Fill(e.ItemId);
			NomenclatureEdit.Show();
			Result = (ResponseType)NomenclatureEdit.Run();
			NomenclatureEdit.Destroy();
			break;
		default:
			Result = ResponseType.None;
			break;
		}
		e.Result = Result;
	}
	
	protected void OnAction7Activated(object sender, EventArgs e)
	{
		Reference winref = new Reference();
		winref.SetMode(true,false,true,true,true);
		winref.FillList("units","Единица измерения", "Единицы измерения");
		winref.Show();
		winref.Run();
		winref.Destroy();
	}

	protected void OnAction8Activated(object sender, EventArgs e)
	{
		Reference winref = new Reference();
		winref.SetMode(true,false,true,true,true);
		winref.FillList("posts","Должность", "Должности");
		winref.Show();
		winref.Run();
		winref.Destroy();
	}
	
	protected void OnAction9Activated(object sender, EventArgs e)
	{
		Reference winref = new Reference();
		winref.SetMode(true,false,true,true,true);
		winref.FillList("leaders","Руководитель", "Руководители");
		winref.Show();
		winref.Run();
		winref.Destroy();
	}
	
	protected void OnAction5Activated(object sender, EventArgs e)
	{
		Reference winref = new Reference();
		winref.SetMode(false, false, true, true, true);
		winref.SqlSelect = "SELECT id, name, norm_quantity, norm_life FROM @tablename ";
		winref.Columns.Add(new Reference.ColumnInfo("Норма выдачи", "{2}", false));
		winref.Columns.Add(new Reference.ColumnInfo("Срок носки", "{3} мес.", false));
		winref.FillList("item_types","Тип номенклатуры", "Виды номенклатуры");
		winref.Show();
		winref.Run();
		winref.Destroy();
	}

	protected void OnAction6Activated(object sender, EventArgs e)
	{
		Reference winref = new Reference();
		winref.SetMode(false, false, true, true, true);
		winref.FillList("nomenclature","Номенклатура", "Номенклатуры");
		winref.Show();
		winref.Run();
		winref.Destroy();

	}

	protected void OnAboutActionActivated(object sender, EventArgs e)
	{
		AboutDialog dialog = new AboutDialog ();
		dialog.ProgramName = "QS:Учет спецодежды";

		Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
		dialog.Version = String.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);

		dialog.Logo = Gdk.Pixbuf.LoadFromResource ("workwear.icon.logo.png");

		dialog.Comments = "Программа позволяет вести учет спецодежды." +
			"\nРазработана на MonoDevelop с использованием открытых технологий Mono, GTK#, MySQL." +
			"\nТелефон тех. поддержки +7(812)575-79-44";

		dialog.Copyright = "Quality Solution 2013";

		dialog.Authors = new string [] {"Ганьков Андрей <gav@qsolution.ru>"};

		dialog.Website = "http://www.qsolution.ru/";

		dialog.Run ();
		dialog.Destroy();
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
				WearCard winWearCard = new WearCard();
				winWearCard.NewItem = true;
				winWearCard.Show();
				winWearCard.Run();
				winWearCard.Destroy();
				UpdateCards();
				break;
				case 2:
				switch (notebookStock.CurrentPage)
				{
					case 0:
						IncomeDoc winIncome = new IncomeDoc();
						winIncome.NewItem = true;
						winIncome.Show();
						winIncome.Run();
						winIncome.Destroy();
						break;
					case 1:
						ExpenseDoc winExpense = new ExpenseDoc();
						winExpense.NewItem = true;
						winExpense.Show();
						winExpense.Run();
						winExpense.Destroy();
						break;
					case 2:
						WriteOffDoc winWriteOff = new WriteOffDoc();
						winWriteOff.NewItem = true;
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
				WearCard winWearCadr = new WearCard();
				winWearCadr.Fill(itemid);
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
						IncomeDoc winIncome = new IncomeDoc();
						winIncome.Fill(itemid);
						winIncome.Show();
						result = (ResponseType) winIncome.Run();
						winIncome.Destroy();
						break;
					case 1:
						treeviewExpense.Selection.GetSelected(out iter);
						itemid = (int)ExpenseFilter.GetValue(iter, 0);
						ExpenseDoc winExpense = new ExpenseDoc();
						winExpense.Fill(itemid);
						winExpense.Show();
						result = (ResponseType) winExpense.Run();
						winExpense.Destroy();
						break;
					case 2:
						treeviewWriteOff.Selection.GetSelected(out iter);
						itemid = (int)WriteOffFilter.GetValue(iter, 0);
						WriteOffDoc winWriteOff = new WriteOffDoc();
						winWriteOff.Fill(itemid);
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
		Delete winDelete = new Delete();

		switch (notebookMain.CurrentPage) 
		{
			case 0:
				treeviewObjects.Selection.GetSelected(out iter);
				itemid = (int) ObjectFilter.GetValue(iter,0);
				winDelete.RunDeletion("objects", itemid);
				UpdateObject();
				break;
			case 1:
				treeviewCards.Selection.GetSelected(out iter);
				itemid = (int) CardsFilter.GetValue(iter,0);
				winDelete.RunDeletion("wear_cards", itemid);
				UpdateCards();
				break;
			case 2:
				switch (notebookStock.CurrentPage)
				{
					case 0:
						treeviewIncome.Selection.GetSelected(out iter);
						itemid = (int)IncomeFilter.GetValue(iter, 0);
						winDelete.RunDeletion("stock_income", itemid);
						break;
					case 1:
						treeviewExpense.Selection.GetSelected(out iter);
						itemid = (int)ExpenseFilter.GetValue(iter, 0);
						winDelete.RunDeletion("stock_expense", itemid);
						break;
					case 2:
						treeviewWriteOff.Selection.GetSelected(out iter);
						itemid = (int)WriteOffFilter.GetValue(iter, 0);
						winDelete.RunDeletion("stock_write_off", itemid);
						break;
				}
				UpdateStock();
				break;
		}
		winDelete.Destroy();

	}

	protected void OnNotebookMainSwitchPage(object o, SwitchPageArgs args)
	{
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
		ViewReportExt.Run("ListBySize", "", true);
	}

}