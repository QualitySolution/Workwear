using System;
using QS.Views;
using Workwear.ViewModels.Supply;

namespace Workwear.Views.Supply {
	public partial class ShipmentPeriodView : ViewBase<ShipmentPeriodViewModel> {
		public ShipmentPeriodView(ShipmentPeriodViewModel viewModel): base(viewModel) {
			this.Build();
			ydateStart.Binding
				.AddBinding(ViewModel, vm => vm.StartPeriod, w => w.DateOrNull)
				.InitializeFromSource();
			ydateEnd.Binding
				.AddBinding(ViewModel, vm => vm.EndPeriod, w => w.DateOrNull)
				.InitializeFromSource();
		}

		protected void OnButtonSetPeriodClicked(object sender, EventArgs e) {
		}
	}
}
