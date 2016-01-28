
// This file has been generated by the GUI designer. Do not modify.

public partial class MainWindow
{
	private global::Gtk.UIManager UIManager;
	
	private global::Gtk.Action Action;
	
	private global::Gtk.Action Action1;
	
	private global::Gtk.Action Action18;
	
	private global::Gtk.Action Action3;
	
	private global::Gtk.Action dialogAuthenticationAction;
	
	private global::Gtk.Action UsersAction;
	
	private global::Gtk.Action quitAction;
	
	private global::Gtk.Action Action5;
	
	private global::Gtk.Action Action6;
	
	private global::Gtk.Action Action7;
	
	private global::Gtk.Action Action8;
	
	private global::Gtk.Action Action9;
	
	private global::Gtk.Action aboutAction;
	
	private global::Gtk.Action Action10;
	
	private global::Gtk.Action Action11;
	
	private global::Gtk.Action Action12;
	
	private global::Gtk.Action helpAction;
	
	private global::Gtk.Action ActionHistory;
	
	private global::Gtk.Action ActionUpdate;
	
	private global::Gtk.Action ActionSN;
	
	private global::Gtk.Action ActionNorms;
	
	private global::Gtk.Action Action13;
	
	private global::Gtk.Action Action21;
	
	private global::Gtk.Action ActionYearRequestSheet;
	
	private global::Gtk.Action Action17;
	
	private global::Gtk.VBox vbox1;
	
	private global::Gtk.MenuBar menubar1;
	
	private global::Gtk.Notebook notebookMain;
	
	private global::Gtk.VBox vbox2;
	
	private global::Gtk.HBox hbox2;
	
	private global::Gtk.Label label4;
	
	private global::Gtk.Entry entryObjectSearch;
	
	private global::Gtk.Button buttonObjectSearchClean;
	
	private global::Gtk.ScrolledWindow GtkScrolledWindow;
	
	private global::Gtk.TreeView treeviewObjects;
	
	private global::Gtk.Label label1;
	
	private global::Gtk.VBox vbox3;
	
	private global::Gtk.HBox hbox3;
	
	private global::Gtk.Label label5;
	
	private global::Gtk.Entry entryCardsSearch;
	
	private global::Gtk.Button buttonCardsSearchClear;
	
	private global::Gtk.CheckButton checkCardsOnlyActual;
	
	private global::Gtk.ScrolledWindow GtkScrolledWindow1;
	
	private global::Gtk.TreeView treeviewCards;
	
	private global::Gtk.Label label2;
	
	private global::Gtk.VBox vbox4;
	
	private global::Gtk.HBox hbox4;
	
	private global::QSWidgetLib.SelectPeriod selectStockDates;
	
	private global::Gtk.Notebook notebookStock;
	
	private global::Gtk.ScrolledWindow GtkScrolledWindow2;
	
	private global::Gtk.TreeView treeviewIncome;
	
	private global::Gtk.Label label7;
	
	private global::Gtk.ScrolledWindow GtkScrolledWindow3;
	
	private global::Gtk.TreeView treeviewExpense;
	
	private global::Gtk.Label label8;
	
	private global::Gtk.ScrolledWindow GtkScrolledWindow4;
	
	private global::Gtk.TreeView treeviewWriteOff;
	
	private global::Gtk.Label label10;
	
	private global::Gtk.Label label9;
	
	private global::workwear.StockBalanceView stockbalanceview1;
	
	private global::Gtk.Label label11;
	
	private global::Gtk.HBox hbox7;
	
	private global::Gtk.Button buttonAdd;
	
	private global::Gtk.Button buttonEdit;
	
	private global::Gtk.Button buttonDelete;
	
	private global::Gtk.Button buttonRefresh;
	
	private global::Gtk.Statusbar MainStatusBar;
	
	private global::Gtk.Label labelUser;
	
	private global::Gtk.Label labelStatus;

