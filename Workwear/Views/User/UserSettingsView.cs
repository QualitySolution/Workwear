using QS.Views.Dialog;
using Workwear.Domain.Users;
using Workwear.ViewModels.User;

namespace workwear.Views.User
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UserSettingsView : EntityDialogViewBase<UserSettingsViewModel, UserSettings>
	{
		public UserSettingsView(UserSettingsViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			entityentryWarehouse.ViewModel = ViewModel.WarehouseFromEntryViewModel;
			entityentryLeader.ViewModel = ViewModel.LeaderFromEntryViewModel;
			entityentryOrganization.ViewModel = ViewModel.OrganizationFromEntryViewModel;
			entityentryResponsiblePerson.ViewModel = ViewModel.ResponsiblePersonFromEntryViewModel;

			label13.Visible = entityentryWarehouse.Visible = ViewModel.VisibleWarehouse;
		}

	}
}
