using QS.Dialog.GtkUI;
using QS.Views;
using Workwear.Domain.Visits;
using Workwear.Journal.Filter.ViewModels.Visits;
using workwear.Journal.ViewModels.Visits;

namespace Workwear.Journal.Filter.Views.Visits {
	public partial class IssuanceRequestFilterView : ViewBase<IssuanceRequestFilterViewModel> {
		public IssuanceRequestFilterView(IssuanceRequestFilterViewModel viewModel): base(viewModel) {
			this.Build();
			yenumcomboboxStatus.ItemsEnum = typeof(IssuanceRequestStatus);
			yenumcomboboxStatus.Binding
				.AddBinding(ViewModel, v => v.Status, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			buttonColorsLegend.Clicked += (o, e) => MessageDialogHelper.RunInfoDialog(IssuanceRequestJournalViewModel.ColorsLegendText, "Цветовая легенда,");
		}
	}
}
