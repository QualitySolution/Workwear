using QS.Views.Dialog;
using Workwear.ViewModels.Tools;

namespace Workwear.Views.Tools
{
	public partial class ReplaceEntityView : DialogViewBase<ReplaceEntityViewModel>
	{
		public ReplaceEntityView(ReplaceEntityViewModel viewModel) : base(viewModel)
		{
			this.Build();

			entrySource.ViewModel = ViewModel.SourceEntryViewModel;
			entryTarget.ViewModel = ViewModel.TargetEntryViewModel;
			checkRemoveSource.Binding.AddBinding(ViewModel, v => v.RemoveSource, w => w.Active).InitializeFromSource();
			labelTotalLinks.Binding.AddBinding(ViewModel, v => v.TotalLinksText, w => w.LabelProp).InitializeFromSource();

			ViewModel.Progress = progressTotal;
			buttonReplace.Binding.AddBinding(ViewModel, v => v.SensitiveReplaceButton, w => w.Sensitive).InitializeFromSource();
		}

		protected void OnButtonReplaceClicked(object sender, System.EventArgs e)
		{
			ViewModel.RunReplace();
		}
	}
}
