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
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Date, w => w.Date)
				.AddBinding(vm => vm.DateSensitive, w => w.Sensitive)
				.InitializeFromSource();
			yentryEmployee.Binding
				.AddBinding(ViewModel, vm => vm.EmployeeSensitive, w=> w.Sensitive)
				.InitializeFromSource();
			yentrySubdivision.Binding
				.AddBinding(ViewModel, vm => vm.SubdivisionSensitive, w=> w.Sensitive)
				.InitializeFromSource();

			yentryEmployee.ViewModel = ViewModel.EmployeeEntry;
			yentrySubdivision.ViewModel = ViewModel.SubdivisionEntry;
		}	
	}
}
