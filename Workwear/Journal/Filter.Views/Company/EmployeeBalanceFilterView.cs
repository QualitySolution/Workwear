using QS.Views;
using workwear.Journal.Filter.ViewModels.Company;

namespace workwear.Journal.Filter.Views.Company
{
	public partial class EmployeeBalanceFilterView : ViewBase<EmployeeBalanceFilterViewModel>
	{
		public EmployeeBalanceFilterView(EmployeeBalanceFilterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}
		private void ConfigureDlg()
		{
			datepicker.Binding
				.AddBinding(ViewModel, vm => vm.Date, w => w.Date)
				.InitializeFromSource();
			yentryEmployee.ViewModel = ViewModel.EmployeeEntry;
		}	
	}
}
