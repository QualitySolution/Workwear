using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Autofac;
using Gtk;
using MySql.Data.MySqlClient;
using NLog;
using QS.BusinessCommon.Domain;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.NewsFeed;
using QS.NewsFeed.Views;
using QS.Project.Domain;
using QS.Project.Versioning;
using QS.Project.Views;
using QS.Report;
using QS.Report.ViewModels;
using QS.Serial.ViewModels;
using QS.Services;
using QS.Tdi;
using QS.Tdi.Gtk;
using QS.Updater;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Control.ESVM;
using QS.ViewModels.Dialog;
using QSOrmProject;
using QSProjectsLib;
using QSTelemetry;
using workwear;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Domain.Users;
using workwear.Journal.ViewModels.Communications;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Regulations;
using workwear.Journal.ViewModels.Statements;
using workwear.Journal.ViewModels.Stock;
using workwear.Journal.ViewModels.Tools;
using workwear.Models.Import;
using workwear.ReportParameters.ViewModels;
using workwear.ReportsDlg;
using workwear.Repository.Stock;
using workwear.Tools;
using workwear.Tools.Features;
using workwear.ViewModels.Company;
using workwear.ViewModels.Import;
using workwear.ViewModels.Stock;
using workwear.ViewModels.Tools;
using workwear.ViewModels.User;

public partial class MainWindow : Gtk.Window
{
	private static Logger logger = LogManager.GetCurrentClassLogger();

	private ILifetimeScope AutofacScope = MainClass.AppDIContainer.BeginLifetimeScope();
	public TdiNavigationManager NavigationManager;
	public IProgressBarDisplayable ProgressBar;
	public IUnitOfWork UoW = UnitOfWorkFactory.CreateWithoutRoot();

	public FeaturesService FeaturesService { get; private set; }


