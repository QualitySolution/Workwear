using System;
using QS.Views;
using workwear.ReportParameters.ViewModels;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ClothingServiceReportView : ViewBase<ClothingServiceReportViewModel> {
		public ClothingServiceReportView(ClothingServiceReportViewModel viewModel) : base (viewModel){
			this.Build();
			buttonRun.Clicked += OnButtonRunClicked;
			ycheckbuttonshowclosed.Binding.AddBinding(ViewModel, w => w.ShowClosed, v => v.Active).InitializeFromSource();
		}
		protected void OnButtonRunClicked(object sender, EventArgs e) => ViewModel.LoadReport();
	}
}
