using QS.Views;
using Workwear.ViewModels.Import;

namespace Workwear.Views.Import {
	public partial class SettingsNormsView : ViewBase<SettingsNormsViewModel> {
		public SettingsNormsView(SettingsNormsViewModel viewModel) : base(viewModel) {
			this.Build();

			entrySeparator.Binding.AddBinding(ViewModel, v => v.ListSeparator, w => w.Text).InitializeFromSource();

			checkWearoutToName.Binding.AddBinding(ViewModel, v => v.WearoutToName, w => w.Active).InitializeFromSource();
			
			checkSubdivisionLevelEnable.Binding
				.AddBinding(ViewModel, v => v.SubdivisionLevelEnable, w => w.Active)
				.InitializeFromSource();

			checkSubdivisionLevelReverse.Binding
				.AddSource(ViewModel)
				.AddBinding(v => v.SubdivisionLevelReverse, w => w.Active)
				.AddBinding(v => v.SubdivisionLevelEnable, w => w.Sensitive)
				.InitializeFromSource();

			entrySubdivisionLevelSeparator.Binding
				.AddSource(ViewModel)
				.AddBinding(v => v.SubdivisionLevelSeparator, w => w.Text)
				.AddBinding(v => v.SubdivisionLevelEnable, w => w.Sensitive)
				.InitializeFromSource();
		}
	}
}
