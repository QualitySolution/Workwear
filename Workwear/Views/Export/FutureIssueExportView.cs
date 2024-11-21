using QS.Views.Dialog;
using Workwear.ViewModels.Export;

namespace Workwear.Views.Export {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FutureIssueExportView : DialogViewBase<FutureIssueExportViewModel> {
		public FutureIssueExportView(FutureIssueExportViewModel viewModel) : base(viewModel) {
			this.Build();
			
			ydateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(v => v.StartDate, w => w.StartDate)
				.AddBinding(v => v.EndDate, w => w.EndDate)
				.InitializeFromSource();
			entityentryOrganization.ViewModel = ViewModel.ResponsibleOrganizationEntryViewModel;
			checkMoveDebt.Binding.AddBinding(ViewModel, v => v.MoveDebt, w => w.Active).InitializeFromSource();

			choiceprotectiontoolsview1.ViewModel = ViewModel.ChoiceProtectionToolsViewModel;
			
			yprogressTotal.Visible = false;
			yprogressLocal.Visible = false;
			ViewModel.ProgressGlobal = yprogressTotal;
			ViewModel.ProgressLocal = yprogressLocal;

			ybuttonRun.Binding
				.AddBinding(ViewModel, v => v.RunSensitive, w => w.Sensitive)
				.AddBinding(ViewModel, v => v.RunVisible, w => w.Visible).InitializeFromSource();
			ylabel_done.Binding.AddBinding(ViewModel, vm => vm.DoneVisible, w => w.Visible).InitializeFromSource();
		}

		protected void OnYbuttonRunClicked(object sender, System.EventArgs e) {
			ybuttonRun.Sensitive = false;
			ViewModel.Create();
			ybuttonRun.Sensitive = true;
		}
	}
}
