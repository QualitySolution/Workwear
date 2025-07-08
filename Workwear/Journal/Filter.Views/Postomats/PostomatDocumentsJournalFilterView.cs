using QS.Views;
using Workwear.Journal.Filter.ViewModels.Postomats;

namespace Workwear.Journal.Filter.Views.Postomats {
	public partial class PostomatDocumentsJournalFilterView : ViewBase<PostomatDocumentsJournalFilterViewModel> {
		public PostomatDocumentsJournalFilterView(PostomatDocumentsJournalFilterViewModel viewModel) : base(viewModel) {
			this.Build();

			checkShowClosed.Binding
				.AddSource(ViewModel)
				.AddBinding(v => v.ShowClosed, w => w.Active)
				.AddBinding(v => v.SensitiveShowClosed, w => w.Sensitive)
				.InitializeFromSource();
		}
	}
}
