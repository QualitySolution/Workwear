using QS.Views.Dialog;
using Workwear.ViewModels.Tools;

namespace Workwear.Views.Tools
{
	public partial class ReplaceEntityView : DialogViewBase<ReplaceEntityViewModel>
	{
		public ReplaceEntityView(ReplaceEntityViewModel viewModel) : base(viewModel)
		{
			this.Build();

			treeSources.CreateFluentColumnsConfig<ReplaceEntityItem>()
				.AddColumn("Название").AddReadOnlyTextRenderer(x => x.Title)
				.AddColumn("Найдено ссылок").AddReadOnlyTextRenderer(x => x.TotalLinks.ToString())
				.Finish();
			treeSources.ItemsDataSource = ViewModel.SourceList;
			treeSources.Selection.Mode = Gtk.SelectionMode.Multiple;
			treeSources.Selection.Changed += (sender, e) => buttonRemove.Sensitive = treeSources.Selection.CountSelectedRows() > 0;

			entryTarget.ViewModel = ViewModel.TargetEntryViewModel;
			checkRemoveSource.Binding.AddBinding(ViewModel, v => v.RemoveSource, w => w.Active).InitializeFromSource();

			ViewModel.ProgressTotal = progressTotal;
			ViewModel.Progress = progressOperation;
			buttonReplace.Binding.AddBinding(ViewModel, v => v.SensitiveReplaceButton, w => w.Sensitive).InitializeFromSource();
		}

		protected void OnButtonReplaceClicked(object sender, System.EventArgs e)
		{
			ViewModel.RunReplace();
		}

		protected void OnButtonAddClicked(object sender, System.EventArgs e) {
			ViewModel.AddSourceItems();
		}

		protected void OnButtonRemoveClicked(object sender, System.EventArgs e) {
			ViewModel.RemoveSourceItem(treeSources.GetSelectedObjects<ReplaceEntityItem>());
		}
	}
}
