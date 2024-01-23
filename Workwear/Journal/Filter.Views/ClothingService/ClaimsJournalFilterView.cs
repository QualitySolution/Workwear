using System;
using QS.Views;
using Workwear.Journal.Filter.ViewModels.ClothingService;

namespace Workwear.Journal.Filter.Views.ClothingService {
	public partial class ClaimsJournalFilterView : ViewBase<ClaimsJournalFilterViewModel> {
		public ClaimsJournalFilterView(ClaimsJournalFilterViewModel viewModel) : base(viewModel) {
			this.Build();

			checkShowClosed.Binding
				.AddSource(ViewModel)
				.AddBinding(v => v.ShowClosed, w => w.Active)
				.AddBinding(v => v.SensitiveShowClosed, w => w.Sensitive)
				.InitializeFromSource();
		}
	}
}
