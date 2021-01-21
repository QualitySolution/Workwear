using System;
using System.Data.Bindings.Collections.Generic;
using QS.Views;
using workwear.ReportParameters.ViewModels;

namespace workwear.ReportParameters.Views
{
	public partial class AmountIssuedWearView : ViewBase<AmountIssuedWearViewModel>
	{
		public AmountIssuedWearView(AmountIssuedWearViewModel viewModel) : base(viewModel)
		{
			this.Build();

			ydateperiodpicker.Binding.AddSource(viewModel)
				.AddBinding(v => v.StartDate, w => w.StartDateOrNull)
				.AddBinding(v => v.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			ycheckSummry.Binding.AddBinding(viewModel, v => v.Summary, w => w.Active).InitializeFromSource();
			ycheckAll.Binding.AddSource(viewModel)
				.AddBinding(v => v.SelectAll, w => w.Active)
				.AddBinding(v => v.SensetiveSubdivisions, w => w.Sensitive).InitializeFromSource();

			ytreeSubdivisions.Binding.AddBinding(viewModel, v => v.SensetiveSubdivisions, w => w.Sensitive).InitializeFromSource();
			ytreeSubdivisions.CreateFluentColumnsConfig<SelectedSubdivison>()
				.AddColumn("Показ").AddToggleRenderer(x => x.Select).Editing()
				.AddColumn("Подразделение").AddTextRenderer(x => x.Name)
				.Finish();
			ytreeSubdivisions.ItemsDataSource = new GenericObservableList<SelectedSubdivison>(viewModel.Subdivisons);

			buttonPrintReport.Binding.AddBinding(viewModel, v => v.SensetiveLoad, w => w.Sensitive).InitializeFromSource();
		}

		protected void OnButtonPrintReportClicked(object sender, EventArgs e)
		{
			ViewModel.LoadReport();
		}
	}
}
