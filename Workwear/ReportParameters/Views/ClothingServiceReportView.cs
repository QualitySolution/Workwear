using System;
using QS.Views;
using Workwear.ReportParameters.ViewModels;
namespace Workwear.ReportParameters.Views {
	public partial class ClothingServiceReportView : ViewBase<ClothingServiceReportViewModel> {
		public ClothingServiceReportView(ClothingServiceReportViewModel viewModel) : base (viewModel){
			this.Build();
			ydateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(w => w.StartDate, v => v.StartDateOrNull)
				.AddBinding(w => w.EndDate, v => v.EndDateOrNull)
				.AddBinding(vm => vm.VisibleUseAlternative, w => w.Visible)
				.InitializeFromSource();
			ylabelPeriod.Binding.AddBinding(ViewModel, v => v.VisibleUseAlternative, w => w.Visible).InitializeFromSource();
			buttonRun.Clicked += OnButtonRunClicked;
			buttonRun.Binding.AddBinding(ViewModel, v=>v.SensetiveLoad, w=>w.Sensitive).InitializeFromSource();
			ycheckbuttonshowclosed.Binding.AddBinding(ViewModel, w => w.ShowClosed, v => v.Active).InitializeFromSource();
		}
		protected void OnButtonRunClicked(object sender, EventArgs e) => ViewModel.LoadReport();
	}
}
