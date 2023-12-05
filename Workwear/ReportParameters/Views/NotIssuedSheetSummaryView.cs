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

			labelIssueType.Binding.AddBinding(viewModel, v => v.VisibleIssueType, w => w.Visible).InitializeFromSource();
			ydateReport.Binding.AddBinding(viewModel, v => v.ReportDate, w => w.DateOrNull).InitializeFromSource();
			dateExcludeBefore.Binding.AddBinding(viewModel, v => v.ExcludeBefore, w => w.DateOrNull).InitializeFromSource();
			comboIssueType.ItemsEnum = typeof(IssueType);
			comboIssueType.Binding.AddSource(viewModel)
				.AddBinding(v => v.IssueType, w => w.SelectedItemOrNull)
				.AddBinding(v => v.VisibleIssueType, w => w.Visible)
				.InitializeFromSource();
			checkShowSex.Binding.AddBinding(viewModel, v => v.ShowSex, w => w.Active).InitializeFromSource();
			checkShowEmployees.Binding.AddBinding(viewModel, v => v.ShowEmployees, w => w.Active).InitializeFromSource();
			ycheckExcludeInVacation.Binding.AddBinding(viewModel, v => v.ExcludeInVacation, w => w.Active).InitializeFromSource();
			ycheckCondition.Binding.AddBinding(viewModel, v => v.Condition, w => w.Active).InitializeFromSource();
			ycheckCondition.Binding.AddBinding(viewModel, v => v.VisibleCondition, w => w.Visible).InitializeFromSource();
			ylabelcheckCondition.Binding.AddBinding(viewModel, v => v.VisibleCondition, w => w.Visible).InitializeFromSource();
			checkGroupBySubdivision.Binding.AddBinding(viewModel, v => v.GroupBySubdivision, w => w.Active).InitializeFromSource();
			ycheckShowStock.Binding.AddBinding(viewModel, v => v.ShowStock, w => w.Active).InitializeFromSource();
			
			buttonRun.Binding.AddBinding(ViewModel, v => v.SensetiveLoad, w => w.Sensitive).InitializeFromSource();

			entitySubdivision.ViewModel = viewModel.SubdivisionEntry;
			choiceprotectiontoolsview2.ViewModel = viewModel.ChoiceProtectionToolsViewModel;
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			ViewModel.LoadReport();
		}
	}
}
