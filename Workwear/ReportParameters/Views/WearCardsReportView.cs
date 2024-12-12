using System;
using QS.Views;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	public partial class WearCardsReportView : ViewBase<WearCardsReportViewModel> {
		public WearCardsReportView(WearCardsReportViewModel viewModel) : base(viewModel) {
			this.Build();
			comboReportType.ItemsEnum = typeof(WearCardsReportViewModel.WearCardsReportType);
			comboReportType.Binding.AddBinding(ViewModel, v=>v.ReportType, w=>w.SelectedItem).InitializeFromSource();
			ycheckbuttonOnlyWithoutNorms.Binding.AddBinding(ViewModel, v=>v.OnlyWithoutNorms, w=>w.Active).InitializeFromSource();
			ycheckbuttonOnlyWorking.Binding.AddBinding(ViewModel, v=>v.OnlyWorking, w=>w.Active).InitializeFromSource();
		}
		protected void OnYbuttonRunClicked(object sender, EventArgs e) {
			ViewModel.LoadReport();
		}
	}
}
