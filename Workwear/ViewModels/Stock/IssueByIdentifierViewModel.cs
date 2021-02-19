using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
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
			if(rusGuardService != null) {
				rusGuardService.RefreshDevices();
			}
			else {
				CurrentState = "Библиотека RusGuard не загружена";
				CurrentStateColor = "Orange Red";
			}
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
		#endregion
		#endregion
	}
}
