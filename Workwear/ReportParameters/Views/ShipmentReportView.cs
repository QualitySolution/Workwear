using System;
using QS.Views;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	public partial class ShipmentReportView : ViewBase<ShipmentReportViewModel> {
		public ShipmentReportView(ShipmentReportViewModel viewModel) : base(viewModel) {
			this.Build();
			
		}
	}
}
