using System;
using QS.Views;
using Workwear.ViewModels.Supply;

namespace Workwear.Views.Supply {
	public partial class ShipmentPeriodView : ViewBase<ShipmentPeriodViewModel> {
		public ShipmentPeriodView(ShipmentPeriodViewModel viewModel): base(viewModel) {
			this.Build();
			datePeriod.Binding.AddSource(ViewModel)
				.AddBinding(v => v.StartPeriod, w => w.StartDateOrNull)
				.AddBinding(v => v.EndPeriod, w => w.EndDateOrNull)
				.InitializeFromSource();
		}

		protected void OnButtonSetPeriodClicked(object sender, EventArgs e) {
			ViewModel.FillPeriod();
		}
	}
}
