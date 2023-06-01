using System;
using QS.Views;
using Workwear.ViewModels.Import;

namespace Workwear.Views.Import {
	public partial class SettingsNormsView : ViewBase<SettingsNormsViewModel> {
		public SettingsNormsView(SettingsNormsViewModel viewModel) : base(viewModel) {
			this.Build();

			entrySeparator.Binding.AddBinding(ViewModel, v => v.ListSeparator, w => w.Text).InitializeFromSource();

			checkWearoutToName.Binding.AddBinding(ViewModel, v => v.WearoutToName, w => w.Active).InitializeFromSource();
		}
	}
}
