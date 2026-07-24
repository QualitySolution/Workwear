using Gamma.Widgets;
using QS.Views;
using Workwear.Domain.Operations;
using Workwear.Journal.Filter.ViewModels.Stock;

namespace Workwear.Journal.Filter.Views.Stock {
	public partial class OverNormIssuedJournalFilterView : ViewBase<OverNormIssuedJournalFilterViewModel> {
		public OverNormIssuedJournalFilterView(OverNormIssuedJournalFilterViewModel viewModel) : base(viewModel) {
			this.Build();

			enumcomboType.ItemsEnum = typeof(OverNormType);
			enumcomboType.Binding
				.AddBinding(ViewModel, v => v.Type, w => w.SelectedItemOrNull)
				.InitializeFromSource();
		}
	}
}
