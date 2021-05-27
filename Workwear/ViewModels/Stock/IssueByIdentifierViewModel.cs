using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Timers;
using Autofac;
using QS.Configuration;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.Utilities;
using QS.Utilities.Text;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.Repository.Company;
using workwear.Repository.Stock;
using workwear.Tools;
using workwear.Tools.Features;
using workwear.Tools.IdentityCards;
using Workwear.Domain.Company;
using Workwear.Measurements;

namespace workwear.ViewModels.Stock
{
	public class IssueByIdentifierViewModel : WindowDialogViewModelBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IUnitOfWorkFactory unitOfWorkFactory;
		private readonly IGuiDispatcher guiDispatcher;
		private readonly IUserService userService;
		private readonly ILifetimeScope autofacScope;
		private readonly EmployeeRepository employeeRepository;
		private readonly IValidator validator;
		private readonly BaseParameters baseParameters;
		private readonly IInteractiveQuestion interactive;
		private readonly IChangeableConfiguration configuration;
		private readonly ICardReaderService cardReaderService;
		private TextSpinner textSpinner = new TextSpinner(new SpinnerTemplateAestheticScrolling());
		private readonly IUnitOfWork UowOfDialog;

		public IssueByIdentifierViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IGuiDispatcher guiDispatcher,
			IUserService userService,
			ILifetimeScope autofacScope,
			StockRepository stockRepository,
			EmployeeRepository employeeRepository,
			FeaturesService featuresService,
			IValidator validator,
			BaseParameters baseParameters,
			IInteractiveQuestion interactive,
			IChangeableConfiguration configuration,
			SizeService sizeService,
			ICardReaderService cardReaderService = null) : base(navigation)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			SizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			this.cardReaderService = cardReaderService;
			IsModal = false;
			EnableMinimizeMaximize = true;
			Title = "Выдача по картам СКУД";

			UowOfDialog = unitOfWorkFactory.CreateWithoutRoot();
			var entryBuilder = new CommonEEVMBuilderFactory<IssueByIdentifierViewModel>(this, this, UowOfDialog, navigation, autofacScope);

			if(cardReaderService != null) {
				cardReaderService.RefreshDevices();
				cardReaderService.СardStatusRead += RusGuardService_СardStatusRead;
				cardReaderService.CardFamilies.ListChanged += CardFamilies_ListChanged;
			}
			UpdateState();


			WarehouseEntryViewModel = entryBuilder.ForProperty(x => x.Warehouse).MakeByType().Finish();
			Warehouse = stockRepository.GetDefaultWarehouse(UowOfDialog, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);

			//Настройка таймера сброса
			timerCleanSuccessfullyText = new Timer(40000);
			timerCleanSuccessfullyText.AutoReset = false;
			timerCleanSuccessfullyText.Elapsed += delegate (object sender, ElapsedEventArgs e) { 
				guiDispatcher.RunInGuiTread(() => SuccessfullyText = null); 
			};

			ReadConfig();
		}

		#region Считыватель
		#region События
		void CardFamilies_ListChanged(object sender, ListChangedEventArgs e)
		{
			UpdateState();
			TryStartPoll();
			SetCardFamiliesConfig();
		}

		void RusGuardService_СardStatusRead(object sender, CardStateEventArgs e)
		{
			guiDispatcher.RunInGuiTread(delegate {
				CardUid = e.CardUid;
				UpdateState();
			});
		}
		#endregion

		#region Свойства View
		public SizeService SizeService { get; }

		private string currentState;
		public virtual string CurrentState {
			get => currentState;
			set => SetField(ref currentState, value);
		}

		private string currentStateColor;
		public virtual string CurrentStateColor {
			get => currentStateColor;
			set => SetField(ref currentStateColor, value);
		}

		private bool showSettings;
		public virtual bool ShowSettings {
			get => showSettings;
			set => SetField(ref showSettings, value);
		}

		private string cardUid;
		[PropertyChangedAlso(nameof(RecommendedActions))]
		[PropertyChangedAlso(nameof(VisibleRecommendedActions))]
		[PropertyChangedAlso(nameof(NoCard))]
		public virtual string CardUid {
			get => cardUid;
			set {
				if(SetField(ref cardUid, value)) {
					if(String.IsNullOrEmpty(CardUid))
						LastRemoveCard = DateTime.Now;
					NewCardUid();
					OnPropertyChanged(nameof(EmployeeFullName));
					CreateExpenseDoc();
				}
			}
		}

		public string CardUidСompact => CardUid.Replace("-", "");

		public bool NoCard => String.IsNullOrEmpty(CardUid);

		#region Настройки
		public List<DeviceInfo> Devices => cardReaderService?.Devices;

		public BindingList<CardType> CardFamilies => cardReaderService.CardFamilies;

		private DeviceInfo selectedDevice;
		public virtual DeviceInfo SelectedDevice {
			get => selectedDevice;
			set {
				if(selectedDevice != value && cardReaderService.IsAutoPoll)
					cardReaderService.StopAutoPoll();
				SetField(ref selectedDevice, value);
				UpdateState();
				if(SelectedDevice != null) {
					TryStartPoll();
					configuration["CardReader:Address"] = SelectedDevice.EndpointInfo.Address + ':' + SelectedDevice.DeviceInfoShort.DeviceAddress.ToString();
				}
			}
		}

		private Warehouse warehouse;
		public virtual Warehouse Warehouse {
			get => warehouse;
			set => SetField(ref warehouse, value);
		}

		public EntityEntryViewModel<Warehouse> WarehouseEntryViewModel;
		#endregion
		#endregion

		#region Внутренние методы
		void UpdateState()
		{
			if(cardReaderService == null) {
				CurrentState = "Библиотека RusGuard не загружена";
				CurrentStateColor = "Orange Red";
				return;
			}
			if(!Devices.Any()) {
				CurrentState = "Считыватели не подключены";
				CurrentStateColor = "Orange Red";
				return;
			}
			if(SelectedDevice == null) {
				CurrentState = "Необходимо выбрать считыватель";
				CurrentStateColor = "Cornflower Blue";
				return;
			}
			if(!cardReaderService.CardFamilies.Any(x => x.Active)) {
				CurrentState = "Не выбран тип карт";
				CurrentStateColor = "Cornflower Blue";
				return;
			}
			if(Warehouse == null) {
				CurrentState = "Не выбран склад выдачи";
				CurrentStateColor = "Cornflower Blue";
				return;
			}

			if(cardReaderService?.IsAutoPoll == true) {
				if(NoCard) {
					CurrentState = "Нет карты\t" + textSpinner.GetFrame();
					CurrentStateColor = "Dark Salmon";
					return;
				}
				if(!String.IsNullOrEmpty(CardUid)) {
					CurrentState = $"Карта: {CardUid}\t" + textSpinner.GetFrame();
					CurrentStateColor = "Dark Sea Green";
					return;
				}
			}

			CurrentState = "Считыватель готов";
			CurrentStateColor = "Lime Green";
		}

		void TryStartPoll()
		{
			if(cardReaderService == null)
				return;
			if(SelectedDevice == null)
				return;
			if(!cardReaderService.CardFamilies.Any(x => x.Active))
				return;

			cardReaderService.StartDevice(SelectedDevice);
			cardReaderService.StartAutoPoll(SelectedDevice);
			OnPropertyChanged(nameof(VisibleRecommendedActions));
		}

		private void ReadConfig()
		{
			var readerAddress = configuration["CardReader:Address"];
			if(!String.IsNullOrEmpty(readerAddress) && readerAddress.Contains(':')) {
				var parts = readerAddress.Split(':');
				SelectedDevice = Devices.First(x => x.Endpoint.Address == parts[0] && x.DeviceInfoShort.DeviceAddress == int.Parse(parts[1]));
			}

			var cardTypes = configuration["CardReader:CardTypes"];
			if(!String.IsNullOrEmpty(cardTypes)) {
				var parts = cardTypes.Split(',');
				foreach(var part in parts) {
					var family = cardReaderService.CardFamilies.FirstOrDefault(x => x.CardTypeFamily.ToString() == part);
					if(family != null)
						family.Active = true;
				}
			}
		}

		private void SetCardFamiliesConfig()
		{
			if(cardReaderService.CardFamilies.Any(x => x.Active))
				configuration["CardReader:CardTypes"] = String.Join(",", cardReaderService.CardFamilies.Where(x => x.Active).Select(x => x.CardTypeFamily.ToString()));
			else
				configuration["CardReader:CardTypes"] = null;
		}

		#endregion
		#endregion
		#region Выдача
		private DateTime LastRemoveCard;
		private IUnitOfWork uow;

		public virtual string EmployeeFullName {
			get => Employee?.FullName ?? (String.IsNullOrEmpty(CardUid) ? null : $"Сотрудник с картой: {CardUid} не найден");
		}

		public virtual bool CanAccept => Expense.Items.Any(x => x.Amount > 0);

		private bool canAcceptByTime => (DateTime.Now - LastRemoveCard).TotalSeconds > 3;

		public bool VisibleCancelButton => Employee != null;
		public bool VisibleRecommendedActions => cardReaderService?.IsAutoPoll == true && !String.IsNullOrEmpty(RecommendedActions);

		private Timer timerCleanSuccessfullyText;
		public bool VisibleSuccessfully => !String.IsNullOrEmpty(SuccessfullyText);

		private string successfullyText;
		[PropertyChangedAlso(nameof(VisibleSuccessfully))]
		public virtual string SuccessfullyText {
			get => successfullyText;
			set {
				if(SetField(ref successfullyText, value))
					timerCleanSuccessfullyText.Enabled = !String.IsNullOrEmpty(successfullyText);
			}
		}

		public string RecommendedActions {
			get {
				if(NoCard && Employee == null)
					return "Приложите карту для идентификации сотрудника";
				if(NoCard && CanAccept && canAcceptByTime)
					return "Приложите карту для подтверждения выдачи";
				if(!String.IsNullOrEmpty(CardUid) && Employee != null) {
					if(Employee.CardKey != CardUidСompact)
						return "Для подтверждения приложите карту " + Employee.ShortName;
					else
						return "Уберите карту и проверьте список выдаваемого";
				}
				if(!String.IsNullOrEmpty(CardUid) && Employee == null)
					return "Уберите карту";

				return String.Empty;
			}
		}

		private EmployeeCard employee;
		[PropertyChangedAlso(nameof(EmployeeFullName))]
		[PropertyChangedAlso(nameof(VisibleCancelButton))]
		public virtual EmployeeCard Employee {
			get => employee;
			set => SetField(ref employee, value);
		}

		private Expense expense;
		[PropertyChangedAlso(nameof(ObservableItems))]
		public virtual Expense Expense {
			get => expense;
			set => SetField(ref expense, value);
		}

		public GenericObservableList<ExpenseItem> ObservableItems => Expense?.ObservableItems;

		#region Вызовы View

		public void CleanEmployee()
		{
			Employee = null;
			Expense = null;
			if(uow != null) {
				uow.Dispose();
				uow = null;
			}
		}

		#endregion

		#region Внутрение Методы

		private void NewCardUid()
		{
			if(String.IsNullOrEmpty(CardUid))
				return;

			SuccessfullyText = null;

			if(Employee == null) {
				LoadEmployee();
			}
			else if(CanAccept && canAcceptByTime && Employee.CardKey == CardUidСompact)
				AcceptIssue();
		}

		private void LoadEmployee()
		{
			uow = unitOfWorkFactory.CreateWithoutRoot();
			Employee = employeeRepository.GetEmployeeByCardkey(uow, CardUidСompact);
			if(Employee == null) {
				uow.Dispose();
				uow = null;
			}
			SuccessfullyText = null;
		}

		private void CreateExpenseDoc()
		{
			if(Employee == null)
				return;
			Expense = new Expense();
			Expense.CreatedbyUser = userService.GetCurrentUser(uow);
			Expense.Operation = ExpenseOperations.Employee;
			Expense.Employee = Employee;
			Expense.Warehouse = Warehouse;
			Expense.ObservableItems.Clear();

			Employee.FillWearInStockInfo(uow, Warehouse, Expense.Date, onlyUnderreceived: false);
			foreach(var item in Employee.WorkwearItems) {
				Expense.AddItem(item);
			}
		}

		private void AcceptIssue()
		{
			if(!validator.Validate(Expense))
				return;

			Expense.CleanupItems();
			Expense.UpdateOperations(uow, baseParameters, interactive, CardUidСompact);
			uow.Save(Expense);

			logger.Debug("Обновляем записи о выданной одежде в карточке сотрудника...");
			Expense.UpdateEmployeeWearItems();
			uow.Commit();
			logger.Info($"Записан документ выдачи №{Expense.Id} на {Employee.ShortName}");
			SuccessfullyText = MakeConfirmText();
			CleanEmployee();
		}

		private string MakeConfirmText()
		{
			var text = Expense.Employee.ShortName;
			text += Expense.Employee.Sex == Sex.F ? " подтвердила " : " подтвердил ";
			text += $"выдачу №{Expense.Id} в количестве ";
			text += NumberToTextRus.FormatCase(Expense.Items.Sum(x => x.Amount), "{0} единицы", "{0} единиц", "{0} единиц");
			return text;
		}

		#endregion
		#endregion
	}
}
	