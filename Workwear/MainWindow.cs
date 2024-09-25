using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Autofac;
using Gtk;
using MySqlConnector;
using NLog;
using QS.BusinessCommon.Domain;
using QS.Configuration;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.ErrorReporting;
using QS.HistoryLog.ViewModels;
using QS.HistoryLog;
using QS.Navigation;
using QS.NewsFeed.Views;
using QS.NewsFeed;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Project.Versioning;
using QS.Project.Views;
using QS.Report.ViewModels;
using QS.Serial.ViewModels;
using QS.Services;
using QS.Tdi.Gtk;
using QS.Tdi;
using QS.Updater.App;
using QS.Updater;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Control.ESVM;
using QSOrmProject;
using QSProjectsLib;
using QSTelemetry;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Domain.Users;
using Workwear.Models.Import.Employees;
using Workwear.Models.Import.Issuance;
using Workwear.Models.Import.Norms;
using Workwear.Models.Import;
using Workwear.Repository.Stock;
using Workwear.Tools.Features;
using Workwear.Tools.User;
using Workwear.Tools;
using Workwear.ViewModels.Communications;
using Workwear.ViewModels.Company;
using Workwear.ViewModels.Import;
using Workwear.ViewModels.Stock;
using Workwear.ViewModels.Tools;
using Workwear.ViewModels.User;
using workwear.Journal.ViewModels.ClothingService;
using workwear.Journal.ViewModels.Communications;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Postomats;
using workwear.Journal.ViewModels.Regulations;
using workwear.Journal.ViewModels.Statements;
using workwear.Journal.ViewModels.Stock;
using workwear.Journal.ViewModels.Tools;
using workwear.Models.WearLk;
using workwear.ReportParameters.ViewModels;
using Workwear.ReportParameters.ViewModels;
using workwear.ReportsDlg;
using workwear;
using Workwear;
using Workwear.Journal.ViewModels.Analytics;
using Workwear.Repository.Company;
using Workwear.ViewModels.Export;
using CurrencyWorks = QS.Utilities.CurrencyWorks;
using Workwear.ViewModels.Analytics;

public partial class MainWindow : Gtk.Window {
	private static Logger logger = LogManager.GetCurrentClassLogger();

	private ILifetimeScope AutofacScope;
	public TdiNavigationManager NavigationManager;
	public IProgressBarDisplayable ProgressBar;
	public IUnitOfWork UoW = UnitOfWorkFactory.CreateWithoutRoot();
	public readonly CurrentUserSettings CurrentUserSettings;
	public readonly IApplicationQuitService quitService;
	public readonly IInteractiveService interactive;
	public readonly IGuiDispatcher dispatcher;
	
	public FeaturesService FeaturesService { get; private set; }
	
