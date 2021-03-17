using System;
using QS.Views;
using workwear.ReportParameters.ViewModels.Gok;

namespace workwear.ReportParameters.Views.Gok
{
	public partial class IssuanceSheet138View : ViewBase<IssuanceSheet138ViewModel>
	{
		public IssuanceSheet138View(IssuanceSheet138ViewModel viewModel) : base(viewModel)
		{
			this.Build();

			ydateperiodpicker.Binding.AddSource(viewModel)
				.AddBinding(v => v.StartDate, w => w.StartDateOrNull)
				.AddBinding(v => v.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			buttonPrintReport.Binding.AddBinding(viewModel, v => v.SensetiveLoad, w => w.Sensitive).InitializeFromSource();
		}

		protected void OnButtonPrintReportClicked(object sender, EventArgs e)
		{
			ViewModel.LoadReport();
		}
	}
}
