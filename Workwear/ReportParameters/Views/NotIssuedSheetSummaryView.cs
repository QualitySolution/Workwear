﻿using System;
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
			ycheckShowEmployees.Binding.AddBinding(ViewModel, v => v.VisibleShowEmployees, w => w.Visible).InitializeFromSource();
			ylabelShowEmployees.Binding.AddBinding(ViewModel, v => v.VisibleShowEmployees, w => w.Visible).InitializeFromSource();
			ycheckExcludeInVacation.Binding.AddBinding(ViewModel, v => v.ExcludeInVacation, w => w.Active).InitializeFromSource();
			ycheckCondition.Binding.AddBinding(ViewModel, v => v.Condition, w => w.Active).InitializeFromSource();
			ycheckCondition.Binding.AddBinding(ViewModel, v => v.VisibleCondition, w => w.Visible).InitializeFromSource();
			ylabelcheckCondition.Binding.AddBinding(ViewModel, v => v.VisibleCondition, w => w.Visible).InitializeFromSource();
			ycheckGroupBySubdivision.Binding.AddBinding(ViewModel, v => v.GroupBySubdivision, w => w.Active).InitializeFromSource();
			ycheckHideWorn.Binding.AddBinding(ViewModel, v => v.HideWorn, w => w.Active).InitializeFromSource();
			ycomboboxWarehouse.Binding
				.AddSource(ViewModel)
				.AddBinding(wm => wm.WarehousesList, w => w.ItemsList)
				.AddBinding(wm => wm.Warehouse, w => w.SelectedItem)
				.InitializeFromSource();
			ycheckHideWorn.Binding.AddBinding(ViewModel, v => v.StockElementsSensetive, w => w.Sensitive).InitializeFromSource();
			ycheckGroupBySubdivision.Binding.AddBinding(ViewModel, v => v.GroupByElementSensetive, w => w.Sensitive).InitializeFromSource();
			
			buttonRun.Binding.AddBinding(ViewModel, v => v.SensetiveLoad, w => w.Sensitive).InitializeFromSource();
			
			comboReportType.ItemsEnum = typeof(NotIssuedSheetSummaryReportType);
			comboReportType.Binding.AddBinding(ViewModel, v => v.ReportType, w => w.SelectedItem).InitializeFromSource();

			entitySubdivision.ViewModel = ViewModel.SubdivisionEntry;
			entityDepartment.ViewModel = ViewModel.DepartmentEntry;
			choiceprotectiontoolsview1.ViewModel = ViewModel.ChoiceProtectionToolsViewModel;
			choiceemployeegroupview2.ViewModel = ViewModel.ChoiceEmployeeGroupViewModel;
			choiceemployeegroupview2.Visible = ViewModel.VisibleChoiceEmployeeGroup;
			expander2.Visible = ViewModel.VisibleChoiceEmployeeGroup;
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			ViewModel.LoadReport();
		}
		protected void OnExpander1Activated(object sender, EventArgs e) {
			(dialog1_VBox[expander1] as Gtk.Box.BoxChild).Expand = expander1.Expanded;
		}

		protected void OnExpander2Activated(object sender, EventArgs e) {
			(dialog1_VBox[expander2] as Gtk.Box.BoxChild).Expand = expander2.Expanded;
		}
	}
}