	public MainWindow() : base(Gtk.WindowType.Toplevel)
	{
		Build();
		//Передаем лебл
		QSMain.StatusBarLabel = labelStatus;
		ProgressBar = progresswidget1;
		this.Title = AutofacScope.Resolve<IApplicationInfo>().ProductTitle;
		QSMain.MakeNewStatusTargetForNlog();

		QSMain.CheckServer(this); // Проверяем настройки сервера

		NavigationManager = AutofacScope.Resolve<TdiNavigationManager>(new TypedParameter(typeof(TdiNotebook), tdiMain));
		tdiMain.WidgetResolver = AutofacScope.Resolve<ITDIWidgetResolver>(new TypedParameter(typeof(Assembly[]), new[] { Assembly.GetAssembly(typeof(OrganizationViewModel)) }));

		var checker = new VersionCheckerService(MainClass.AppDIContainer);
		checker.RunUpdate();

		var userService = AutofacScope.Resolve<IUserService>();
		var user = userService.GetCurrentUser(UoW);
		var databaseInfo = AutofacScope.Resolve<IDataBaseInfo>();

		//Пока такая реализация чтобы не плодить сущностей.
		var connectionBuilder = AutofacScope.Resolve<MySqlConnectionStringBuilder>();
		if(connectionBuilder.UserID == "root") {
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

		if(databaseInfo.IsDemo) {
			string Message = "Вы подключились к демонстрационному серверу. НЕ используете его для работы! " +
				"Введенные данные будут доступны другим пользователям.\n\nДля работы вам необходимо " +
				"установить собственный сервер или купить подписку на QS:Облако.";
			MessageDialog md = new MessageDialog(this, DialogFlags.DestroyWithParent,
												  MessageType.Info,
												  ButtonsType.Ok,
												  Message);
			md.Run();
			md.Destroy();
			dialogAuthenticationAction.Sensitive = false;
			ActionSN.Sensitive = false;
		}

		bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		if(isWindows)
			this.KeyReleaseEvent += ClipboardWorkaround.HandleKeyReleaseEvent;
		TDIMain.MainNotebook = tdiMain;
		this.KeyReleaseEvent += TDIMain.TDIHandleKeyReleaseEvent;

		UsersAction.Sensitive = user.IsAdmin;
		labelUser.LabelProp = user.Name;

		//Настраиваем новости
		var feeds = new List<NewsFeed>(){
			new NewsFeed("workwearsite", "Новости программы", "http://workwear.qsolution.ru/?feed=atom")
			};
		var reader = AutofacScope.Resolve<FeedReader>(new TypedParameter(typeof(List<NewsFeed>), feeds));
		reader.LoadReadFeed();
		var newsmenuModel = new QS.NewsFeed.ViewModels.NewsMenuViewModel(reader);
		var newsmenu = new NewsMenuView(newsmenuModel);
		menubar1.Add(newsmenu);
		newsmenuModel.LoadFeed();

		ReadUserSettings();

		var EntityAutocompleteSelector = new JournalViewModelAutocompleteSelector<EmployeeCard, EmployeeJournalViewModel>(UoW, AutofacScope);
		entitySearchEmployee.ViewModel = new EntitySearchViewModel<EmployeeCard>(EntityAutocompleteSelector);
		entitySearchEmployee.ViewModel.EntitySelected += SearchEmployee_EntitySelected;

		NavigationManager = AutofacScope.Resolve<TdiNavigationManager>(new TypedParameter(typeof(TdiNotebook), tdiMain));
		tdiMain.WidgetResolver = AutofacScope.Resolve<ITDIWidgetResolver>(new TypedParameter(typeof(Assembly[]), new[] { Assembly.GetAssembly(typeof(OrganizationViewModel)) }));
		NavigationManager.ViewModelOpened += NavigationManager_ViewModelOpened;

		#region Проверки и исправления базы
		//Если склады отсутствуют создаём новый, так как для версий ниже предприятия пользователь его создать не сможет.
		if(UoW.GetAll<Warehouse>().Count() == 0)
			CreateDefaultWarehouse();
		//Если у базы еще нет Guid создаем его.
		using(var localScope = MainClass.AppDIContainer.BeginLifetimeScope()) {
			var baseParam = localScope.Resolve<BaseParameters>();
			if(baseParam.Dynamic.BaseGuid == null)
				baseParam.Dynamic.BaseGuid = Guid.NewGuid();
		}
		#endregion

		FeaturesService = AutofacScope.Resolve<FeaturesService>();
		DisableFeatures();
	}

	private void CreateDefaultWarehouse()
	{
		Warehouse warehouse = new Warehouse();
		warehouse.Name = "Основной склад";
		UoW.Session.Save(warehouse);
	}

	void NavigationManager_ViewModelOpened(object sender, ViewModelOpenedEventArgs e)
	{
		if(e.ViewModel != null)
			MainTelemetry.AddCount(e.ViewModel.GetType().Name.Replace("ViewModel", ""));
	}

	#region Workwear featrures
	private void DisableFeatures()
	{
		ActionWarehouse.Visible = FeaturesService.Available(WorkwearFeature.Warehouses);
		ActionCardIssuee.Visible = FeaturesService.Available(WorkwearFeature.IdentityCards);
		ActionImport.Visible = FeaturesService.Available(WorkwearFeature.LoadExcel);
		ActionBatchProcessing.Visible = FeaturesService.Available(WorkwearFeature.BatchProcessing);
		ActionConversatoins.Visible = FeaturesService.Available(WorkwearFeature.Communications);
		ActionNotificationTemplates.Visible = FeaturesService.Available(WorkwearFeature.Communications);
	}
	#endregion

	void SearchEmployee_EntitySelected(object sender, EntitySelectedEventArgs e)
	{
		MainTelemetry.AddCount("SearchEmployeeToolBar");
		var id = DomainHelper.GetId(e.Entity);
		DialogViewModelBase after = null;
		if(cnbOpenInWindow.Active) {
			IPage replaced;
			if(NavigationManager.CurrentPage?.ViewModel is EmployeeViewModel)
				replaced = NavigationManager.CurrentPage;
			else {
				replaced = NavigationManager.IndependentPages.Reverse().FirstOrDefault(x => x.ViewModel is EmployeeViewModel);
			}

			if(replaced != null) {
				IPage last = null;
				foreach(var page in NavigationManager.TopLevelPages) {
					if(page == replaced) {
						after = last?.ViewModel;
						break;
					}
					last = page;
				}
				NavigationManager.AskClosePage(replaced);
			}
		}
		NavigationManager.OpenViewModel<EmployeeViewModel, IEntityUoWBuilder>(after, EntityUoWBuilder.ForOpen(id));
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		a.RetVal = true;
		Application.Quit();
	}

	public override void Destroy()
	{
		AutofacScope.Dispose();
		UoW.Dispose();
		base.Destroy();
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
		tdiMain.OpenTab(OrmReference.GenerateHashName<MeasurementUnits>(),
						() => new OrmReference(typeof(MeasurementUnits))
					   );
	}

	protected void OnAction8Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<PostJournalViewModel>(null);
	}

