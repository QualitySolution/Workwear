﻿using System;
using QS.Views;
using workwear.ReportParameters.ViewModels;
using Workwear.Domain.Stock;
using Workwear.Tools.Features;

namespace workwear.ReportParameters.Views {
	public partial class AmountIssuedWearView : ViewBase<AmountIssuedWearViewModel>
	{
		public AmountIssuedWearView(AmountIssuedWearViewModel viewModel) : base(viewModel)
		{
			this.Build();

			comboReportType.ItemsEnum = typeof(AmountIssuedWearReportType);
			comboReportType.Binding.AddBinding(ViewModel, v => v.ReportType, w => w.SelectedItem).InitializeFromSource();

			ydateperiodpicker.Binding.AddSource(viewModel)
				.AddBinding(v => v.StartDate, w => w.StartDateOrNull)
				.AddBinding(v => v.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			labelIssueType.Binding.AddBinding(ViewModel, v => v.VisibleIssueType, w => w.Visible).InitializeFromSource();
			comboIssueType.ItemsEnum = typeof(IssueType);
			comboIssueType.Binding.AddSource(ViewModel)
				.AddBinding(v => v.IssueType, w => w.SelectedItemOrNull)
				.AddBinding(v => v.VisibleIssueType, w => w.Visible)
				.InitializeFromSource();

			checkBySubdivision.Binding.AddBinding(viewModel, v => v.BySubdivision, w => w.Active).InitializeFromSource();
			checkByEmployee.Binding.AddBinding(ViewModel, v => v.ByEmployee, w => w.Active).InitializeFromSource();
			checkBySize.Binding.AddBinding(ViewModel, v => v.BySize, w => w.Active).InitializeFromSource();

			buttonPrintReport.Binding.AddBinding(viewModel, v => v.SensetiveLoad, w => w.Sensitive).InitializeFromSource();

			yentryMatch.Binding.AddBinding(viewModel, v => v.MatchString, w => w.Text).InitializeFromSource();
			yentryNoMatch.Binding.AddBinding(viewModel, v => v.NoMatchString, w => w.Text).InitializeFromSource();
			
			ycheckChild.Binding
				.AddSource(viewModel)
				.AddBinding(vm => vm.AddChildSubdivisions, w => w.Active)
				.InitializeFromSource();
			checkUseAlterName.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.UseAlternativeName, w => w.Active)
				.AddBinding(vm => vm.VisibleUseAlternative, w => w.Visible)
				.InitializeFromSource();

			ylabelOwners.Visible = yspeccomboboxOwners.Visible = ViewModel.VisibleOwners;
			yspeccomboboxOwners.SelectedItemStrictTyped = false;
			yspeccomboboxOwners.Binding
				.AddSource(ViewModel)
				.AddBinding(wm => wm.Owners, w => w.ItemsList)
				.AddBinding(wm => wm.SelectOwner, w => w.SelectedItem)
				.InitializeFromSource();

			checkShowCost.Visible = ViewModel.FeaturesService.Available(WorkwearFeature.Selling);
			checkShowCost.Binding
				.AddBinding(ViewModel, wm => wm.ShowCost, w => w.Active)
				.InitializeFromSource();

			checkShowCostCenter.Visible = ViewModel.VisibleCostCenter;
			checkShowCostCenter.Binding.AddBinding(ViewModel, v => v.ShowCostCenter, w => w.Active).InitializeFromSource();
			
			checkShowOnlyWithoutNorm.Binding.AddBinding(ViewModel, v => v.ShowOnlyWithoutNorm, w => w.Active).InitializeFromSource();
			
			choicesubdivisionview1.ViewModel = ViewModel.ChoiceSubdivisionViewModel;
		}

		protected void OnButtonPrintReportClicked(object sender, EventArgs e)
		{
			ViewModel.LoadReport();
		}
	}
}
