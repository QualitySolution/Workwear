using QS.Views;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WearStatementView : ViewBase<WearStatementViewModel> {
		public WearStatementView(WearStatementViewModel viewModel) : base(viewModel) {
			this.Build();

			entitySubdivision.ViewModel = ViewModel.SubdivisionEntry;
			buttonRun.Binding.AddBinding(ViewModel, vm => vm.SensitiveLoad, w => w.Sensitive).InitializeFromSource();
			buttonRun.Clicked += (sender, args) => ViewModel.LoadReport();
		}
	}
}