	protected virtual void Build ()
	{
		global::Stetic.Gui.Initialize (this);
		// Widget MainWindow
		this.UIManager = new global::Gtk.UIManager ();
		global::Gtk.ActionGroup w1 = new global::Gtk.ActionGroup ("Default");
		this.Action = new global::Gtk.Action ("Action", global::Mono.Unix.Catalog.GetString ("Файл"), null, null);
		this.Action.ShortLabel = global::Mono.Unix.Catalog.GetString ("Файл");
		w1.Add (this.Action, null);
		this.Action1 = new global::Gtk.Action ("Action1", global::Mono.Unix.Catalog.GetString ("Справочники"), null, null);
		this.Action1.ShortLabel = global::Mono.Unix.Catalog.GetString ("Справочники");
		w1.Add (this.Action1, null);
		this.Action18 = new global::Gtk.Action ("Action18", global::Mono.Unix.Catalog.GetString ("Ведомости"), null, null);
		this.Action18.ShortLabel = global::Mono.Unix.Catalog.GetString ("Ведомости");
		w1.Add (this.Action18, null);
		this.Action3 = new global::Gtk.Action ("Action3", global::Mono.Unix.Catalog.GetString ("Справка"), null, null);
		this.Action3.ShortLabel = global::Mono.Unix.Catalog.GetString ("Справка");
		w1.Add (this.Action3, null);
		this.dialogAuthenticationAction = new global::Gtk.Action ("dialogAuthenticationAction", global::Mono.Unix.Catalog.GetString ("Изменить пароль"), null, "gtk-dialog-authentication");
		this.dialogAuthenticationAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Изменить пароль");
		w1.Add (this.dialogAuthenticationAction, null);
		this.UsersAction = new global::Gtk.Action ("UsersAction", global::Mono.Unix.Catalog.GetString ("Пользователи"), null, null);
		this.UsersAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Пользователи");
		w1.Add (this.UsersAction, null);
		this.quitAction = new global::Gtk.Action ("quitAction", global::Mono.Unix.Catalog.GetString ("Выход"), null, "gtk-quit");
		this.quitAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Выход");
		w1.Add (this.quitAction, null);
		this.Action5 = new global::Gtk.Action ("Action5", global::Mono.Unix.Catalog.GetString ("Типы номенклатуры"), null, null);
		this.Action5.ShortLabel = global::Mono.Unix.Catalog.GetString ("Типы номенклатуры");
		w1.Add (this.Action5, null);
		this.Action6 = new global::Gtk.Action ("Action6", global::Mono.Unix.Catalog.GetString ("Номенклатура"), null, null);
		this.Action6.ShortLabel = global::Mono.Unix.Catalog.GetString ("Номенклатура");
		w1.Add (this.Action6, null);
		this.Action7 = new global::Gtk.Action ("Action7", global::Mono.Unix.Catalog.GetString ("Единицы измерения"), null, null);
		this.Action7.ShortLabel = global::Mono.Unix.Catalog.GetString ("Единицы измерения");
		w1.Add (this.Action7, null);
		this.Action8 = new global::Gtk.Action ("Action8", global::Mono.Unix.Catalog.GetString ("Должности"), null, null);
		this.Action8.ShortLabel = global::Mono.Unix.Catalog.GetString ("Должности");
		w1.Add (this.Action8, null);
		this.Action9 = new global::Gtk.Action ("Action9", global::Mono.Unix.Catalog.GetString ("Руководители"), null, null);
		this.Action9.ShortLabel = global::Mono.Unix.Catalog.GetString ("Руководители");
		w1.Add (this.Action9, null);
		this.aboutAction = new global::Gtk.Action ("aboutAction", global::Mono.Unix.Catalog.GetString ("_О программе"), null, "gtk-about");
		this.aboutAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("_О программе");
		w1.Add (this.aboutAction, null);
		this.Action10 = new global::Gtk.Action ("Action10", global::Mono.Unix.Catalog.GetString ("Сводная ведомость"), null, null);
		this.Action10.ShortLabel = global::Mono.Unix.Catalog.GetString ("Сводная ведомость");
		w1.Add (this.Action10, null);
		this.Action11 = new global::Gtk.Action ("Action11", global::Mono.Unix.Catalog.GetString ("Складская ведомость"), null, null);
		this.Action11.ShortLabel = global::Mono.Unix.Catalog.GetString ("Складская ведомость");
		w1.Add (this.Action11, null);
		this.Action12 = new global::Gtk.Action ("Action12", global::Mono.Unix.Catalog.GetString ("Список по размерам"), null, null);
		this.Action12.ShortLabel = global::Mono.Unix.Catalog.GetString ("Список по размерам");
		w1.Add (this.Action12, null);
		this.helpAction = new global::Gtk.Action ("helpAction", global::Mono.Unix.Catalog.GetString ("Документация"), null, "gtk-help");
		this.helpAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Документация");
		w1.Add (this.helpAction, null);
		this.ActionHistory = new global::Gtk.Action ("ActionHistory", global::Mono.Unix.Catalog.GetString ("История версий"), null, "gtk-file");
		this.ActionHistory.ShortLabel = global::Mono.Unix.Catalog.GetString ("История версий");
		w1.Add (this.ActionHistory, null);
		this.ActionUpdate = new global::Gtk.Action ("ActionUpdate", global::Mono.Unix.Catalog.GetString ("Проверить обновление..."), null, "gtk-go-down");
		this.ActionUpdate.ShortLabel = global::Mono.Unix.Catalog.GetString ("Проверить обновление...");
		w1.Add (this.ActionUpdate, null);
		this.ActionSN = new global::Gtk.Action ("ActionSN", global::Mono.Unix.Catalog.GetString ("Ввести серийный номер..."), null, null);
		this.ActionSN.ShortLabel = global::Mono.Unix.Catalog.GetString ("Ввести серийный номер...");
		w1.Add (this.ActionSN, null);
		this.ActionNorms = new global::Gtk.Action ("ActionNorms", global::Mono.Unix.Catalog.GetString ("Нормы выдачи"), null, null);
		this.ActionNorms.ShortLabel = global::Mono.Unix.Catalog.GetString ("Нормы выдачи");
		w1.Add (this.ActionNorms, null);
		this.Action13 = new global::Gtk.Action ("Action13", global::Mono.Unix.Catalog.GetString ("Ведомость на выдачу"), null, null);
		this.Action13.ShortLabel = global::Mono.Unix.Catalog.GetString ("Ведомость на выдачу");
		w1.Add (this.Action13, null);
		this.Action21 = new global::Gtk.Action ("Action21", global::Mono.Unix.Catalog.GetString ("Квартальная заявка"), null, null);
		this.Action21.ShortLabel = global::Mono.Unix.Catalog.GetString ("Месячная заявка");
		w1.Add (this.Action21, null);
		this.ActionYearRequestSheet = new global::Gtk.Action ("ActionYearRequestSheet", global::Mono.Unix.Catalog.GetString ("Годовая заявка"), null, null);
		this.ActionYearRequestSheet.ShortLabel = global::Mono.Unix.Catalog.GetString ("Сводная заявка");
		w1.Add (this.ActionYearRequestSheet, null);
		this.Action17 = new global::Gtk.Action ("Action17", global::Mono.Unix.Catalog.GetString ("Справка по невыданному"), null, null);
		this.Action17.ShortLabel = global::Mono.Unix.Catalog.GetString ("Справка по невыданному");
		w1.Add (this.Action17, null);
		this.UIManager.InsertActionGroup (w1, 0);
		this.AddAccelGroup (this.UIManager.AccelGroup);
		this.Name = "MainWindow";
		this.Title = global::Mono.Unix.Catalog.GetString ("QS: Спецодежда и имущество");
		this.Icon = global::Gdk.Pixbuf.LoadFromResource ("workwear.icon.logo.ico");
		this.WindowPosition = ((global::Gtk.WindowPosition)(1));
		// Container child MainWindow.Gtk.Container+ContainerChild
		this.vbox1 = new global::Gtk.VBox ();
		this.vbox1.Name = "vbox1";
		this.vbox1.Spacing = 6;
		// Container child vbox1.Gtk.Box+BoxChild
		this.UIManager.AddUiFromString ("<ui><menubar name='menubar1'><menu name='Action' action='Action'><menuitem name='dialogAuthenticationAction' action='dialogAuthenticationAction'/><menuitem name='UsersAction' action='UsersAction'/><separator/><menuitem name='quitAction' action='quitAction'/></menu><menu name='Action1' action='Action1'><menuitem name='ActionNorms' action='ActionNorms'/><menuitem name='Action5' action='Action5'/><menuitem name='Action6' action='Action6'/><separator/><menuitem name='Action7' action='Action7'/><separator/><menuitem name='Action8' action='Action8'/><menuitem name='Action9' action='Action9'/></menu><menu name='Action18' action='Action18'><menuitem name='Action10' action='Action10'/><menuitem name='Action11' action='Action11'/><separator/><menuitem name='Action12' action='Action12'/><separator/><menuitem name='Action13' action='Action13'/><menuitem name='Action21' action='Action21'/><menuitem name='ActionYearRequestSheet' action='ActionYearRequestSheet'/><menuitem name='Action17' action='Action17'/></menu><menu name='Action3' action='Action3'><menuitem name='helpAction' action='helpAction'/><menuitem name='ActionHistory' action='ActionHistory'/><menuitem name='ActionUpdate' action='ActionUpdate'/><separator/><menuitem name='ActionSN' action='ActionSN'/><separator/><menuitem name='aboutAction' action='aboutAction'/></menu></menubar></ui>");
		this.menubar1 = ((global::Gtk.MenuBar)(this.UIManager.GetWidget ("/menubar1")));
		this.menubar1.Name = "menubar1";
		this.vbox1.Add (this.menubar1);
		global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.menubar1]));
		w2.Position = 0;
		w2.Expand = false;
		w2.Fill = false;
		// Container child vbox1.Gtk.Box+BoxChild
		this.notebookMain = new global::Gtk.Notebook ();
		this.notebookMain.CanFocus = true;
		this.notebookMain.Name = "notebookMain";
		this.notebookMain.CurrentPage = 0;
		// Container child notebookMain.Gtk.Notebook+NotebookChild
		this.vbox2 = new global::Gtk.VBox ();
		this.vbox2.Name = "vbox2";
		this.vbox2.Spacing = 6;
		this.vbox2.BorderWidth = ((uint)(3));
		// Container child vbox2.Gtk.Box+BoxChild
		this.hbox2 = new global::Gtk.HBox ();
		this.hbox2.Name = "hbox2";
		this.hbox2.Spacing = 6;
		// Container child hbox2.Gtk.Box+BoxChild
		this.label4 = new global::Gtk.Label ();
		this.label4.Name = "label4";
		this.label4.LabelProp = global::Mono.Unix.Catalog.GetString ("Поиск по имени и адресу:");
		this.hbox2.Add (this.label4);
		global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.label4]));
		w3.Position = 0;
		w3.Expand = false;
		w3.Fill = false;
		// Container child hbox2.Gtk.Box+BoxChild
		this.entryObjectSearch = new global::Gtk.Entry ();
		this.entryObjectSearch.CanFocus = true;
		this.entryObjectSearch.Name = "entryObjectSearch";
		this.entryObjectSearch.IsEditable = true;
		this.entryObjectSearch.InvisibleChar = '●';
		this.hbox2.Add (this.entryObjectSearch);
		global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.entryObjectSearch]));
		w4.Position = 1;
		// Container child hbox2.Gtk.Box+BoxChild
		this.buttonObjectSearchClean = new global::Gtk.Button ();
		this.buttonObjectSearchClean.CanFocus = true;
		this.buttonObjectSearchClean.Name = "buttonObjectSearchClean";
		this.buttonObjectSearchClean.UseUnderline = true;
		global::Gtk.Image w5 = new global::Gtk.Image ();
		w5.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-clear", global::Gtk.IconSize.Menu);
		this.buttonObjectSearchClean.Image = w5;
		this.hbox2.Add (this.buttonObjectSearchClean);
		global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.buttonObjectSearchClean]));
		w6.Position = 2;
		w6.Expand = false;
		w6.Fill = false;
		this.vbox2.Add (this.hbox2);
		global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hbox2]));
		w7.Position = 0;
		w7.Expand = false;
		w7.Fill = false;
		// Container child vbox2.Gtk.Box+BoxChild
		this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
		this.GtkScrolledWindow.Name = "GtkScrolledWindow";
		this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
		this.treeviewObjects = new global::Gtk.TreeView ();
		this.treeviewObjects.CanFocus = true;
		this.treeviewObjects.Name = "treeviewObjects";
		this.GtkScrolledWindow.Add (this.treeviewObjects);
		this.vbox2.Add (this.GtkScrolledWindow);
		global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.GtkScrolledWindow]));
		w9.Position = 1;
		this.notebookMain.Add (this.vbox2);
		// Notebook tab
		this.label1 = new global::Gtk.Label ();
		this.label1.Name = "label1";
		this.label1.LabelProp = global::Mono.Unix.Catalog.GetString ("Объекты");
		this.notebookMain.SetTabLabel (this.vbox2, this.label1);
		this.label1.ShowAll ();
		// Container child notebookMain.Gtk.Notebook+NotebookChild
		this.vbox3 = new global::Gtk.VBox ();
		this.vbox3.Name = "vbox3";
		this.vbox3.Spacing = 6;
		this.vbox3.BorderWidth = ((uint)(3));
		// Container child vbox3.Gtk.Box+BoxChild
		this.hbox3 = new global::Gtk.HBox ();
		this.hbox3.Name = "hbox3";
		this.hbox3.Spacing = 6;
		// Container child hbox3.Gtk.Box+BoxChild
		this.label5 = new global::Gtk.Label ();
		this.label5.Name = "label5";
		this.label5.LabelProp = global::Mono.Unix.Catalog.GetString ("Поиск по имени:");
		this.hbox3.Add (this.label5);
		global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.hbox3 [this.label5]));
		w11.Position = 0;
		w11.Expand = false;
		w11.Fill = false;
		// Container child hbox3.Gtk.Box+BoxChild
		this.entryCardsSearch = new global::Gtk.Entry ();
		this.entryCardsSearch.CanFocus = true;
		this.entryCardsSearch.Name = "entryCardsSearch";
		this.entryCardsSearch.IsEditable = true;
		this.entryCardsSearch.InvisibleChar = '●';
		this.hbox3.Add (this.entryCardsSearch);
		global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.hbox3 [this.entryCardsSearch]));
		w12.Position = 1;
		// Container child hbox3.Gtk.Box+BoxChild
		this.buttonCardsSearchClear = new global::Gtk.Button ();
		this.buttonCardsSearchClear.CanFocus = true;
		this.buttonCardsSearchClear.Name = "buttonCardsSearchClear";
		this.buttonCardsSearchClear.UseUnderline = true;
		global::Gtk.Image w13 = new global::Gtk.Image ();
		w13.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-clear", global::Gtk.IconSize.Menu);
		this.buttonCardsSearchClear.Image = w13;
		this.hbox3.Add (this.buttonCardsSearchClear);
		global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.hbox3 [this.buttonCardsSearchClear]));
		w14.Position = 2;
		w14.Expand = false;
		w14.Fill = false;
		// Container child hbox3.Gtk.Box+BoxChild
		this.checkCardsOnlyActual = new global::Gtk.CheckButton ();
		this.checkCardsOnlyActual.CanFocus = true;
		this.checkCardsOnlyActual.Name = "checkCardsOnlyActual";
		this.checkCardsOnlyActual.Label = global::Mono.Unix.Catalog.GetString ("Только работающие");
		this.checkCardsOnlyActual.Active = true;
		this.checkCardsOnlyActual.DrawIndicator = true;
		this.checkCardsOnlyActual.UseUnderline = true;
		this.hbox3.Add (this.checkCardsOnlyActual);
		global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.hbox3 [this.checkCardsOnlyActual]));
		w15.Position = 3;
		w15.Expand = false;
		this.vbox3.Add (this.hbox3);
		global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.hbox3]));
		w16.Position = 0;
		w16.Expand = false;
		w16.Fill = false;
		// Container child vbox3.Gtk.Box+BoxChild
		this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow ();
		this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
		this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
		this.treeviewCards = new global::Gtk.TreeView ();
		this.treeviewCards.CanFocus = true;
		this.treeviewCards.Name = "treeviewCards";
		this.GtkScrolledWindow1.Add (this.treeviewCards);
		this.vbox3.Add (this.GtkScrolledWindow1);
		global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.GtkScrolledWindow1]));
		w18.Position = 1;
		this.notebookMain.Add (this.vbox3);
		global::Gtk.Notebook.NotebookChild w19 = ((global::Gtk.Notebook.NotebookChild)(this.notebookMain [this.vbox3]));
		w19.Position = 1;
		// Notebook tab
		this.label2 = new global::Gtk.Label ();
		this.label2.Name = "label2";
		this.label2.LabelProp = global::Mono.Unix.Catalog.GetString ("Карточки сотрудников");
		this.notebookMain.SetTabLabel (this.vbox3, this.label2);
		this.label2.ShowAll ();
		// Container child notebookMain.Gtk.Notebook+NotebookChild
		this.vbox4 = new global::Gtk.VBox ();
		this.vbox4.Name = "vbox4";
		this.vbox4.Spacing = 6;
		// Container child vbox4.Gtk.Box+BoxChild
		this.hbox4 = new global::Gtk.HBox ();
		this.hbox4.Name = "hbox4";
		this.hbox4.Spacing = 6;
		// Container child hbox4.Gtk.Box+BoxChild
		this.selectStockDates = new global::QSWidgetLib.SelectPeriod ();
		this.selectStockDates.Events = ((global::Gdk.EventMask)(256));
		this.selectStockDates.Name = "selectStockDates";
		this.selectStockDates.DateBegin = new global::System.DateTime (0);
		this.selectStockDates.DateEnd = new global::System.DateTime (0);
		this.selectStockDates.AutoDateSeparation = true;
		this.selectStockDates.ShowToday = true;
		this.selectStockDates.ShowWeek = false;
		this.selectStockDates.ShowMonth = true;
		this.selectStockDates.Show3Month = false;
		this.selectStockDates.Show6Month = true;
		this.selectStockDates.ShowYear = true;
		this.selectStockDates.ShowAllTime = false;
		this.selectStockDates.ShowCurWeek = false;
		this.selectStockDates.ShowCurMonth = false;
		this.selectStockDates.ShowCurQuarter = false;
		this.selectStockDates.ShowCurYear = false;
		this.hbox4.Add (this.selectStockDates);
		global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.hbox4 [this.selectStockDates]));
		w20.Position = 1;
		w20.Expand = false;
		w20.Fill = false;
		this.vbox4.Add (this.hbox4);
		global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.hbox4]));
		w21.Position = 0;
		w21.Expand = false;
		// Container child vbox4.Gtk.Box+BoxChild
		this.notebookStock = new global::Gtk.Notebook ();
		this.notebookStock.CanFocus = true;
		this.notebookStock.Name = "notebookStock";
		this.notebookStock.CurrentPage = 0;
		this.notebookStock.TabPos = ((global::Gtk.PositionType)(0));
		// Container child notebookStock.Gtk.Notebook+NotebookChild
		this.GtkScrolledWindow2 = new global::Gtk.ScrolledWindow ();
		this.GtkScrolledWindow2.Name = "GtkScrolledWindow2";
		this.GtkScrolledWindow2.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow2.Gtk.Container+ContainerChild
		this.treeviewIncome = new global::Gtk.TreeView ();
		this.treeviewIncome.CanFocus = true;
		this.treeviewIncome.Name = "treeviewIncome";
		this.GtkScrolledWindow2.Add (this.treeviewIncome);
		this.notebookStock.Add (this.GtkScrolledWindow2);
		// Notebook tab
		this.label7 = new global::Gtk.Label ();
		this.label7.Name = "label7";
		this.label7.LabelProp = global::Mono.Unix.Catalog.GetString ("Приход");
		this.notebookStock.SetTabLabel (this.GtkScrolledWindow2, this.label7);
		this.label7.ShowAll ();
		// Container child notebookStock.Gtk.Notebook+NotebookChild
		this.GtkScrolledWindow3 = new global::Gtk.ScrolledWindow ();
		this.GtkScrolledWindow3.Name = "GtkScrolledWindow3";
		this.GtkScrolledWindow3.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow3.Gtk.Container+ContainerChild
		this.treeviewExpense = new global::Gtk.TreeView ();
		this.treeviewExpense.CanFocus = true;
		this.treeviewExpense.Name = "treeviewExpense";
		this.GtkScrolledWindow3.Add (this.treeviewExpense);
		this.notebookStock.Add (this.GtkScrolledWindow3);
		global::Gtk.Notebook.NotebookChild w25 = ((global::Gtk.Notebook.NotebookChild)(this.notebookStock [this.GtkScrolledWindow3]));
		w25.Position = 1;
		// Notebook tab
		this.label8 = new global::Gtk.Label ();
		this.label8.Name = "label8";
		this.label8.LabelProp = global::Mono.Unix.Catalog.GetString ("Расход");
		this.notebookStock.SetTabLabel (this.GtkScrolledWindow3, this.label8);
		this.label8.ShowAll ();
		// Container child notebookStock.Gtk.Notebook+NotebookChild
		this.GtkScrolledWindow4 = new global::Gtk.ScrolledWindow ();
		this.GtkScrolledWindow4.Name = "GtkScrolledWindow4";
		this.GtkScrolledWindow4.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow4.Gtk.Container+ContainerChild
		this.treeviewWriteOff = new global::Gtk.TreeView ();
		this.treeviewWriteOff.CanFocus = true;
		this.treeviewWriteOff.Name = "treeviewWriteOff";
		this.GtkScrolledWindow4.Add (this.treeviewWriteOff);
		this.notebookStock.Add (this.GtkScrolledWindow4);
		global::Gtk.Notebook.NotebookChild w27 = ((global::Gtk.Notebook.NotebookChild)(this.notebookStock [this.GtkScrolledWindow4]));
		w27.Position = 2;
		// Notebook tab
		this.label10 = new global::Gtk.Label ();
		this.label10.Name = "label10";
		this.label10.LabelProp = global::Mono.Unix.Catalog.GetString ("Списание");
		this.notebookStock.SetTabLabel (this.GtkScrolledWindow4, this.label10);
		this.label10.ShowAll ();
		this.vbox4.Add (this.notebookStock);
		global::Gtk.Box.BoxChild w28 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.notebookStock]));
		w28.Position = 1;
		this.notebookMain.Add (this.vbox4);
		global::Gtk.Notebook.NotebookChild w29 = ((global::Gtk.Notebook.NotebookChild)(this.notebookMain [this.vbox4]));
		w29.Position = 2;
		// Notebook tab
		this.label9 = new global::Gtk.Label ();
		this.label9.Name = "label9";
		this.label9.LabelProp = global::Mono.Unix.Catalog.GetString ("Складские документы");
		this.notebookMain.SetTabLabel (this.vbox4, this.label9);
		this.label9.ShowAll ();
		// Container child notebookMain.Gtk.Notebook+NotebookChild
		this.stockbalanceview1 = new global::workwear.StockBalanceView ();
		this.stockbalanceview1.Events = ((global::Gdk.EventMask)(256));
		this.stockbalanceview1.Name = "stockbalanceview1";
		this.notebookMain.Add (this.stockbalanceview1);
		global::Gtk.Notebook.NotebookChild w30 = ((global::Gtk.Notebook.NotebookChild)(this.notebookMain [this.stockbalanceview1]));
		w30.Position = 3;
		// Notebook tab
		this.label11 = new global::Gtk.Label ();
		this.label11.Name = "label11";
		this.label11.LabelProp = global::Mono.Unix.Catalog.GetString ("Остатки");
		this.notebookMain.SetTabLabel (this.stockbalanceview1, this.label11);
		this.label11.ShowAll ();
		this.vbox1.Add (this.notebookMain);
		global::Gtk.Box.BoxChild w31 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.notebookMain]));
		w31.Position = 1;
		// Container child vbox1.Gtk.Box+BoxChild
		this.hbox7 = new global::Gtk.HBox ();
		this.hbox7.Name = "hbox7";
		this.hbox7.Spacing = 6;
		this.hbox7.BorderWidth = ((uint)(3));
		// Container child hbox7.Gtk.Box+BoxChild
		this.buttonAdd = new global::Gtk.Button ();
		this.buttonAdd.CanFocus = true;
		this.buttonAdd.Name = "buttonAdd";
		this.buttonAdd.UseUnderline = true;
		this.buttonAdd.Label = global::Mono.Unix.Catalog.GetString ("Добавить");
		global::Gtk.Image w32 = new global::Gtk.Image ();
		w32.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-add", global::Gtk.IconSize.Menu);
		this.buttonAdd.Image = w32;
		this.hbox7.Add (this.buttonAdd);
		global::Gtk.Box.BoxChild w33 = ((global::Gtk.Box.BoxChild)(this.hbox7 [this.buttonAdd]));
		w33.Position = 0;
		w33.Expand = false;
		w33.Fill = false;
		// Container child hbox7.Gtk.Box+BoxChild
		this.buttonEdit = new global::Gtk.Button ();
		this.buttonEdit.CanFocus = true;
		this.buttonEdit.Name = "buttonEdit";
		this.buttonEdit.UseUnderline = true;
		this.buttonEdit.Label = global::Mono.Unix.Catalog.GetString ("Изменить");
		global::Gtk.Image w34 = new global::Gtk.Image ();
		w34.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-edit", global::Gtk.IconSize.Menu);
		this.buttonEdit.Image = w34;
		this.hbox7.Add (this.buttonEdit);
		global::Gtk.Box.BoxChild w35 = ((global::Gtk.Box.BoxChild)(this.hbox7 [this.buttonEdit]));
		w35.Position = 1;
		w35.Expand = false;
		w35.Fill = false;
		// Container child hbox7.Gtk.Box+BoxChild
		this.buttonDelete = new global::Gtk.Button ();
		this.buttonDelete.CanFocus = true;
		this.buttonDelete.Name = "buttonDelete";
		this.buttonDelete.UseUnderline = true;
		this.buttonDelete.Label = global::Mono.Unix.Catalog.GetString ("Удалить");
		global::Gtk.Image w36 = new global::Gtk.Image ();
		w36.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-remove", global::Gtk.IconSize.Menu);
		this.buttonDelete.Image = w36;
		this.hbox7.Add (this.buttonDelete);
		global::Gtk.Box.BoxChild w37 = ((global::Gtk.Box.BoxChild)(this.hbox7 [this.buttonDelete]));
		w37.Position = 2;
		w37.Expand = false;
		w37.Fill = false;
		// Container child hbox7.Gtk.Box+BoxChild
		this.buttonRefresh = new global::Gtk.Button ();
		this.buttonRefresh.TooltipMarkup = "Обновить текущую таблицу.";
		this.buttonRefresh.CanFocus = true;
		this.buttonRefresh.Name = "buttonRefresh";
		this.buttonRefresh.UseUnderline = true;
		this.buttonRefresh.Label = global::Mono.Unix.Catalog.GetString ("Обновить");
		global::Gtk.Image w38 = new global::Gtk.Image ();
		w38.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-refresh", global::Gtk.IconSize.Menu);
		this.buttonRefresh.Image = w38;
		this.hbox7.Add (this.buttonRefresh);
		global::Gtk.Box.BoxChild w39 = ((global::Gtk.Box.BoxChild)(this.hbox7 [this.buttonRefresh]));
		w39.PackType = ((global::Gtk.PackType)(1));
		w39.Position = 3;
		w39.Expand = false;
		w39.Fill = false;
		this.vbox1.Add (this.hbox7);
		global::Gtk.Box.BoxChild w40 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.hbox7]));
		w40.Position = 2;
		w40.Expand = false;
		w40.Fill = false;
		// Container child vbox1.Gtk.Box+BoxChild
		this.MainStatusBar = new global::Gtk.Statusbar ();
		this.MainStatusBar.Name = "MainStatusBar";
		this.MainStatusBar.Spacing = 6;
		// Container child MainStatusBar.Gtk.Box+BoxChild
		this.labelUser = new global::Gtk.Label ();
		this.labelUser.Name = "labelUser";
		this.labelUser.LabelProp = global::Mono.Unix.Catalog.GetString ("Пользователь");
		this.MainStatusBar.Add (this.labelUser);
		global::Gtk.Box.BoxChild w41 = ((global::Gtk.Box.BoxChild)(this.MainStatusBar [this.labelUser]));
		w41.Position = 0;
		w41.Expand = false;
		w41.Fill = false;
		// Container child MainStatusBar.Gtk.Box+BoxChild
		this.labelStatus = new global::Gtk.Label ();
		this.labelStatus.Name = "labelStatus";
		this.labelStatus.LabelProp = global::Mono.Unix.Catalog.GetString ("Ok");
		this.MainStatusBar.Add (this.labelStatus);
		global::Gtk.Box.BoxChild w42 = ((global::Gtk.Box.BoxChild)(this.MainStatusBar [this.labelStatus]));
		w42.Position = 2;
		w42.Expand = false;
		w42.Fill = false;
		this.vbox1.Add (this.MainStatusBar);
		global::Gtk.Box.BoxChild w43 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.MainStatusBar]));
		w43.Position = 3;
		w43.Expand = false;
		w43.Fill = false;
		this.Add (this.vbox1);
		if ((this.Child != null)) {
			this.Child.ShowAll ();
		}
		this.DefaultWidth = 709;
		this.DefaultHeight = 458;
		this.Show ();
		this.DeleteEvent += new global::Gtk.DeleteEventHandler (this.OnDeleteEvent);
		this.dialogAuthenticationAction.Activated += new global::System.EventHandler (this.OnDialogAuthenticationActionActivated);
		this.UsersAction.Activated += new global::System.EventHandler (this.OnUsersActionActivated);
		this.quitAction.Activated += new global::System.EventHandler (this.OnQuitActionActivated);
		this.Action5.Activated += new global::System.EventHandler (this.OnAction5Activated);
		this.Action6.Activated += new global::System.EventHandler (this.OnAction6Activated);
		this.Action7.Activated += new global::System.EventHandler (this.OnAction7Activated);
		this.Action8.Activated += new global::System.EventHandler (this.OnAction8Activated);
		this.Action9.Activated += new global::System.EventHandler (this.OnAction9Activated);
		this.aboutAction.Activated += new global::System.EventHandler (this.OnAboutActionActivated);
		this.Action10.Activated += new global::System.EventHandler (this.OnAction10Activated);
		this.Action11.Activated += new global::System.EventHandler (this.OnAction11Activated);
		this.Action12.Activated += new global::System.EventHandler (this.OnAction12Activated);
		this.helpAction.Activated += new global::System.EventHandler (this.OnHelpActionActivated);
		this.ActionHistory.Activated += new global::System.EventHandler (this.OnActionHistoryActivated);
		this.ActionUpdate.Activated += new global::System.EventHandler (this.OnActionUpdateActivated);
		this.ActionSN.Activated += new global::System.EventHandler (this.OnActionSNActivated);
		this.ActionNorms.Activated += new global::System.EventHandler (this.OnActionNormsActivated);
		this.Action13.Activated += new global::System.EventHandler (this.OnAction13Activated);
		this.ActionYearRequestSheet.Activated += new global::System.EventHandler (this.OnActionYearRequestSheetActivated);
		this.Action17.Activated += new global::System.EventHandler (this.OnAction17Activated);
		this.notebookMain.SwitchPage += new global::Gtk.SwitchPageHandler (this.OnNotebookMainSwitchPage);
		this.entryObjectSearch.Changed += new global::System.EventHandler (this.OnEntryObjectSearchChanged);
		this.buttonObjectSearchClean.Clicked += new global::System.EventHandler (this.OnButtonObjectSearchCleanClicked);
		this.treeviewObjects.CursorChanged += new global::System.EventHandler (this.OnTreeviewObjectsCursorChanged);
		this.treeviewObjects.RowActivated += new global::Gtk.RowActivatedHandler (this.OnTreeviewObjectsRowActivated);
		this.entryCardsSearch.Changed += new global::System.EventHandler (this.OnEntryCardsSearchChanged);
		this.buttonCardsSearchClear.Clicked += new global::System.EventHandler (this.OnButtonCardsSearchClearClicked);
		this.checkCardsOnlyActual.Clicked += new global::System.EventHandler (this.OnCheckOnlyActualClicked);
		this.treeviewCards.RowActivated += new global::Gtk.RowActivatedHandler (this.OnTreeviewCardsRowActivated);
		this.treeviewCards.CursorChanged += new global::System.EventHandler (this.OnTreeviewCardsCursorChanged);
		this.selectStockDates.DatesChanged += new global::System.EventHandler (this.OnSelectStockDatesDatesChanged);
		this.notebookStock.SwitchPage += new global::Gtk.SwitchPageHandler (this.OnNotebookStockSwitchPage);
		this.treeviewIncome.RowActivated += new global::Gtk.RowActivatedHandler (this.OnTreeviewIncomeRowActivated);
		this.treeviewIncome.CursorChanged += new global::System.EventHandler (this.OnTreeviewIncomeCursorChanged);
		this.treeviewExpense.CursorChanged += new global::System.EventHandler (this.OnTreeviewExpenseCursorChanged);
		this.treeviewExpense.RowActivated += new global::Gtk.RowActivatedHandler (this.OnTreeviewExpenseRowActivated);
		this.treeviewWriteOff.CursorChanged += new global::System.EventHandler (this.OnTreeviewWriteOffCursorChanged);
		this.treeviewWriteOff.RowActivated += new global::Gtk.RowActivatedHandler (this.OnTreeviewWriteOffRowActivated);
		this.buttonAdd.Clicked += new global::System.EventHandler (this.OnButtonAddClicked);
		this.buttonEdit.Clicked += new global::System.EventHandler (this.OnButtonEditClicked);
		this.buttonDelete.Clicked += new global::System.EventHandler (this.OnButtonDeleteClicked);
		this.buttonRefresh.Clicked += new global::System.EventHandler (this.OnButtonRefreshClicked);
	}
}