	public MainWindow(UnhandledExceptionHandler unhandledExceptionHandler, bool isDemo) : base(Gtk.WindowType.Toplevel) {
		Build();
		ProgressBar = progresswidget1;
		var progress = new ProgressPerformanceHelper(ProgressBar, 34, "Подготовка статусной строк", logger, showProgressText: true);
		//Передаем лебл
		QSMain.StatusBarLabel = labelStatus;
		QSMain.MakeNewStatusTargetForNlog();
		
		progress.StartGroup("Настройка базы");
		MainClass.CreateBaseConfig (progress);
		progress.EndGroup();
		progress.CheckPoint("Конфигурация классов приложения");
		MainClass.AppDIContainer = MainClass.StartupContainer.BeginLifetimeScope(c => MainClass.AutofacClassConfig(c, isDemo));
		progress.CheckPoint("DI главного окна");
		AutofacScope = MainClass.AppDIContainer.BeginLifetimeScope();
		this.Title = AutofacScope.Resolve<IApplicationInfo>().ProductTitle;
		progress.StartGroup("Донастройка обработчика ошибок");
		unhandledExceptionHandler.UpdateDependencies(MainClass.AppDIContainer, progress);
		progress.EndGroup();
		progress.CheckPoint("Инициализация глобального обработчика");
		BusinessLogicGlobalEventHandler.Init(MainClass.AppDIContainer);

		progress.CheckPoint("Проверка кодировки SQL сервера");
		QSMain.CheckServer(this); // Проверяем настройки сервера

		progress.CheckPoint("Подготовка менеджера вкладок");
		NavigationManager = AutofacScope.Resolve<TdiNavigationManager>(new TypedParameter(typeof(TdiNotebook), tdiMain));
		tdiMain.WidgetResolver = AutofacScope.Resolve<ITDIWidgetResolver>(new TypedParameter(typeof(Assembly[]), new[] { Assembly.GetAssembly(typeof(OrganizationViewModel)) }));
		interactive = AutofacScope.Resolve<IInteractiveService>();
		quitService = AutofacScope.Resolve<IApplicationQuitService>();
		dispatcher = AutofacScope.Resolve<IGuiDispatcher>();
		FeaturesService = AutofacScope.Resolve<FeaturesService>();

		progress.CheckPoint("Проверка обновлений");
		using(var updateScope = AutofacScope.BeginLifetimeScope()) {
			var checker = updateScope.Resolve<VersionCheckerService>();
			UpdateInfo? updateInfo = checker.RunUpdate();
			if (updateInfo?.Status == UpdateStatus.Error) 
			{
				interactive.ShowMessage(updateInfo.Value.ImportanceLevel, updateInfo.Value.Message, updateInfo.Value.Title);
				quitService.Quit();
				return;
			}
			
			if (updateInfo?.Status == UpdateStatus.ExternalError)
			{
				interactive.ShowMessage(updateInfo.Value.ImportanceLevel, updateInfo.Value.Message, updateInfo.Value.Title);
				if (!EnterNewSN()) 
				{
					quitService.Quit();
					return;
				}
			}
		}

		progress.CheckPoint("Проверка входа под root");
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

		progress.CheckPoint("Установка настроек пользователя");
		var userService = AutofacScope.Resolve<IUserService>();
		var user = userService.GetCurrentUser();
		var databaseInfo = AutofacScope.Resolve<IDataBaseInfo>();
		CurrentUserSettings = AutofacScope.Resolve<CurrentUserSettings>();
		CurrencyWorks.CurrencyShortName = AutofacScope.Resolve<BaseParameters>().UsedCurrency;
		
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
		progress.CheckPoint("Запускаем чтение новостей");
		var feeds = new List<NewsFeed>(){
			new NewsFeed("workwearsite", "Новости программы", "https://workwear.qsolution.ru/?feed=atom")
			};
		var reader = AutofacScope.Resolve<FeedReader>(new TypedParameter(typeof(List<NewsFeed>), feeds));
		reader.LoadReadFeed();
		var newsmenuModel = new QS.NewsFeed.ViewModels.NewsMenuViewModel(reader);
		var newsmenu = new NewsMenuView(newsmenuModel);
		menubar1.Add(newsmenu);
		newsmenuModel.LoadFeed();

		progress.CheckPoint("Настройка виджета поиска сотрудников");
		var EntityAutocompleteSelector = new JournalViewModelAutocompleteSelector<EmployeeCard, EmployeeJournalViewModel>(AutofacScope);
		entitySearchEmployee.ViewModel = new EntitySearchViewModel<EmployeeCard>(EntityAutocompleteSelector);
		entitySearchEmployee.ViewModel.EntitySelected += SearchEmployee_EntitySelected;

		NavigationManager = AutofacScope.Resolve<TdiNavigationManager>(new TypedParameter(typeof(TdiNotebook), tdiMain));
		tdiMain.WidgetResolver = AutofacScope.Resolve<ITDIWidgetResolver>();
		NavigationManager.ViewModelOpened += NavigationManager_ViewModelOpened;

		progress.CheckPoint("Проверка и исправления базы");
		#region Проверки и исправления базы
		//Если склады отсутствуют создаём новый, так как для версий ниже предприятия пользователь его создать не сможет.
		if(!UoW.GetAll<Warehouse>().Any())
			CreateDefaultWarehouse();
		using(var localScope = MainClass.AppDIContainer.BeginLifetimeScope()) {
			//Если у базы еще нет Guid создаем его.
			var baseParam = localScope.Resolve<BaseParameters>();
			if(baseParam.Dynamic.BaseGuid == null)
				baseParam.Dynamic.BaseGuid = Guid.NewGuid();
			
			//Уведомление о скором истечении срока действия серийного номера
			if (FeaturesService.ExpiryDate != null) 
			{
				if (FeaturesService.ExpiryDate < DateTime.Now) 
				{
					if (EnterNewSN())
					{
						quitService.Quit();
						return;
					}
				}
				else
				{
					int daysLeft = (FeaturesService.ExpiryDate.Value - DateTime.Now).Days;
					if (daysLeft < 14) 
					{
						interactive.ShowMessage(ImportanceLevel.Warning,
							$"Срок действия серийного номера истекает {FeaturesService.ExpiryDate.Value.ToString("d")}");
					}
				}
			}
			
			//Если доступна возможность использовать штрих коды, а префикс штрих кодов для базы не задан, создаем его.
			if(FeaturesService.Available(WorkwearFeature.Barcodes) && baseParam.Dynamic.BarcodePrefix == null) {
				var prefix = FeaturesService.ClientId % 1000 + 2000; //Оставляем последние 3 цифры кода клиента и добавляем их к 2000.
				logger.Info($"Создали префикс штрихкодов для базы: {prefix}");
				baseParam.Dynamic.BarcodePrefix = prefix;
			}
		}
		#endregion

		progress.CheckPoint("Отключение недоступных функций");
		DisableFeatures();
		if(FeaturesService.Available(WorkwearFeature.Claims)) {
			var button = toolbarMain.Children.FirstOrDefault(x => x.Action == ActionClaims);
			var counter = AutofacScope.Resolve<UnansweredClaimsCounter>(new TypedParameter(typeof(Gtk.ToolButton), button));
		}

		progress.CheckPoint("Включаем мониторинг изменений");
		HistoryMain.Enable(connectionBuilder);

		//Настраиваем каналы обновлений
		progress.CheckPoint("Настройка каналов обновления");
		using(var releaseScope = AutofacScope.BeginLifetimeScope()) {
			var appInfo = releaseScope.Resolve<IApplicationInfo>();
			if(appInfo.Modification == null) { //Пока не используем каналы для редакций
				var configuration = releaseScope.Resolve<IChangeableConfiguration>();
				var channel = configuration[$"AppUpdater:Channel"];
				if(channel == null) { //Устанавливаем значение по умолчанию. Необходимо поменять при уходе версии в Stable 
					channel = UpdateChannel.Current.ToString();
					configuration[$"AppUpdater:Channel"] = channel;
				}
				ActionChannelStable.Active = channel == UpdateChannel.Stable.ToString();
				ActionChannelCurrent.Active = channel == UpdateChannel.Current.ToString();
			}
			else {
				ActionUpdateChannel.Visible = false;
			}
		}
		
		progress.CheckPoint("Настройка удаления");
		//Настройка удаления
		Configure.ConfigureDeletion();
		
		progress.CheckPoint("Настройка панелей");
		ReadUserSettings();
		
		//Дополнительные параметры в телеметрию
		progress.CheckPoint("Запускаем телеметрию");
		#if !DEBUG
			//Инициализируем телеметрию
			using(var telemetryScope = AutofacScope.BeginLifetimeScope()) {
				var applicationInfo = telemetryScope.Resolve<IApplicationInfo>();
				var configuration = telemetryScope.Resolve<IChangeableConfiguration>();
				MainTelemetry.Product = applicationInfo.ProductName;
				MainTelemetry.Edition = applicationInfo.Modification;
				MainTelemetry.Version = applicationInfo.Version.ToString();
				MainTelemetry.IsDemo = databaseInfo.IsDemo;
				MainTelemetry.DoNotTrack = configuration["Application:DoNotTrack"] == "true";
				MainTelemetry.StartUpdateByTimer(600);
			}
		#else
			MainTelemetry.DoNotTrack = true;
		#endif
		if(!MainTelemetry.DoNotTrack)
			Task.Run(FillTelemetry);
		
		progress.CheckPoint("Запуск QS: Облако");
		QSSaaS.Session.StartSessionRefresh ();
		progress.End();
		logger.Info($"Заппуск за {progress.TotalTime.TotalSeconds} сек.");
	}

