using QS.Views;
using Workwear.Journal.Filter.ViewModels.Regulations;

namespace Workwear.Journal.Filter.Views.Regulations {
	public partial class DutyNormFilterView : ViewBase<DutyNormFilterViewModel>{
		public DutyNormFilterView(DutyNormFilterViewModel viewModel) : base(viewModel) {
			this.Build();
			yShowArchival.Binding.AddBinding(viewModel, vm => vm.ShowArchival, w => w.Active).InitializeFromSource();
		}
	}
}
