using System;
using System.Collections.Generic;
using System.ComponentModel;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using RglibInterop;
using workwear.Tools.IdentityCards;

namespace workwear.ViewModels.Stock
{
	public class IssueByIdentifierViewModel : WindowDialogViewModelBase
	{
		private readonly RusGuardService rusGuardService;

		public IssueByIdentifierViewModel(IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, RusGuardService rusGuardService = null) : base(navigation)
		{
			this.rusGuardService = rusGuardService;
			IsModal = false;
			Title = "Выдача по картам СКУД";
			if(rusGuardService != null) {
				rusGuardService.RefreshDevices();
			}
			else {
				CurrentState = "Библиотека RusGuard не загружена";
				CurrentStateColor = "Orange Red";
			}

			CardFamilies.ListChanged += CardFamilies_ListChanged;
		}

		void CardFamilies_ListChanged(object sender, ListChangedEventArgs e)
		{
			if(rusGuardService != null && SelectedDevice != null)
				rusGuardService.SetCardMask(SelectedDevice, CardFamilies);
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

		#region Настройки
		public List<DeviceInfo> Devices => rusGuardService?.Devices;

		private DeviceInfo selectedDevice;
		public virtual DeviceInfo SelectedDevice {
			get => selectedDevice;
			set => SetField(ref selectedDevice, value);
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
}
}
