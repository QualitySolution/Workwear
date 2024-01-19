using System;
using QS.Views;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	public partial class BarcodeCompletenessReportView : ViewBase<BarcodeCompletenessReportViewModel>{
		public BarcodeCompletenessReportView(BarcodeCompletenessReportViewModel viewModel) : base(viewModel) {
			this.Build();
			
			choiceprotectiontoolsview1.ViewModel = ViewModel.ChoiceProtectionToolsViewModel;
			choicesubdivisionview1.ViewModel = ViewModel.ChoiceSubdivisionViewModel;

			ycheckbuttonExcludeInVacation.Binding.AddBinding(ViewModel, v => v.ExcludeInVacation, w => w.Active).InitializeFromSource();
			ycheckbuttonShowSex.Binding.AddBinding(ViewModel, v => v.ShowSex, w => w.Active).InitializeFromSource();
			ycheckbuttonShowSize.Binding.AddBinding(ViewModel, v => v.ShowSize, w => w.Active).InitializeFromSource();
			ycheckbuttonGroupBySubdivision.Binding.AddBinding(ViewModel, v => v.GroupBySubdivision, w => w.Active).InitializeFromSource();
			ycheckShowEmployees.Binding.AddBinding(ViewModel, v => v.ShowEmployees, w => w.Active).InitializeFromSource();
			ycheckShowEmployees.Binding.AddBinding(ViewModel, v => v.VisibleShowEmployee, w => w.Visible).InitializeFromSource();
			ylabelShowEmployees.Binding.AddBinding(ViewModel, v => v.VisibleShowEmployee, w => w.Visible).InitializeFromSource();

			yspinbuttonBarcodeLag.Binding.AddBinding(ViewModel, e => e.BarcodeLag, w=> w.ValueAsDecimal).InitializeFromSource();
			
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
	}
}
