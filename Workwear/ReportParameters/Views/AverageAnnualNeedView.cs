using System;
using QS.Views;
using Workwear.Domain.Stock;
using workwear.ReportParameters.ViewModels;

namespace workwear.ReportParameters.Views
{
	public partial class AverageAnnualNeedView : ViewBase<AverageAnnualNeedViewModel>
	{

		public AverageAnnualNeedView(AverageAnnualNeedViewModel viewModel) : base(viewModel)
		{
			this.Build();

			labelIssueType.Binding.AddBinding(ViewModel, v => v.VisibleIssueType, w => w.Visible).InitializeFromSource();
			comboIssueType.ItemsEnum = typeof(IssueType);
			comboIssueType.Binding.AddSource(ViewModel)
				.AddBinding(v => v.IssueType, w => w.SelectedItemOrNull)
				.AddBinding(v => v.VisibleIssueType, w => w.Visible)
				.InitializeFromSource();
			checkShowSex.Binding.AddBinding(ViewModel, v => v.ShowSex, w => w.Active).InitializeFromSource();
			ycheckShowSize.Binding.AddBinding(ViewModel, v => v.ShowSize, w => w.Active).InitializeFromSource();

			buttonRun.Binding.AddBinding(ViewModel, v => v.SensetiveLoad, w => w.Sensitive).InitializeFromSource();
			
			ycheckSummry.Binding
				.AddBinding(ViewModel, v => v.Summary, w => w.Active)
				.InitializeFromSource();

			entitySubdivision.ViewModel = ViewModel.SubdivisionEntry;
			choiceemployeegroupview1.ViewModel = ViewModel.ChoiceEmployeeGroupViewModel;
			choiceemployeegroupview1.Visible = ViewModel.VisibleChoiceEmployeeGroup;
			expander1.Visible = ViewModel.VisibleChoiceEmployeeGroup;
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			ViewModel.LoadReport();
		}
	}
}
