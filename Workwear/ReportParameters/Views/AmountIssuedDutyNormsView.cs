using QS.Views;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	public partial class AmountIssuedDutyNormsView : ViewBase<AmountIssuedDutyNormsViewModel> {
		public AmountIssuedDutyNormsView(AmountIssuedDutyNormsViewModel viewModel) : base(viewModel) {
			this.Build();
			
			ydateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(v => v.StartDate, w => w.StartDateOrNull)
				.AddBinding(v => v.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			buttonPrintReport.Binding.AddBinding(ViewModel, v => v.SensetiveLoad, w => w.Sensitive).InitializeFromSource();

			yentryMatch.Binding.AddBinding(ViewModel, v => v.MatchString, w => w.Text).InitializeFromSource();
			yentryNoMatch.Binding.AddBinding(ViewModel, v => v.NoMatchString, w => w.Text).InitializeFromSource();
			
			checkUseAlterName.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.UseAlternativeName, w => w.Active)
				.InitializeFromSource();

			ylabelOwners.Visible = yspeccomboboxOwners.Visible = ViewModel.VisibleOwners;
			yspeccomboboxOwners.SelectedItemStrictTyped = false;
			yspeccomboboxOwners.Binding
				.AddSource(ViewModel)
				.AddBinding(wm => wm.Owners, w => w.ItemsList)
				.AddBinding(wm => wm.SelectOwner, w => w.SelectedItem)
				.InitializeFromSource();

			checkShowCost.Visible = ViewModel.VisibleShowCost; ;
			checkShowCost.Binding
				.AddBinding(ViewModel, wm => wm.ShowCost, w => w.Active)
				.InitializeFromSource();
	
			choicesubdivisionview1.ViewModel = ViewModel.ChoiceSubdivisionViewModel;
		}
		
		protected void OnButtonPrintReportClicked(object sender, System.EventArgs e) =>
			ViewModel.LoadReport();
	}
}

