using QS.Views.Dialog;
using QS.Widgets;
using Workwear.Domain.Users;
using Workwear.ViewModels.User;

namespace Workwear.Views.User
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
			yentryByerEmail.ValidationMode = ValidationType.Email;
			yentryByerEmail.Binding
				.AddBinding(Entity, e => e.BuyerEmail, w => w.Text).InitializeFromSource();

			labelWarehouse.Visible = entityentryWarehouse.Visible = ViewModel.VisibleWarehouse;
			ylabelByerEmail.Visible = yentryByerEmail.Visible = ViewModel.VisibleByerEmail;
		}

	}
}
