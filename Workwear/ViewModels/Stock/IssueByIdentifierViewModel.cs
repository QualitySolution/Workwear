using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Utilities.Text;
using QS.ViewModels.Dialog;
using RglibInterop;
using workwear.Domain.Company;
using workwear.Tools.IdentityCards;

namespace workwear.ViewModels.Stock
{
	public class IssueByIdentifierViewModel : WindowDialogViewModelBase
	{
		private readonly IUnitOfWorkFactory unitOfWorkFactory;
		private readonly IGuiDispatcher guiDispatcher;
		private readonly ICardReaderService cardReaderService;
		private TextSpinner textSpinner = new TextSpinner(new SpinnerTemplateDots());

		public IssueByIdentifierViewModel(IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, IGuiDispatcher guiDispatcher, ICardReaderService cardReaderService = null) : base(navigation)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			this.cardReaderService = cardReaderService;
			IsModal = false;
			EnableMinimizeMaximize = true;
			Title = "Выдача по картам СКУД";
			if(cardReaderService != null) {
				cardReaderService.RefreshDevices();
				cardReaderService.СardStatusRead += RusGuardService_СardStatusRead;
			}
			UpdateState();

			CardFamilies.ListChanged += CardFamilies_ListChanged;
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
					TimeOfSetUid = DateTime.Now;
					NewCardUid();
					OnPropertyChanged(nameof(EmployeeFullName));
				}
			}
		}

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
		private DateTime TimeOfSetUid;
		private IUnitOfWork uow;

		public virtual string EmployeeFullName {
			get => Employee?.FullName ?? (String.IsNullOrEmpty(CardUid) ? null : $"Сотрудник с картой: {CardUid} не найден");
		}

		private bool canAccept;
		public virtual bool CanAccept {
			get => canAccept;
			set => SetField(ref canAccept, value);
		}

		public bool VisibleCancelButton => Employee != null;
		public bool VisibleRecommendedActions => cardReaderService?.IsAutoPoll == true && !String.IsNullOrEmpty(RecommendedActions);

		public string RecommendedActions {
			get {
				if(NoCard && Employee == null)
					return "Приложите карту для идентификации сотрудника";
				if(NoCard && CanAccept)
					return "Приложите карту для подтверждения выдачи";
				if(!String.IsNullOrEmpty(CardUid) && Employee != null)
					return 	(DateTime.Now - TimeOfSetUid).TotalSeconds < 3 
					? "Уберите карту и проверьте список выдаваемого"
					: "Уберите карту";
				
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

		#region Вызовы View

		public void CleanEmployee()
		{
			Employee = null;
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

			if(Employee == null) {
				LoadEmployee();
			}
			else {
				if(CanAccept)
					AcceptIssue();
			}
		}

		private void LoadEmployee()
		{
			var cardUidstr = CardUid.Replace("-", "");
			uow = unitOfWorkFactory.CreateWithoutRoot();
			Employee = uow.Session.QueryOver<EmployeeCard>()
				.Where(x => x.CardKey == cardUidstr)
				.Take(1)
				.SingleOrDefault();
			if(Employee == null) {
				uow.Dispose();
				uow = null;
			}
		}

		private void AcceptIssue()
		{
		}

		#endregion
		#endregion
	}
}
