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

		public IssueByIdentifierViewModel(IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, RusGuardService rusGuardService) : base(navigation)
		{
			this.rusGuardService = rusGuardService ?? throw new ArgumentNullException(nameof(rusGuardService));
			IsModal = false;
			rusGuardService.RefreshDevices();
		}

		private string cardId;
		public virtual string CardID {
			get => cardId;
			set => SetField(ref cardId, value);
		}

		public List<DeviceInfo> Devices => rusGuardService.Devices;

		private DeviceInfo selectedDevice;
		public virtual DeviceInfo SelectedDevice {
			get => selectedDevice;
			set => SetField(ref selectedDevice, value);
		}

	}
}
