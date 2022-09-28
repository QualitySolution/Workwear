using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Views;
using Workwear.Domain.Stock;
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
			yenumIssueType.ItemsEnum = typeof(IssueType);
			yenumIssueType.Binding.AddBinding(ViewModel, v => v.IssueTypeOptions, w => w.SelectedItemOrNull).InitializeFromSource();

			ytreeNomenclature.CreateFluentColumnsConfig<SelectedProtectionTools>()
				.AddColumn("🗹").AddToggleRenderer(x => x.Select).Editing()
				.AddColumn("Название").AddTextRenderer(x => x.Name)
				.Finish();
			var protectionTools = new GenericObservableList<SelectedProtectionTools>(viewModel.ProtectionTools);
			ytreeNomenclature.ItemsDataSource = protectionTools;
			ycheckbuttonAllNomenclature.Sensitive = protectionTools.Any();
			
			ycheckChild.Binding
				.AddBinding(viewModel, vm => vm.AddChildSubdivisions, w => w.Active)
				.InitializeFromSource();
			checkShowSex.Binding.AddBinding(ViewModel, v => v.ShowSex, w => w.Active).InitializeFromSource();
			
			ycheckExcludeInVacation.Binding
				.AddBinding(viewModel, w => w.ExcludeInVacation, v => v.Active)
				.InitializeFromSource();
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

		protected void SelectAll(object sender, EventArgs e)
		{
			var antiSelect = !ViewModel.ProtectionTools.FirstOrDefault().Select;
			foreach(var item in ViewModel.ProtectionTools) { 
				item.Select = antiSelect; 
			}
		}
	}
}

