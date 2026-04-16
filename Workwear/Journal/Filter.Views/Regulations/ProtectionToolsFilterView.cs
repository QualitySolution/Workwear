using QS.Views;
using Workwear.Journal.Filter.ViewModels.Regulations;

namespace Workwear.Journal.Filter.Views.Regulations {
	public partial class ProtectionToolsFilterView : ViewBase<ProtectionToolsFilterViewModel>
	{
		public ProtectionToolsFilterView(ProtectionToolsFilterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ycheckOnlyDermal.Binding
				.AddBinding(viewModel, vm => vm.OnlyDermal, w => w.Active)
				.InitializeFromSource();
			yShowArchival.Binding
				.AddBinding(viewModel, vm=>vm.ShowArchival, w=>w.Active)
				.InitializeFromSource();
		}
	}
}
