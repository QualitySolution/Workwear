using System;
using QS.Views;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	public partial class VacationsReportView : ViewBase<VacationsReportViewModel> {
		public VacationsReportView(VacationsReportViewModel viewModel) : base(viewModel) {
			this.Build();

			ydateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(v => v.StartDate, w => w.StartDateOrNull)
				.AddBinding(v => v.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			yspeccomboboxVacType.SelectedItemStrictTyped = false;
			yspeccomboboxVacType.Binding.AddSource(ViewModel)
				.AddBinding(v => v.VacationTypes, w => w.ItemsList)
				.AddBinding(v => v.SelectVacationType, w => w.SelectedItem)
				.InitializeFromSource();

			choicesubdivisionview1.ViewModel = ViewModel.ChoiceSubdivisionViewModel;

			buttonPrintReport.Binding.AddBinding(ViewModel, v => v.SensitiveLoad, w => w.Sensitive).InitializeFromSource();
			buttonPrintReport.Clicked += OnButtonPrintReportClicked;
		}

		protected void OnButtonPrintReportClicked(object sender, EventArgs e) {
			ViewModel.LoadReport();
		}
	}
}
