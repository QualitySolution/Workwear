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

			labelIssueType.Binding.AddBinding(viewModel, v => v.VisibleIssueType, w => w.Visible).InitializeFromSource();
			ydateReport.Binding.AddBinding(viewModel, v => v.ReportDate, w => w.DateOrNull).InitializeFromSource();
			dateExcludeBefore.Binding.AddBinding(viewModel, v => v.ExcludeBefore, w => w.DateOrNull).InitializeFromSource();
			comboIssueType.ItemsEnum = typeof(IssueType);
			comboIssueType.Binding.AddSource(viewModel)
				.AddBinding(v => v.IssueType, w => w.SelectedItemOrNull)
				.AddBinding(v => v.VisibleIssueType, w => w.Visible)
				.InitializeFromSource();

			buttonRun.Binding.AddBinding(viewModel, v => v.SensetiveLoad, w => w.Sensitive).InitializeFromSource();

			ycheckExcludeInVacation.Binding.AddBinding(viewModel, w => w.ExcludeInVacation, v => v.Active).InitializeFromSource();
			ycheckCondition.Binding.AddBinding(viewModel, w => w.Condition, v => v.Active).InitializeFromSource();
			ycheckCondition.Binding.AddBinding(viewModel, v => v.VisibleCondition, w => w.Visible).InitializeFromSource();
			ylabelcheckCondition.Binding.AddBinding(viewModel, v => v.VisibleCondition, w => w.Visible).InitializeFromSource();
			ycheckShowStock.Binding.AddBinding(viewModel, v => v.ShowStock, w => w.Active).InitializeFromSource();
			checkDontShowZeroStock.Binding.AddBinding(viewModel, v => v.ExcludeZeroStock, w => w.Active).InitializeFromSource();
			checkDontShowZeroStock.Binding.AddBinding(viewModel, v => v.DontShowZeroStockVisible, w => w.Sensitive).InitializeFromSource();
			ylabelDontShowZeroStock.Binding.AddBinding(viewModel, v => v.DontShowZeroStockVisible, w => w.Sensitive).InitializeFromSource();
				
			entitySubdivision.ViewModel = viewModel.SubdivisionEntry;
			choiceprotectiontoolsview1.ViewModel = viewModel.ChoiceProtectionToolsViewModel;
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			ViewModel.LoadReport();
		}
	}
}
