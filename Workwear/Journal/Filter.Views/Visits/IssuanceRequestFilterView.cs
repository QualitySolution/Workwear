using System;
using QS.Views;
using Workwear.Domain.Visits;
using workwear.Journal.Filter.ViewModels.Visits;

namespace Workwear.Journal.Filter.Views.Visits {
	public partial class IssuanceRequestFilterView : ViewBase<IssuanceRequestFilterViewModel> {
		public IssuanceRequestFilterView(IssuanceRequestFilterViewModel viewModel): base(viewModel) {
			this.Build();
			yenumcomboboxStatus.ItemsEnum = typeof(IssuanceRequestStatus);
			yenumcomboboxStatus.Binding
				.AddBinding(ViewModel, v => v.Status, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
		}
	}
}
