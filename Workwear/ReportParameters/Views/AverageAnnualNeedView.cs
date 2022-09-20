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

			comboIssueType.ItemsEnum = typeof(IssueType);
			comboIssueType.Binding.AddBinding(ViewModel, v => v.IssueType, w => w.SelectedItemOrNull).InitializeFromSource();
			checkShowSex.Binding.AddBinding(ViewModel, v => v.ShowSex, w => w.Active).InitializeFromSource();

			buttonRun.Binding.AddBinding(ViewModel, v => v.SensetiveLoad, w => w.Sensitive).InitializeFromSource();
			
			ycheckSummry.Binding
				.AddBinding(viewModel, v => v.Summary, w => w.Active)
				.InitializeFromSource();

			entitySubdivision.ViewModel = ViewModel.SubdivisionEntry;
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			ViewModel.LoadReport();
		}
	}
}
