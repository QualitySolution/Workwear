using System;
using QS.Views;
using workwear.ReportParameters.ViewModels;

namespace workwear.ReportParameters.Views
{
	public partial class RequestSheetView : ViewBase<RequestSheetViewModel>
	{
		public RequestSheetView(RequestSheetViewModel viewModel) : base(viewModel)
		{
			this.Build();
			entitySubdivision.ViewModel = ViewModel.EntrySubdivisionViewModel;
			labelPeriodType.Binding.AddBinding(ViewModel, v => v.PeriodLabel, w => w.LabelProp).InitializeFromSource();
			comboQuarter.Binding.AddSource(ViewModel)
				.AddBinding(v => v.PeriodList, w => w.ItemsList)
				.AddBinding(v => v.SelectedPeriod, w => w.SelectedItem)
				.InitializeFromSource();
			ViewModel.PeriodType = PeriodType.Month;
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			ViewModel.LoadReport();
		}

		protected void OnRadioMonthToggled(object sender, EventArgs e)
		{
			if (radioMonth.Active)
				ViewModel.PeriodType = PeriodType.Month;
		}

		protected void OnRadioQuarterToggled(object sender, EventArgs e)
		{
			if (radioQuarter.Active)
				ViewModel.PeriodType = PeriodType.Quarter;
		}

		protected void OnRadioYearToggled(object sender, EventArgs e)
		{
			if (radioYear.Active)
				ViewModel.PeriodType = PeriodType.Year;
		}
	}
}

