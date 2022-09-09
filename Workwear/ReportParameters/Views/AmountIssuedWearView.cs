using System;
using System.Data.Bindings.Collections.Generic;
using QS.Views;
using workwear.Domain.Stock;
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

			comboIssueType.ItemsEnum = typeof(IssueType);
			comboIssueType.Binding.AddBinding(ViewModel, v => v.IssueType, w => w.SelectedItemOrNull).InitializeFromSource();

			checkBySize.Binding.AddBinding(ViewModel, v => v.BySize, w => w.Active).InitializeFromSource();

			ycheckSummry.Binding.AddBinding(viewModel, v => v.Summary, w => w.Active).InitializeFromSource();
			ycheckAll.Binding.AddSource(viewModel)
				.AddBinding(v => v.SelectAll, w => w.Active)
				.InitializeFromSource();
			
			ytreeSubdivisions.CreateFluentColumnsConfig<SelectedSubdivison>()
				.AddColumn("Показ").AddToggleRenderer(x => x.Select).Editing()
				.AddColumn("Подразделение").AddTextRenderer(x => x.Name)
				.Finish();
			ytreeSubdivisions.ItemsDataSource = new GenericObservableList<SelectedSubdivison>(viewModel.Subdivisions);

			buttonPrintReport.Binding.AddBinding(viewModel, v => v.SensitiveLoad, w => w.Sensitive).InitializeFromSource();

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
		}

		protected void OnButtonPrintReportClicked(object sender, EventArgs e)
		{
			ViewModel.LoadReport();
		}
	}
}
