using System;
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
			//entitySubdivision.Binding.AddBinding(ViewModel, v => v.Subdivision, w => w.cha).InitializeFromSource();
			entityDepartment.ViewModel = ViewModel.EntryDepartmentViewModel;
			choiceprotectiontoolsview1.ViewModel = ViewModel.ChoiceProtectionToolsViewModel;
			
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

			ycheckChild.Binding
				.AddBinding(ViewModel, vm => vm.AddChildSubdivisions, w => w.Active)
				.InitializeFromSource();
			checkShowSex.Binding.AddBinding(ViewModel, v => v.ShowSex, w => w.Active).InitializeFromSource();
			
			ycheckExcludeInVacation.Binding
				.AddBinding(ViewModel, w => w.ExcludeInVacation, v => v.Active)
				.InitializeFromSource();

			buttonRun.Binding.AddBinding(ViewModel, v => v.SensitiveRunReport, w => w.Sensitive).InitializeFromSource();
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			ViewModel.LoadReport();
		}
	}
}

