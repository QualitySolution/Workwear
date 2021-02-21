using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Utilities.Text;
using QS.ViewModels.Dialog;
using RglibInterop;
using workwear.Tools.IdentityCards;

namespace workwear.ViewModels.Stock
{
	public class IssueByIdentifierViewModel : WindowDialogViewModelBase
	{
		private readonly IGuiDispatcher guiDispatcher;
		private readonly RusGuardService rusGuardService;
		private TextSpinner textSpinner = new TextSpinner(new SpinnerTemplateDots());

		public IssueByIdentifierViewModel(IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, IGuiDispatcher guiDispatcher, RusGuardService rusGuardService = null) : base(navigation)
		{
			this.guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			this.rusGuardService = rusGuardService;
			IsModal = false;
			Title = "Выдача по картам СКУД";
			if(rusGuardService != null) {
				rusGuardService.RefreshDevices();
				rusGuardService.СardStatusRead += RusGuardService_СardStatusRead;
			}
			UpdateState();

			CardFamilies.ListChanged += CardFamilies_ListChanged;
		}

		void CardFamilies_ListChanged(object sender, ListChangedEventArgs e)
		{
			if(rusGuardService != null && SelectedDevice != null)
				rusGuardService.SetCardMask(SelectedDevice, CardFamilies);
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

		#region Свойства View
		private string CurrentState;
		public virtual string CurentState {
			get => CurrentState;
			set => SetField(ref CurrentState, value);
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
		public virtual string CardUid {
			get => cardUid;
			set => SetField(ref cardUid, value);
		}

		private bool noCard;
		public virtual bool NoCard {
			get => noCard;
			set => SetField(ref noCard, value);
		}

		#region Настройки
		public List<DeviceInfo> Devices => rusGuardService?.Devices;

		private DeviceInfo selectedDevice;
		public virtual DeviceInfo SelectedDevice {
			get => selectedDevice;
			set {
				if(selectedDevice != value && rusGuardService.IsAutoPoll)
					rusGuardService.StopAutoPoll();
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
			if(rusGuardService != null) {
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

			if(rusGuardService?.IsAutoPoll == true) {
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
			if(rusGuardService == null)
				return;
			if(SelectedDevice == null)
				return;
			if(!CardFamilies.Any(x => x.Active))
				return;

			rusGuardService.StartDevice(SelectedDevice);
			rusGuardService.StartAutoPoll(SelectedDevice);
		}

		#endregion
	}
}
