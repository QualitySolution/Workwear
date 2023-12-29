using QS.Views.Dialog;
using Workwear.ViewModels.Export;

namespace Workwear.Views.Export {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FutureIssueExportView : DialogViewBase<FutureIssueExportViewModel> {
		public FutureIssueExportView(FutureIssueExportViewModel viewModel) : base(viewModel) {
			this.Build();
			
			ydateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(v => v.StartDate, w => w.StartDateOrNull)
				.AddBinding(v => v.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();
			entityentryOrganization.ViewModel = ViewModel.ResponsibleOrganizationEntryViewModel;
			ycheckbuttonShowCredit.Binding.AddBinding(ViewModel, v => v.ShowCredit, w => w.Active);
		}

		protected void OnYbuttonRunClicked(object sender, System.EventArgs e) {
			ybuttonRun.Sensitive = false;
			ViewModel.Create();
			ybuttonRun.Sensitive = true;
		}
	}
}
