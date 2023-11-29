using System;
using QS.Views;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	
	public partial class ProvisionReportView : ViewBase<ProvisionReportViewModel>{
		public ProvisionReportView(ProvisionReportViewModel viewModel) : base(viewModel) {
			this.Build();
			
			choiceprotectiontoolsview1.ViewModel = ViewModel.ChoiceProtectionToolsViewModel;
			choicesubdivisionview1.ViewModel = ViewModel.ChoiceSubdivisionViewModel;
			
			ydateReport.Binding.AddBinding(ViewModel, v => v.ReportDate, w => w.DateOrNull).InitializeFromSource();
			ycheckbuttonExcludeInVacation.Binding.AddBinding(ViewModel, v => v.ExcludeInVacation, w => w.Active).InitializeFromSource();
			ycheckbuttonShowSex.Binding.AddBinding(ViewModel, v => v.ShowSex, w => w.Active).InitializeFromSource();
			ycheckbuttonShowSize.Binding.AddBinding(ViewModel, v => v.ShowSize, w => w.Active).InitializeFromSource();
			ycheckbuttonGroupBySubdivision.Binding.AddBinding(ViewModel, v => v.GroupBySubdivision, w => w.Active).InitializeFromSource();
			ybuttonRun.Binding.AddBinding(ViewModel, v => v.SensetiveLoad, w => w.Sensitive).InitializeFromSource();
		}

		protected void OnYbuttonRunClicked(object sender, EventArgs e) {
			ViewModel.LoadReport();
		}
	}
}
