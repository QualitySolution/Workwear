using QS.Views;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OverNormIssuedReportView : ViewBase<OverNormIssuedReportViewModel> {
		public OverNormIssuedReportView(OverNormIssuedReportViewModel viewModel) : base(viewModel) {
			this.Build();
			ydateReport.Binding.AddBinding(ViewModel, vm => vm.ReportDate, w => w.DateOrNull).InitializeFromSource();
			choiceNomenclatures.ViewModel = ViewModel.ChoiceNomenclatureViewModel;
			ycheckbuttonShowComments.Binding.AddBinding(ViewModel, vm => vm.ShowComments, w => w.Active).InitializeFromSource();
			ycheckbuttonShowPhone.Binding.AddBinding(ViewModel, vm => vm.ShowPhone, w => w.Active).InitializeFromSource();
			buttonRun.Binding.AddBinding(ViewModel, vm => vm.SensitiveLoad, w => w.Sensitive).InitializeFromSource();
			buttonRun.Clicked += (sender, args) => ViewModel.LoadReport();
		}
	}
}
