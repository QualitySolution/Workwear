using System;
using QS.Views;
using Workwear.ViewModels.Supply;

namespace Workwear.Views.Supply {
	public partial class ShipmentDiffCauseView : ViewBase<ShipmentDiffCauseViewModel> {
		public ShipmentDiffCauseView(ShipmentDiffCauseViewModel viewModel) : base(viewModel) {
			this.Build();
			entryDiffCause.Binding
				.AddBinding(ViewModel, vm => vm.DiffCause, w => w.Text)
				.InitializeFromSource();
		}
		
		protected void OnButtonFillDiffCauseClicked(object sender, EventArgs e) {
			ViewModel.FillDiffCause();
		}
	}
}
