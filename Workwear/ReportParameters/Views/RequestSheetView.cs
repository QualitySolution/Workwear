using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Utilities;
using QS.Utilities.Text;
using QS.Views;
using workwear.ReportParameters.ViewModels;
using Workwear.Domain.Stock;

namespace workwear.ReportParameters.Views {
	public partial class RequestSheetView : ViewBase<RequestSheetViewModel>
	{
		public RequestSheetView(RequestSheetViewModel viewModel) : base(viewModel)
		{
			this.Build();
			entitySubdivision.ViewModel = ViewModel.EntrySubdivisionViewModel;

			labelIssueType.Binding.AddBinding(ViewModel, v => v.VisibleIssueType, w => w.Visible).InitializeFromSource();
			yenumIssueType.ItemsEnum = typeof(IssueType);
			yenumIssueType.Binding.AddSource(ViewModel)
				.AddBinding(v => v.IssueTypeOptions, w => w.SelectedItemOrNull)
				.AddBinding(v => v.VisibleIssueType, w => w.Visible)
				.InitializeFromSource();

			spinStartYear.Binding.AddBinding(ViewModel, v => v.BeginYear, w => w.ValueAsInt).InitializeFromSource();
			spinEndYear.Binding.AddBinding(ViewModel, v => v.EndYear, w => w.ValueAsInt).InitializeFromSource();

			comboStartMonth.SetRenderTextFunc<int>(x => DateHelper.GetMonthName(x).StringToTitleCase());
			comboStartMonth.ItemsList = Enumerable.Range(1, 12);
			comboStartMonth.Binding.AddBinding(ViewModel, v => v.BeginMonth, w => w.SelectedItem).InitializeFromSource();

			comboEndMonth.SetRenderTextFunc<int>(x => DateHelper.GetMonthName(x).StringToTitleCase());
			comboEndMonth.ItemsList = Enumerable.Range(1, 12);
			comboEndMonth.Binding.AddBinding(ViewModel, v => v.EndMonth, w => w.SelectedItem).InitializeFromSource();

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

			buttonRun.Binding.AddBinding(ViewModel, v => v.SensetiveRunReport, w => w.Sensitive).InitializeFromSource();
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			ViewModel.LoadReport();
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

