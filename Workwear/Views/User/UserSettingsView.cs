using Gamma.Binding.Converters;
using NLog;
using QS.Views.Dialog;
using workwear.Domain.Stock;
using workwear.Domain.Users;
using workwear.Measurements;
using workwear.ViewModels.User;
using workwear.ViewModels.Stock;

namespace workwear.Views.User
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UserSettingsView : EntityDialogViewBase<UserSettingsViewModel, UserSettings>
	{
		public UserSettingsView(UserSettingsViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}

	}
}
