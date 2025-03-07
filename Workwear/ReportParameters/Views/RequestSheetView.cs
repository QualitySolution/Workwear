﻿using System;
using System.Linq;
using QS.Utilities;
using QS.Utilities.Text;
using QS.Views;
using workwear.ReportParameters.ViewModels;
using Workwear.Domain.Stock;

namespace workwear.ReportParameters.Views {
	public partial class RequestSheetView : ViewBase<RequestSheetViewModel>
	{
		public RequestSheetView(RequestSheetViewModel viewModel) : base(viewModel)
		{
			this.Build();
			entitySubdivision.ViewModel = ViewModel.EntrySubdivisionViewModel;
			entitySubdivision.Binding
				.AddBinding(ViewModel, v => v.SensetiveSubdivision, w => w.Sensitive).InitializeFromSource();
			ycheckChild.Binding.AddBinding(ViewModel, vm => vm.AddChildSubdivisions, w => w.Active)
				.AddBinding(ViewModel, v => v.SensetiveSubdivision, w => w.Sensitive).InitializeFromSource();
			entityDepartment.ViewModel = ViewModel.EntryDepartmentViewModel;
			
			choiceprotectiontoolsview1.ViewModel = ViewModel.ChoiceProtectionToolsViewModel;
			
			labelIssueType.Binding
				.AddBinding(ViewModel, v => v.VisibleIssueType, w => w.Visible).InitializeFromSource();
			yenumIssueType.ItemsEnum = typeof(IssueType);
			yenumIssueType.Binding.AddSource(ViewModel)
				.AddBinding(v => v.IssueTypeOptions, w => w.SelectedItemOrNull)
				.AddBinding(v => v.VisibleIssueType, w => w.Visible)
				.InitializeFromSource();

			spinStartYear.Binding.AddBinding(ViewModel, v => v.BeginYear, w => w.ValueAsInt).InitializeFromSource();
			spinEndYear.Binding.AddBinding(ViewModel, v => v.EndYear, w => w.ValueAsInt).InitializeFromSource();

			comboStartMonth.SetRenderTextFunc<int>(x => DateHelper.GetMonthName(x).StringToTitleCase());
			comboStartMonth.ItemsList = Enumerable.Range(1, 12);
			comboStartMonth.Binding.AddBinding(ViewModel, v => v.BeginMonth, w => w.SelectedItem).InitializeFromSource();

			comboEndMonth.SetRenderTextFunc<int>(x => DateHelper.GetMonthName(x).StringToTitleCase());
			comboEndMonth.ItemsList = Enumerable.Range(1, 12);
			comboEndMonth.Binding.AddBinding(ViewModel, v => v.EndMonth, w => w.SelectedItem).InitializeFromSource();
			checkShowSex.Binding.AddBinding(ViewModel, v => v.ShowSex, w => w.Active).InitializeFromSource();
			
			ycheckExcludeInVacation.Binding
				.AddBinding(ViewModel, w => w.ExcludeInVacation, v => v.Active)
				.InitializeFromSource();

			buttonRun.Binding.AddBinding(ViewModel, v => v.SensitiveRunReport, w => w.Sensitive).InitializeFromSource();
			choiceemployeegroupview1.ViewModel = ViewModel.ChoiceEmployeeGroupViewModel;
			choiceemployeegroupview1.Visible = ViewModel.VisibleChoiceEmployeeGroup;
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