	protected void OnAction9Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<LeadersJournalViewModel>(null);
	}

	protected void OnAction5Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ItemsTypeJournalViewModel>(null);
	}

	protected void OnAction6Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<NomenclatureJournalViewModel>(null);
	}

	protected void OnAboutActionActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("RunAboutDialog");
		using(var local = AutofacScope.BeginLifetimeScope()) {
			var about = local.Resolve<AboutView>();
			about.Run();
			about.Destroy();
		}
	}

	protected void OnAction11Activated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("ReportStockAllWear");
		workwear.ReportsDlg.StockAllWearDlg stockAllWearDlg = new workwear.ReportsDlg.StockAllWearDlg();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(stockAllWearDlg),
			() => new QSReport.ReportViewDlg(stockAllWearDlg)
		);
	}

	protected void OnAction10Activated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("ReportWearStatement");
		WearStatement widget = new WearStatement();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnAction12Activated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("ReportListBySize");
		var reportInfo = new ReportInfo {
			Title = "Список по размерам",
			Identifier = "ListBySize",
		};

		tdiMain.OpenTab(QSReport.ReportViewDlg.GenerateHashName(reportInfo),
						  () => new QSReport.ReportViewDlg(reportInfo)
						 );

	}

	protected void OnHelpActionActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("OpenUserGuide");
		try {
			System.Diagnostics.Process.Start("user-guide.pdf");
		}
		catch(System.ComponentModel.Win32Exception ex) {
			AutofacScope.Resolve<IInteractiveMessage>().ShowMessage(ImportanceLevel.Error,
			$"При открытии PDF файла с документацией произошла ошибка:\n{ex.Message}\n" +
				"Возможно на компьютере не установлена или неисправна программа для открытия PDF");
		}
	}

	protected void OnActionHistoryActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("RunChangeLogDlg");
		QSMain.RunChangeLogDlg(this);
	}

	protected void OnActionUpdateActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("CheckUpdate");
		using(var scope = MainClass.AppDIContainer.BeginLifetimeScope()) {
			var updater = scope.Resolve<ApplicationUpdater>();
			updater.StartCheckUpdate(UpdaterFlags.ShowAnyway, scope);
		}
	}

	protected void OnActionSNActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("EditSerialNumber");
		NavigationManager.OpenViewModel<SerialNumberViewModel>(null);
	}

	protected void OnActionNormsActivated(object sender, EventArgs e)
	{
		var page = NavigationManager.OpenViewModelNamedArgs<NormJournalViewModel>(null, new Dictionary<string, object> {{ "useMultiSelect", true }});
	}

	protected void OnActionConditionNormsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<NormConditionJournalViewModel>(null);
	}

	protected void OnActionNotIssuedSheetDetailActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(NotIssuedSheetViewModel));
	}

	protected void OnActionNotIssuedSheetSummaryActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(NotIssuedSheetSummaryViewModel));
	}

	protected void OnActionYearRequestSheetActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(AverageAnnualNeedViewModel));
	}

	protected void OnActionStockBalanceActivated(object sender, EventArgs e)
	{
		var page = NavigationManager.OpenViewModel<StockBalanceJournalViewModel>(null);
		page.ViewModel.ShowSummary = true;
		page.ViewModel.Filter.ShowNegativeBalance = true;
		page.ViewModel.Filter.Warehouse = new StockRepository().GetDefaultWarehouse(UoW, FeaturesService, AutofacScope.Resolve<IUserService>().CurrentUserId);
	}

	protected void OnActionStockDocsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<StockDocumentsJournalViewModel>(null);
	}

	protected void OnActionEmployeesActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<EmployeeJournalViewModel>(null);
	}

	protected void OnActionObjectsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<SubdivisionJournalViewModel>(null);
	}

	#region Панель инструментов

	void ReadUserSettings()
	{
		switch(CurrentUserSettings.Settings.ToolbarStyle) {
			case ToolbarStyle.Both:
				ActionToolBarTextAndIcon.Activate();
				break;
			case ToolbarStyle.Icons:
				ActionToolBarIconOnly.Activate();
				break;
			case ToolbarStyle.Text:
				ActionToolBarTextOnly.Activate();
				break;
		}
		switch(CurrentUserSettings.Settings.ToolBarIconsSize) {
			case IconsSize.ExtraSmall:
				ActionIconsExtraSmall.Activate();
				break;
			case IconsSize.Small:
				ActionIconsSmall.Activate();
				break;
			case IconsSize.Middle:
				ActionIconsMiddle.Activate();
				break;
			case IconsSize.Large:
				ActionIconsLarge.Activate();
				break;
		}
		ActionShowBar.Active = CurrentUserSettings.Settings.ShowToolbar;
		if(CurrentUserSettings.Settings.MaximizeOnStart)
			Maximize();
		ActionMaxizizeOnStart.Active = CurrentUserSettings.Settings.MaximizeOnStart;
	}

	private void ToolBarMode(ToolbarStyle style)
	{
		if(CurrentUserSettings.Settings.ToolbarStyle != style) {
			CurrentUserSettings.Settings.ToolbarStyle = style;
			CurrentUserSettings.SaveSettings();
		}
		toolbarMain.ToolbarStyle = style;
		ActionIconsExtraSmall.Sensitive = ActionIconsSmall.Sensitive = ActionIconsMiddle.Sensitive = ActionIconsLarge.Sensitive =
			style != ToolbarStyle.Text;
	}

	private void ToolBarShow(bool show)
	{
		if(toolbarMain.Visible == show)
			return;

		if(CurrentUserSettings.Settings.ShowToolbar != show) {
			CurrentUserSettings.Settings.ShowToolbar = show;
			CurrentUserSettings.SaveSettings();
		}
		toolbarMain.Visible = show;
		ActionIconsExtraSmall.Sensitive = ActionIconsSmall.Sensitive = ActionIconsMiddle.Sensitive = ActionIconsLarge.Sensitive =
			ActionToolBarIconOnly.Sensitive = ActionToolBarTextOnly.Sensitive = ActionToolBarTextAndIcon.Sensitive =
			show;
	}

	private void ToolBarMode(IconsSize size)
	{
		if(CurrentUserSettings.Settings.ToolBarIconsSize != size) {
			CurrentUserSettings.Settings.ToolBarIconsSize = size;
			CurrentUserSettings.SaveSettings();
		}
		switch(size) {
			case IconsSize.ExtraSmall:
				toolbarMain.IconSize = IconSize.SmallToolbar;
				break;
			case IconsSize.Small:
				toolbarMain.IconSize = IconSize.LargeToolbar;
				break;
			case IconsSize.Middle:
				toolbarMain.IconSize = IconSize.Dnd;
				break;
			case IconsSize.Large:
				toolbarMain.IconSize = IconSize.Dialog;
				break;
		}
	}

	protected void OnActionToolBarTextOnlyToggled(object sender, EventArgs e)
	{
		if(ActionToolBarTextOnly.Active)
			ToolBarMode(ToolbarStyle.Text);
	}

	protected void OnActionToolBarIconOnlyToggled(object sender, EventArgs e)
	{
		if(ActionToolBarIconOnly.Active)
			ToolBarMode(ToolbarStyle.Icons);
	}

	protected void OnActionToolBarTextAndIconToggled(object sender, EventArgs e)
	{
		if(ActionToolBarTextAndIcon.Active)
			ToolBarMode(ToolbarStyle.Both);
	}

	protected void OnActionIconsExtraSmallToggled(object sender, EventArgs e)
	{
		if(ActionIconsExtraSmall.Active)
			ToolBarMode(IconsSize.ExtraSmall);
	}

	protected void OnActionIconsSmallToggled(object sender, EventArgs e)
	{
		if(ActionIconsSmall.Active)
			ToolBarMode(IconsSize.Small);
	}

	protected void OnActionIconsMiddleToggled(object sender, EventArgs e)
	{
		if(ActionIconsMiddle.Active)
			ToolBarMode(IconsSize.Middle);
	}

	protected void OnActionIconsLargeToggled(object sender, EventArgs e)
	{
		if(ActionIconsLarge.Active)
			ToolBarMode(IconsSize.Large);
	}

	protected void OnActionShowBarToggled(object sender, EventArgs e)
	{
		ToolBarShow(ActionShowBar.Active);
	}

	protected void OnActionBarObjectsActivated(object sender, EventArgs e)
	{
		ActionObjects.Activate();
	}

	protected void OnActionBarEmployeesActivated(object sender, EventArgs e)
	{
		ActionEmployees.Activate();
	}

	protected void OnActionBarStoreActivated(object sender, EventArgs e)
	{
		ActionStockDocs.Activate();
	}

	protected void OnActionBarStoreBalanceActivated(object sender, EventArgs e)
	{
		ActionStockBalance.Activate();
	}

	#endregion

	protected void OnActionSiteActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("OpenSite");
		System.Diagnostics.Process.Start("http://workwear.qsolution.ru/?utm_source=qs&utm_medium=app_workwear&utm_campaign=help_open_site");
	}

	protected void OnActionOpenReformalActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("OpenReformal.ru");
		System.Diagnostics.Process.Start("http://qs-workwear.reformal.ru/");
	}

	protected void OnActionVKActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("vk.com");
		System.Diagnostics.Process.Start("https://vk.com/qualitysolution");
	}

	protected void OnActionOdnoklasnikiActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("ok.ru");
		System.Diagnostics.Process.Start("https://ok.ru/qualitysolution");
	}

	protected void OnActionTwitterActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("twitter.com");
		System.Diagnostics.Process.Start("https://twitter.com/QSolutionRu");
	}

	protected void OnActionYouTubeActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("youtube.com");
		System.Diagnostics.Process.Start("https://www.youtube.com/channel/UC4U9Rzp-yfRgWd2R0f4iIGg");
	}

	protected void OnActionRegulationDocActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("RegulationDoc");
		tdiMain.OpenTab(OrmReference.GenerateHashName<RegulationDoc>(),
						() => new OrmReference(typeof(RegulationDoc))
			   );
	}

	protected void OnActionBaseSettingsActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("DataBaseSettings");
		NavigationManager.OpenViewModel<DataBaseSettingsViewModel>(null);
	}

	protected void OnActionVacationTypesActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("VacationType");
		tdiMain.OpenTab<OrmReference, Type>(typeof(VacationType));
	}

	protected void OnActionOrganizationsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<OrganizationJournalViewModel>(null);
	}

	protected void OnActionIssuanceSheetsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<IssuanceSheetJournalViewModel>(null);
	}

	protected void OnActionWarehouseActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<WarehouseJournalViewModel>(null);
	}

	protected void OnActionDepartmentActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<DepartmentJournalViewModel>(null);
	}

	protected void OnActionProfessionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ProfessionJournalViewModel>(null);
	}

	protected void OnActionProtectionToolsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ProtectionToolsJournalViewModel>(null);
	}

	protected void OnActionAmountEmployeeGetWearActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("ReportAmountEmployeeGetWear");
		AmountEmployeeGetWearDlg widget = new AmountEmployeeGetWearDlg("AmountEmployeeGetWear", "Количество сотрудников получивших СИЗ");
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnActionAmountIssuedWearActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(AmountIssuedWearViewModel));
	}

	protected void OnActionUserSettingsActivated(object sender, EventArgs e)
	{
		int idSetting;
		using(IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot()) {
			var user = AutofacScope.Resolve<IUserService>();
			idSetting = uow.Session.QueryOver<UserSettings>()
			.Where(x => x.User.Id == user.CurrentUserId)
			.Select(x => x.Id)
			.SingleOrDefault<int>();
		}
		MainClass.MainWin.NavigationManager.OpenViewModel<UserSettingsViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForOpen(idSetting));
	}

	protected void OnActionCardIssueeActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<IssueByIdentifierViewModel>(null);
	}

	protected void OnActionEmployeeLoadActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ExcelImportViewModel>(null,
			addingRegistrations: c => c.RegisterType<ImportModelEmployee>().As<IImportModel>());
	}

	protected void OnActionNormsLoadActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ExcelImportViewModel>(null,
			addingRegistrations: c => c.RegisterType<ImportModelNorm>().As<IImportModel>());

	}

	protected void OnActionSetNormsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<EmployeeProcessingJournalViewModel>(null);
	}

	protected void OnActionEditNotificationTemplateActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<MessageTemplateJournalViewModel>(null);
	}

	protected void OnActionImportWorkwearItemsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ExcelImportViewModel>(null,
			addingRegistrations: c => c.RegisterType<ImportModelWorkwearItems>().As<IImportModel>());
	}

	protected void OnActionMaxizizeOnStartToggled(object sender, EventArgs e)
	{
		if(CurrentUserSettings.Settings.MaximizeOnStart != ActionMaxizizeOnStart.Active) {
			CurrentUserSettings.Settings.MaximizeOnStart = ActionMaxizizeOnStart.Active;
			CurrentUserSettings.SaveSettings();
		}
	}

	protected void OnActionPayActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("pay.qsolution.ru");
		System.Diagnostics.Process.Start("http://pay.qsolution.ru/");
	}

	protected void OnActionRequestSheetActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(RequestSheetViewModel));
	}

	protected void OnActionAdminGuideActivated(object sender, EventArgs e)
	{
		MainTelemetry.AddCount("OpenAdminGuide");
		try {
			System.Diagnostics.Process.Start("admin-guide.pdf");
		}
		catch(System.ComponentModel.Win32Exception ex) {
			AutofacScope.Resolve<IInteractiveMessage>().ShowMessage(ImportanceLevel.Error,
			$"При открытии PDF файла с документацией произошла ошибка:\n{ex.Message}\n" +
				"Возможно на компьютере не установлена или неисправна программа для открытия PDF");
		}
	}

	protected void OnActionReplaceEntityActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ReplaceEntityViewModel>(null);
	}

	protected void OnActionStockMovementsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<StockMovmentsJournalViewModel>(null);
	}

	protected void OnActionConversatoinsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<EmployeeNotificationJournalViewModel>(null);
	}
}