	void FillTelemetry() {
		logger.Debug("Собираем данные для телеметрии");
		using(var telemetryScope = AutofacScope.BeginLifetimeScope()) {
			var featureService = telemetryScope.Resolve<FeaturesService>();
			MainTelemetry.ProductEdition = featureService.ProductEdition;
			var uowFactory = telemetryScope.Resolve<IUnitOfWorkFactory>();
			using(var uow = uowFactory.CreateWithoutRoot("Сбор телеметрии")) {
				var employeeRepository = telemetryScope.Resolve<EmployeeRepository>();
				MainTelemetry.EmployeesCount = (uint)employeeRepository.ActiveEmployeesQuery(uow).RowCount();
			}
		}
		logger.Debug("Характеристики базы собраны");
	}
	
	private void CreateDefaultWarehouse() {
		Warehouse warehouse = new Warehouse();
		warehouse.Name = "Основной склад";
		UoW.Session.Save(warehouse);
	}

	private bool EnterNewSN() 
	{
		if (!interactive.Question($"Серийный номер недействителен.\nОткрыть окно для его обновления?\n\nПри отказе приложение будет закрыто.")) 
		{
			return false;
		}
					
		IPage<SerialNumberViewModel> page = NavigationManager.OpenViewModel<SerialNumberViewModel>(null);
		bool res = false;
		bool isClosed = false;
		page.PageClosed += (sender, closedArgs) => 
		{
			isClosed = true;
			if (closedArgs.CloseSource == CloseSource.Save) 
			{
				FeaturesService.UpdateSerialNumber();
				res = true;
			}
			else
			{
				res = false;
			}
		};

		dispatcher.WaitInMainLoop(() => isClosed);
		return res;
	}

