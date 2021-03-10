using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Timers;
using Autofac;
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
using RglibInterop;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.Repository.Company;
using workwear.Repository.Stock;
using workwear.Tools;
using workwear.Tools.Features;
using workwear.Tools.IdentityCards;

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
		private readonly ICardReaderService cardReaderService;
		private TextSpinner textSpinner = new TextSpinner(new SpinnerTemplateBouncingBar());
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
			this.cardReaderService = cardReaderService;
			IsModal = false;
			EnableMinimizeMaximize = true;
			Title = "Выдача по картам СКУД";

			UowOfDialog = unitOfWorkFactory.CreateWithoutRoot();
			var entryBuilder = new CommonEEVMBuilderFactory<IssueByIdentifierViewModel>(this, this, UowOfDialog, navigation, autofacScope);

			if(cardReaderService != null) {
				cardReaderService.RefreshDevices();
				cardReaderService.СardStatusRead += RusGuardService_СardStatusRead;
			}
			UpdateState();

			CardFamilies.ListChanged += CardFamilies_ListChanged;

			WarehouseEntryViewModel = entryBuilder.ForProperty(x => x.Warehouse).MakeByType().Finish();
			Warehouse = stockRepository.GetDefaultWarehouse(UowOfDialog, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);

			//Настройка таймера сброса
			timerCleanSuccessfullyText = new Timer(40000);
			timerCleanSuccessfullyText.AutoReset = false;
			timerCleanSuccessfullyText.Elapsed += delegate (object sender, ElapsedEventArgs e) { 
				guiDispatcher.RunInGuiTread(() => SuccessfullyText = null); 
			};
		}

		#region Считыватель
		#region События
		void CardFamilies_ListChanged(object sender, ListChangedEventArgs e)
		{
			if(cardReaderService != null && SelectedDevice != null)
				cardReaderService.SetCardMask(SelectedDevice, CardFamilies);
			UpdateState();
			TryStartPoll();
		}

		void RusGuardService_СardStatusRead(object sender, CardStateEventArgs e)
		{
			guiDispatcher.RunInGuiTread(delegate {;
				CardUid = e.CardUid;
				NoCard = !e.ReadBad && String.IsNullOrEmpty(e.CardUid);
				UpdateState();
			});
		}
		#endregion

		#region Свойства View
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

		private bool noCard;
		[PropertyChangedAlso(nameof(RecommendedActions))]
		[PropertyChangedAlso(nameof(VisibleRecommendedActions))]
		public virtual bool NoCard {
			get => noCard;
			set => SetField(ref noCard, value);
		}

		#region Настройки
		public List<DeviceInfo> Devices => cardReaderService?.Devices;

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
				}
			}
		}

		public BindingList<CardType> CardFamilies = new BindingList<CardType>() {
                new CardType(RG_CARD_FAMILY_CODE.CF_COTAG),
                new CardType(RG_CARD_FAMILY_CODE.CF_EMMARINE),
                new CardType(RG_CARD_FAMILY_CODE.CF_HID),
                new CardType(RG_CARD_FAMILY_CODE.CF_INDALA),
                new CardType(RG_CARD_FAMILY_CODE.CF_PINCODE),
                new CardType(RG_CARD_FAMILY_CODE.CF_TEMIC),
                new CardType(RG_CARD_FAMILY_CODE.EF_MIFARE)

			};

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
			if(!CardFamilies.Any(x => x.Active)) {
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
			if(!CardFamilies.Any(x => x.Active))
				return;

			cardReaderService.StartDevice(SelectedDevice);
			cardReaderService.StartAutoPoll(SelectedDevice);
			OnPropertyChanged(nameof(VisibleRecommendedActions));
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
	