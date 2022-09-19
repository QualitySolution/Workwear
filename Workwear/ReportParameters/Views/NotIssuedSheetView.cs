using System;
using QS.Views;
using Workwear.Domain.Stock;
using workwear.ReportParameters.ViewModels;

namespace workwear.ReportParameters.Views
{
	public partial class NotIssuedSheetView : ViewBase<NotIssuedSheetViewModel>
	{

		public NotIssuedSheetView(NotIssuedSheetViewModel viewModel) : base(viewModel)
		{
			this.Build();

			ydateReport.Binding.AddBinding(ViewModel, v => v.ReportDate, w => w.DateOrNull).InitializeFromSource();
			dateExcludeBefore.Binding.AddBinding(ViewModel, v => v.ExcludeBefore, w => w.DateOrNull).InitializeFromSource();
			comboIssueType.ItemsEnum = typeof(IssueType);
			comboIssueType.Binding.AddBinding(ViewModel, v => v.IssueType, w => w.SelectedItemOrNull).InitializeFromSource();

			buttonRun.Binding.AddBinding(ViewModel, v => v.SensetiveLoad, w => w.Sensitive).InitializeFromSource();

			ycheckExcludeInVacation.Binding.AddBinding(viewModel, w => w.ExcludeInVacation, v => v.Active).InitializeFromSource();

			entitySubdivision.ViewModel = ViewModel.SubdivisionEntry;
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			ViewModel.LoadReport();
		}
	}
}