	void NavigationManager_ViewModelOpened(object sender, ViewModelOpenedEventArgs e) {
		if(e.ViewModel != null)
			MainTelemetry.AddCount(e.ViewModel.GetType().Name.Replace("ViewModel", ""));
	}

	#region Workwear featrures
	private void DisableFeatures() {
		ActionBarcodeCompletenessReport.Visible = FeaturesService.Available(WorkwearFeature.Barcodes);
		ActionBarcodes.Visible = FeaturesService.Available(WorkwearFeature.Barcodes);
		ActionBatchProcessing.Visible = FeaturesService.Available(WorkwearFeature.BatchProcessing);
		ActionCardIssuee.Visible = FeaturesService.Available(WorkwearFeature.IdentityCards);
		ActionClaims.Visible = FeaturesService.Available(WorkwearFeature.Claims);
		ActionClothingService.Visible = FeaturesService.Available(WorkwearFeature.ClothingService);
		ActionClothingServiceReport.Visible = FeaturesService.Available(WorkwearFeature.ClothingService);
		ActionConditionNorm.Visible = FeaturesService.Available(WorkwearFeature.ConditionNorm);
		ActionConversatoins.Visible = FeaturesService.Available(WorkwearFeature.Communications);
		ActionCostCenter.Visible = FeaturesService.Available(WorkwearFeature.CostCenter);
		ActionEmployeeGroup.Visible = FeaturesService.Available(WorkwearFeature.EmployeeGroups);
		ActionExport.Visible = FeaturesService.Available(WorkwearFeature.ExportExcel);
		ActionFullnessPostomats.Visible = FeaturesService.Available(WorkwearFeature.Postomats);
		ActionHistoryLog.Visible = FeaturesService.Available(WorkwearFeature.HistoryLog);
		ActionImport.Visible = FeaturesService.Available(WorkwearFeature.LoadExcel);
		ActionIncomeLoad.Visible = FeaturesService.Available(WorkwearFeature.Exchange1C);
		ActionMenuClaims.Visible = FeaturesService.Available(WorkwearFeature.Claims);
		ActionMenuNotification.Visible = FeaturesService.Available(WorkwearFeature.Communications);
		ActionMenuRatings.Visible = FeaturesService.Available(WorkwearFeature.Ratings);
		ActionNotificationTemplates.Visible = FeaturesService.Available(WorkwearFeature.Communications);
		ActionOwner.Visible = FeaturesService.Available(WorkwearFeature.Owners);
		ActionPostomatDocs.Visible = FeaturesService.Available(WorkwearFeature.Postomats);
		ActionPostomatDocsWithdraw.Visible = FeaturesService.Available(WorkwearFeature.Postomats);
		ActionSpecCoinsBalance.Visible = FeaturesService.Available(WorkwearFeature.SpecCoinsLk);
		ActionWarehouse.Visible = FeaturesService.Available(WorkwearFeature.Warehouses);
		ActionWarehouseForecasting.Visible = FeaturesService.Available(WorkwearFeature.StockForecasting);

		ActionServices.Visible = FeaturesService.Available(WorkwearFeature.Communications)
						 || FeaturesService.Available(WorkwearFeature.Claims)
						 || FeaturesService.Available(WorkwearFeature.Ratings)
						 || FeaturesService.Available(WorkwearFeature.Postomats)
						 || FeaturesService.Available(WorkwearFeature.SpecCoinsLk);
	}
	#endregion

