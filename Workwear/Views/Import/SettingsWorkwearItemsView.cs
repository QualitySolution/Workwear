using QS.Views;
using Workwear.ViewModels.Import;

namespace Workwear.Views.Import
{
	public partial class SettingsWorkwearItemsView : ViewBase<SettingsWorkwearItemsViewModel>
	{
		public SettingsWorkwearItemsView(SettingsWorkwearItemsViewModel viewModel) : base(viewModel)
		{
			this.Build();
			checkConvertPersonnelNumber.Binding
				.AddBinding(ViewModel, v => v.ConvertPersonnelNumber, w => w.Active)
				.InitializeFromSource();
		}
	}
}
