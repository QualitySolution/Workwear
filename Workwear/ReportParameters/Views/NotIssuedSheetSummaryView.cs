using System;
using QS.Views;
using Workwear.Domain.Stock;
using workwear.ReportParameters.ViewModels;

namespace workwear.ReportParameters.Views
{
	public partial class NotIssuedSheetSummaryView : ViewBase<NotIssuedSheetSummaryViewModel>
	{

		public NotIssuedSheetSummaryView(NotIssuedSheetSummaryViewModel viewModel) : base(viewModel)
		{
			this.Build();

			labelIssueType.Binding.AddBinding(ViewModel, v => v.VisibleIssueType, w => w.Visible).InitializeFromSource();
			ydateReport.Binding.AddBinding(ViewModel, v => v.ReportDate, w => w.DateOrNull).InitializeFromSource();
			dateExcludeBefore.Binding.AddBinding(ViewModel, v => v.ExcludeBefore, w => w.DateOrNull).InitializeFromSource();
			comboIssueType.ItemsEnum = typeof(IssueType);
			comboIssueType.Binding.AddSource(ViewModel)
				.AddBinding(v => v.IssueType, w => w.SelectedItemOrNull)
				.AddBinding(v => v.VisibleIssueType, w => w.Visible)
				.InitializeFromSource();
			ycheckShowSex.Binding.AddBinding(ViewModel, v => v.ShowSex, w => w.Active).InitializeFromSource();
			ycheckShowEmployees.Binding.AddBinding(ViewModel, v => v.ShowEmployees, w => w.Active).InitializeFromSource();
			ycheckExcludeInVacation.Binding.AddBinding(ViewModel, v => v.ExcludeInVacation, w => w.Active).InitializeFromSource();
			ycheckCondition.Binding.AddBinding(ViewModel, v => v.Condition, w => w.Active).InitializeFromSource();
			ycheckCondition.Binding.AddBinding(ViewModel, v => v.VisibleCondition, w => w.Visible).InitializeFromSource();
			ylabelcheckCondition.Binding.AddBinding(ViewModel, v => v.VisibleCondition, w => w.Visible).InitializeFromSource();
			ycheckGroupBySubdivision.Binding.AddBinding(ViewModel, v => v.GroupBySubdivision, w => w.Active).InitializeFromSource();
			ycheckShowStock.Binding.AddBinding(ViewModel, v => v.ShowStock, w => w.Active).InitializeFromSource();
			ycheckHideWorn.Binding.AddBinding(ViewModel, v => v.HideWorn, w => w.Active).InitializeFromSource();
			ycheckHideWorn.Binding.AddBinding(ViewModel, v => v.StockElementsVisible, w => w.Sensitive).InitializeFromSource();
			ylabelHideWorn.Binding.AddBinding(ViewModel, v => v.StockElementsVisible, w => w.Sensitive).InitializeFromSource();
			
			buttonRun.Binding.AddBinding(ViewModel, v => v.SensetiveLoad, w => w.Sensitive).InitializeFromSource();
			
			comboReportType.ItemsEnum = typeof(NotIssuedSheetSummaryReportType);
			comboReportType.Binding.AddBinding(ViewModel, v => v.ReportType, w => w.SelectedItem).InitializeFromSource();

			entitySubdivision.ViewModel = ViewModel.SubdivisionEntry;
			choiceprotectiontoolsview2.ViewModel = ViewModel.ChoiceProtectionToolsViewModel;
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			ViewModel.LoadReport();
		}
	}
}