	#region Helpers
	void OpenUrl(string url) {
		//Здесь пробуем исправить ошибку 35026 на нашем багтрекере.
		//Предположил что проблема в этом https://github.com/dotnet/runtime/issues/28005
		//Но проверить действительно ли это так негде.
		ProcessStartInfo psi = new ProcessStartInfo {
			FileName = url,
			UseShellExecute = true
		};
		Process.Start(psi);
	}

	void SetChannel(UpdateChannel channel) {
		using(var releaseScope = AutofacScope.BeginLifetimeScope()) {
			var configuration = releaseScope.Resolve<IChangeableConfiguration>();
			configuration[$"AppUpdater:Channel"] = channel.ToString();
		}
	}
	#endregion

	void SearchEmployee_EntitySelected(object sender, EntitySelectedEventArgs e) {
		MainTelemetry.AddCount("SearchEmployeeToolBar");
		var id = DomainHelper.GetId(e.Entity);
		NavigationManager.OpenViewModel<EmployeeViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForOpen(id));
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a) {
		a.RetVal = true;
		quitService.Quit();
	}

	public override void Destroy() {
		AutofacScope.Dispose();
		UoW.Dispose();
		base.Destroy();
	}

	protected void OnDialogAuthenticationActionActivated(object sender, EventArgs e) {
		MainTelemetry.AddCount("ChangeUserPassword");
		QSMain.User.ChangeUserPassword(this);
	}

	protected void OnUsersActionActivated(object sender, EventArgs e) {
		MainTelemetry.AddCount("OpenUsers");
		Users winUsers = new Users();
		winUsers.Show();
		winUsers.Run();
		winUsers.Destroy();
	}

	protected void OnQuitActionActivated(object sender, EventArgs e) {
		quitService.Quit();
	}

	protected void OnAction7Activated(object sender, EventArgs e) {
		MainTelemetry.AddCount("MeasurementUnits");
		tdiMain.OpenTab(OrmReference.GenerateHashName<MeasurementUnits>(),
						() => new OrmReference(typeof(MeasurementUnits))
					   );
	}

	protected void OnAction8Activated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<PostJournalViewModel>(null);
	}

	protected void OnAction9Activated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<LeadersJournalViewModel>(null);
	}

	protected void OnAction5Activated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<ItemsTypeJournalViewModel>(null);
	}

	protected void OnAction6Activated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<NomenclatureJournalViewModel>(null);
	}

	protected void OnAboutActionActivated(object sender, EventArgs e) {
		MainTelemetry.AddCount("RunAboutDialog");
		using(var local = AutofacScope.BeginLifetimeScope()) {
			var about = local.Resolve<AboutView>();
			about.Run();
			about.Destroy();
		}
	}

	protected void OnAction11Activated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(StockAllWearViewModel));
	}

	protected void OnAction10Activated(object sender, EventArgs e) {
		MainTelemetry.AddCount("ReportWearStatement");
		WearStatement widget = new WearStatement();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnAction12Activated(object sender, EventArgs e) =>
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ListBySizeViewModel));

	protected void OnHelpActionActivated(object sender, EventArgs e) {
		MainTelemetry.AddCount("OpenUserGuide");
		try {
			OpenUrl("user-guide.pdf");
		}
		catch(System.ComponentModel.Win32Exception ex) {
			AutofacScope.Resolve<IInteractiveMessage>().ShowMessage(ImportanceLevel.Error,
			$"При открытии PDF файла с документацией произошла ошибка:\n{ex.Message}\n" +
				"Возможно на компьютере не установлена или неисправна программа для открытия PDF");
		}
	}

	protected void OnActionHistoryActivated(object sender, EventArgs e) {
		MainTelemetry.AddCount("RunChangeLogDlg");
		QSMain.RunChangeLogDlg(this);
	}

	protected void OnActionUpdateActivated(object sender, EventArgs e) {
		MainTelemetry.AddCount("CheckUpdate");
		using(var scope = MainClass.AppDIContainer.BeginLifetimeScope()) {
			var updater = scope.Resolve<IAppUpdater>();
			_ = updater.CheckUpdate(true);
		}
	}

	protected void OnActionSNActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<SerialNumberViewModel>(null);
	}

	protected void OnActionNormsActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<NormJournalViewModel>(null);
	}

	protected void OnActionConditionNormsActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<NormConditionJournalViewModel>(null);
	}

	protected void OnActionNotIssuedSheetDetailActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(NotIssuedSheetViewModel));
	}

	protected void OnActionNotIssuedSheetSummaryActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(NotIssuedSheetSummaryViewModel));
	}

	protected void OnActionYearRequestSheetActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(AverageAnnualNeedViewModel));
	}

	protected void OnActionStockBalanceActivated(object sender, EventArgs e) {
		var page = NavigationManager.OpenViewModel<StockBalanceJournalViewModel>(null);
		page.ViewModel.ShowSummary = true;
		page.ViewModel.Filter.ShowNegativeBalance = true;
		page.ViewModel.Filter.Warehouse = new StockRepository().GetDefaultWarehouse(UoW, FeaturesService, AutofacScope.Resolve<IUserService>().CurrentUserId);
	}

	protected void OnActionStockDocsActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<StockDocumentsJournalViewModel>(null);
	}

	protected void OnActionEmployeesActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<EmployeeJournalViewModel>(null);
	}

	protected void OnActionObjectsActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<SubdivisionJournalViewModel>(null);
	}

	#region Панель инструментов

	void ReadUserSettings() {
		switch(CurrentUserSettings.Settings.ToolbarStyle) {
			case ToolbarType.Both:
				ActionToolBarTextAndIcon.Activate();
				break;
			case ToolbarType.Icons:
				ActionToolBarIconOnly.Activate();
				break;
			case ToolbarType.Text:
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

	private void ToolBarMode(ToolbarType style) {
		if(CurrentUserSettings.Settings.ToolbarStyle != style) {
			CurrentUserSettings.Settings.ToolbarStyle = style;
			CurrentUserSettings.SaveSettings();
		}
		toolbarMain.ToolbarStyle = (ToolbarStyle)Enum.Parse(typeof(ToolbarStyle), style.ToString());
		ActionIconsExtraSmall.Sensitive = ActionIconsSmall.Sensitive = ActionIconsMiddle.Sensitive = ActionIconsLarge.Sensitive =
			style != ToolbarType.Text;
	}

	private void ToolBarShow(bool show) {
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

	private void ToolBarMode(IconsSize size) {
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

	protected void OnActionToolBarTextOnlyToggled(object sender, EventArgs e) {
		if(ActionToolBarTextOnly.Active)
			ToolBarMode(ToolbarType.Text);
	}

	protected void OnActionToolBarIconOnlyToggled(object sender, EventArgs e) {
		if(ActionToolBarIconOnly.Active)
			ToolBarMode(ToolbarType.Icons);
	}

	protected void OnActionToolBarTextAndIconToggled(object sender, EventArgs e) {
		if(ActionToolBarTextAndIcon.Active)
			ToolBarMode(ToolbarType.Both);
	}

	protected void OnActionIconsExtraSmallToggled(object sender, EventArgs e) {
		if(ActionIconsExtraSmall.Active)
			ToolBarMode(IconsSize.ExtraSmall);
	}

	protected void OnActionIconsSmallToggled(object sender, EventArgs e) {
		if(ActionIconsSmall.Active)
			ToolBarMode(IconsSize.Small);
	}

	protected void OnActionIconsMiddleToggled(object sender, EventArgs e) {
		if(ActionIconsMiddle.Active)
			ToolBarMode(IconsSize.Middle);
	}

	protected void OnActionIconsLargeToggled(object sender, EventArgs e) {
		if(ActionIconsLarge.Active)
			ToolBarMode(IconsSize.Large);
	}

	protected void OnActionShowBarToggled(object sender, EventArgs e) {
		ToolBarShow(ActionShowBar.Active);
	}

	protected void OnActionBarObjectsActivated(object sender, EventArgs e) {
		ActionObjects.Activate();
	}

	protected void OnActionBarEmployeesActivated(object sender, EventArgs e) {
		ActionEmployees.Activate();
	}

	protected void OnActionBarStoreActivated(object sender, EventArgs e) {
		ActionStockDocs.Activate();
	}

	protected void OnActionBarStoreBalanceActivated(object sender, EventArgs e) {
		ActionStockBalance.Activate();
	}

	#endregion

	protected void OnActionSiteActivated(object sender, EventArgs e) {
		MainTelemetry.AddCount("OpenSite");
		OpenUrl("https://workwear.qsolution.ru/?utm_source=qs&utm_medium=app_workwear&utm_campaign=help_open_site");
	}

	protected void OnActionRegulationDocActivated(object sender, EventArgs e) {
		MainTelemetry.AddCount("RegulationDoc");
		tdiMain.OpenTab(OrmReference.GenerateHashName<RegulationDoc>(),
						() => new OrmReference(typeof(RegulationDoc))
			   );
	}

	protected void OnActionBaseSettingsActivated(object sender, EventArgs e) {
		MainTelemetry.AddCount("DataBaseSettings");
		NavigationManager.OpenViewModel<DataBaseSettingsViewModel>(null);
	}

	protected void OnActionVacationTypesActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<VacationTypeJournalViewModel>(null);
	}

	protected void OnActionOrganizationsActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<OrganizationJournalViewModel>(null);
	}

	protected void OnActionIssuanceSheetsActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<IssuanceSheetJournalViewModel>(null);
	}

	protected void OnActionWarehouseActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<WarehouseJournalViewModel>(null);
	}

	protected void OnActionDepartmentActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<DepartmentJournalViewModel>(null);
	}

	protected void OnActionProfessionActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<ProfessionJournalViewModel>(null);
	}

	protected void OnActionProtectionToolsActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<ProtectionToolsJournalViewModel>(null);
	}

	protected void OnActionAmountEmployeeGetWearActivated(object sender, EventArgs e) {
		MainTelemetry.AddCount("ReportAmountEmployeeGetWear");
		AmountEmployeeGetWearDlg widget = new AmountEmployeeGetWearDlg("AmountEmployeeGetWear", "Количество сотрудников получивших СИЗ");
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName(widget),
			() => new QSReport.ReportViewDlg(widget)
		);
	}

	protected void OnActionAmountIssuedWearActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(AmountIssuedWearViewModel));
	}

	protected void OnActionUserSettingsActivated(object sender, EventArgs e) {
		int idSetting;
		using(IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot()) {
			var user = AutofacScope.Resolve<IUserService>();
			idSetting = uow.Session.QueryOver<UserSettings>()
			.Where(x => x.User.Id == user.CurrentUserId)
			.Select(x => x.Id)
			.SingleOrDefault<int>();
			if(idSetting == 0) {
				var s = new UserSettings(user.GetCurrentUser());
				uow.Save(s);
				uow.Commit();
				idSetting = s.Id;
			}
		}
		MainClass.MainWin.NavigationManager.OpenViewModel<UserSettingsViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForOpen(idSetting));
	}

	protected void OnActionCardIssueeActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<IssueByIdentifierViewModel>(null);
	}

	protected void OnActionEmployeeLoadActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<ExcelImportViewModel>(null,
			addingRegistrations: c => c.RegisterType<ImportModelEmployee>().As<IImportModel>());
	}

	protected void OnActionNormsLoadActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<ExcelImportViewModel>(null,
			addingRegistrations: c => c.RegisterType<ImportModelNorm>().As<IImportModel>());

	}

	protected void OnActionSetNormsActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<EmployeeProcessingJournalViewModel>(null);
	}

	protected void OnActionEditNotificationTemplateActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<MessageTemplateJournalViewModel>(null);
	}

	protected void OnActionImportWorkwearItemsActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<ExcelImportViewModel>(null,
			addingRegistrations: c => c.RegisterType<ImportModelWorkwearItems>().As<IImportModel>());
	}

	protected void OnActionMaxizizeOnStartToggled(object sender, EventArgs e) {
		if(CurrentUserSettings.Settings.MaximizeOnStart != ActionMaxizizeOnStart.Active) {
			CurrentUserSettings.Settings.MaximizeOnStart = ActionMaxizizeOnStart.Active;
			CurrentUserSettings.SaveSettings();
		}
	}

	protected void OnActionPayActivated(object sender, EventArgs e) {
		MainTelemetry.AddCount("pay.qsolution.ru");
		OpenUrl("https://pay.qsolution.ru/");
	}

	protected void OnActionRequestSheetActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(RequestSheetViewModel));
	}

	protected void OnActionAdminGuideActivated(object sender, EventArgs e) {
		MainTelemetry.AddCount("OpenAdminGuide");
		try {
			OpenUrl("admin-guide.pdf");
		}
		catch(System.ComponentModel.Win32Exception ex) {
			AutofacScope.Resolve<IInteractiveMessage>().ShowMessage(ImportanceLevel.Error,
			$"При открытии PDF файла с документацией произошла ошибка:\n{ex.Message}\n" +
				"Возможно на компьютере не установлена или неисправна программа для открытия PDF");
		}
	}

	protected void OnActionReplaceEntityActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<ReplaceEntityViewModel>(null);
	}

	protected void OnActionStockMovementsActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<StockMovmentsJournalViewModel>(null);
	}

	protected void OnActionConversatoinsActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<EmployeeNotificationJournalViewModel>(null);
	}

	protected void OnShowHistoryLogActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<HistoryViewModel>(null);
	}

	protected void OnActionSizeActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<SizeJournalViewModel>(null);
	}

	protected void OnActionSizeTypeActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<SizeTypeJournalViewModel>(null);
	}

	protected void OnActionClaimsActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<ClaimsViewModel>(null);
	}

	protected void OnActionIncomeLoadActivated(object sender, EventArgs e) =>
		NavigationManager.OpenViewModel<IncomeImportViewModel>(null);

	protected void OnActionMenuNotificationActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<EmployeeNotificationJournalViewModel>(null);
	}

	protected void OnActionMenuClaimsActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<ClaimsViewModel>(null);
	}

	protected void OnActionMenuRatingsActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<RatingsViewModel>(null);
	}

	protected void OnActionOwnerActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<OwnerJournalViewModel>(null);
	}

	protected void BarcodeActivated(object sender, EventArgs e) =>
		NavigationManager.OpenViewModel<BarcodeJournalViewModel>(null);

	protected void OnActionCostCenterActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<CostCenterJournalViewModel>(null);
	}

	protected void OnActionChannelCurrentToggled(object sender, EventArgs e) {
		if(ActionChannelCurrent.Active)
			SetChannel(UpdateChannel.Current);
	}

	protected void OnActionChannelStableToggled(object sender, EventArgs e) {
		if(ActionChannelStable.Active)
			SetChannel(UpdateChannel.Stable);
	}

	protected void OnActionEmployeeGroupActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<EmployeeGroupJournalViewModel>(null);
	}

	protected void OnActionPostomatDocsActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<PostomatDocumentsJournalViewModel>(null);
	}

	protected void OnActionClothingServiceActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<ClaimsJournalViewModel>(null);
	}

	protected void OnActionProvisionActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ProvisionReportViewModel));
	}

	protected void OnActionFullnessPostomatsActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<FullnessJournalViewModel>(null);
	}

	protected void OnFutureIssueActionActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<FutureIssueExportViewModel>(null);
	}

	protected void OnActionBarcodeCompletenessReportActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(BarcodeCompletenessReportViewModel));
	}

	protected void OnActionPostomatDocsWithdrawActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<PostomatDocumentsWithdrawJournalViewModel>(null);
	}

	protected void OnProtectionToolsCategoriesActivated(object sender, EventArgs e) 
	{
		NavigationManager.OpenViewModel<ProtectionToolsCategoryJournalViewModel>(null);
	}

	protected void OnActionClothingServiceReportActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ClothingServiceReportViewModel));
	}

	protected void OnActionSpecCoinsBalanceActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<SpecCoinsBalanceJournalViewModel>(null);
	}

	protected void OnActionWarehouseForecastingActivated(object sender, EventArgs e) {
		NavigationManager.OpenViewModel<WarehouseForecastingViewModel>(null);
	}
}
