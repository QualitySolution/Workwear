using System;
using QS.Cloud.Postomat.Manage;
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
			comboPostomat.SetRenderTextFunc<PostomatInfo>(p => $"{p.Name} {p.Location}");
			comboPostomat.Binding
				.AddSource(ViewModel)
				.AddBinding(v => v.SensitivePostomat, w => w.Sensitive)
				.AddBinding(v => v.Postomats, w => w.ItemsList)
				.AddBinding(v => v.Postomat, w => w.SelectedItem)
				.InitializeFromSource();
		}
	}
}
