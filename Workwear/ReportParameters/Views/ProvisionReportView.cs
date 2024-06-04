using System;
using QS.Views;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	
	public partial class ProvisionReportView : ViewBase<ProvisionReportViewModel>{
		public ProvisionReportView(ProvisionReportViewModel viewModel) : base(viewModel) {
			this.Build();
			
			choiceprotectiontoolsview1.ViewModel = ViewModel.ChoiceProtectionToolsViewModel;
			choicesubdivisionview1.ViewModel = ViewModel.ChoiceSubdivisionViewModel;
			choiceemployeegroupview2.ViewModel = ViewModel.ChoiceEmployeeGroupViewModel;
			choiceemployeegroupview2.Visible = ViewModel.VisibleChoiceEmployeeGroup;
			
			ycheckbuttonExcludeInVacation.Binding.AddBinding(ViewModel, v => v.ExcludeInVacation, w => w.Active).InitializeFromSource();
			ycheckbuttonShowSex.Binding.AddBinding(ViewModel, v => v.ShowSex, w => w.Active).InitializeFromSource();
			ycheckbuttonShowSize.Binding.AddBinding(ViewModel, v => v.ShowSize, w => w.Active).InitializeFromSource();
			ycheckbuttonGroupBySubdivision.Binding.AddBinding(ViewModel, v => v.GroupBySubdivision, w => w.Active).InitializeFromSource();
			ycheckbuttonGroupByNormAmount.Binding.AddBinding(ViewModel, v => v.GroupByNormAmount, w => w.Active).InitializeFromSource();
			ycheckShowStock.Binding.AddBinding(ViewModel, v => v.ShowStock, w => w.Active).InitializeFromSource();
			ycheckShowStock.Binding.AddBinding(ViewModel, v => v.VisibleShowStock, w => w.Visible).InitializeFromSource();
			ylabelShowStock.Binding.AddBinding(ViewModel, v => v.VisibleShowStock, w => w.Visible).InitializeFromSource();
			ycheckShowEmployees.Binding.AddBinding(ViewModel, v => v.ShowEmployees, w => w.Active).InitializeFromSource();
			ycheckShowEmployees.Binding.AddBinding(ViewModel, v => v.VisibleShowEmployee, w => w.Visible).InitializeFromSource();
			ylabelShowEmployees.Binding.AddBinding(ViewModel, v => v.VisibleShowEmployee, w => w.Visible).InitializeFromSource();
			
			comboReportType.ItemsEnum = typeof(ProvisionReportViewModel.ProvisionReportType);
			comboReportType.Binding.AddBinding(ViewModel, v => v.ReportType, w => w.SelectedItem).InitializeFromSource();
			
			ybuttonRun.Binding.AddBinding(ViewModel, v => v.SensetiveLoad, w => w.Sensitive).InitializeFromSource();
		}

		protected void OnYbuttonRunClicked(object sender, EventArgs e) {
			ViewModel.LoadReport();
		}

		protected void OnExpander1Activated(object sender, EventArgs e) {
			(vbox1[expander1] as Gtk.Box.BoxChild).Expand = expander1.Expanded;
		}

		protected void OnExpander2Activated(object sender, EventArgs e) {
			(vbox1[expander2] as Gtk.Box.BoxChild).Expand = expander2.Expanded;
		}

		protected void OnExpander3Activated(object sender, EventArgs e) {
			(vbox1[expander3] as Gtk.Box.BoxChild).Expand = expander3.Expanded;
		}
	}
}
