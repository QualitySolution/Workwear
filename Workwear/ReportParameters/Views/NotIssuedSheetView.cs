﻿using System;
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

			labelIssueType.Binding.AddBinding(ViewModel, v => v.VisibleIssueType, w => w.Visible).InitializeFromSource();
			ydateReport.Binding.AddBinding(ViewModel, v => v.ReportDate, w => w.DateOrNull).InitializeFromSource();
			dateExcludeBefore.Binding.AddBinding(ViewModel, v => v.ExcludeBefore, w => w.DateOrNull).InitializeFromSource();
			comboIssueType.ItemsEnum = typeof(IssueType);
			comboIssueType.Binding.AddSource(ViewModel)
				.AddBinding(v => v.IssueType, w => w.SelectedItemOrNull)
				.AddBinding(v => v.VisibleIssueType, w => w.Visible)
				.InitializeFromSource();

			buttonRun.Binding.AddBinding(ViewModel, v => v.SensetiveLoad, w => w.Sensitive).InitializeFromSource();

			ycheckExcludeInVacation.Binding.AddBinding(ViewModel, w => w.ExcludeInVacation, v => v.Active).InitializeFromSource();
			
			ycheckCondition.Binding.AddBinding(ViewModel, w => w.Condition, v => v.Active).InitializeFromSource();
			ycheckCondition.Binding.AddBinding(ViewModel, v => v.VisibleCondition, w => w.Visible).InitializeFromSource();
			ylabelcheckCondition.Binding.AddBinding(ViewModel, v => v.VisibleCondition, w => w.Visible).InitializeFromSource();
			
			ycheckShowStock.Binding.AddBinding(ViewModel, v => v.ShowStock, w => w.Active).InitializeFromSource();
			checkDontShowZeroStock.Binding.AddBinding(ViewModel, v => v.ExcludeZeroStock, w => w.Active).InitializeFromSource();
			checkDontShowZeroStock.Binding.AddBinding(ViewModel, v => v.DontShowZeroStockVisible, w => w.Sensitive).InitializeFromSource();
			ylabelDontShowZeroStock.Binding.AddBinding(ViewModel, v => v.DontShowZeroStockVisible, w => w.Sensitive).InitializeFromSource();
			
			comboReportType.ItemsEnum = typeof(NotIssuedSheetReportType);
			comboReportType.Binding.AddBinding(ViewModel, v => v.ReportType, w => w.SelectedItem).InitializeFromSource();
				
			entitySubdivision.ViewModel = ViewModel.SubdivisionEntry;
			choiceprotectiontoolsview1.ViewModel = ViewModel.ChoiceProtectionToolsViewModel;
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			ViewModel.LoadReport();
		}
	}
}
