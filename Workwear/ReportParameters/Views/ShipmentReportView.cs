using System;
using QS.Views;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	public partial class ShipmentReportView : ViewBase<ShipmentReportViewModel> {
		public ShipmentReportView(ShipmentReportViewModel viewModel): base(viewModel) {
			this.Build();
			choicenormview.ViewModel = ViewModel.ChoiceNormViewModel;
			choiceshipmentview.ViewModel = ViewModel.ChoiceShipmentViewModel;
			buttonRun.Binding.AddBinding(ViewModel, v=>v.SensetiveLoad,w=>w.Sensitive).InitializeFromSource();
		}

		protected void OnExpander1Activated(object sender, EventArgs e) {
			(yvbox1[expander1] as Gtk.Box.BoxChild).Expand = expander1.Expanded;
		}

		protected void OnExpander2Activated(object sender, EventArgs e) {
			(yvbox1[expander2] as Gtk.Box.BoxChild).Expand = expander2.Expanded;
		}

		protected void OnButtonRunClicked(object sender, EventArgs e) {
			ViewModel.LoadReport();
		}
	}
}
