using QS.Views;
using Workwear.Domain.Users;
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
			ycheckbuttonShowAll.Binding
				.AddBinding(ViewModel, vm => vm.CheckShowAll, w=> w.Active)
				.AddBinding(ViewModel, vm => vm.CheckShowWriteoffVisible, w=> w.Visible)
				.InitializeFromSource();
			ylabelCheckbuttonShowAll.Binding
				.AddBinding(ViewModel, vm => vm.CheckShowWriteoffVisible, w=> w.Visible)
				.InitializeFromSource();
			yenumcomboboxAmount.ItemsEnum = typeof(AddedAmount);
			yenumcomboboxAmount.Binding.
				AddBinding(ViewModel, v => v.CanChooseAmount, w => w.Visible)
				.InitializeFromSource();
			yenumcomboboxAmount.Binding
				.AddBinding(ViewModel, v => v.AddAmount, w => w.SelectedItem)
				.InitializeFromSource();
			labelAmount.Binding
				.AddBinding(ViewModel, v => v.CanChooseAmount, w => w.Visible)
				.InitializeFromSource();

			yentryEmployee.ViewModel = ViewModel.EmployeeEntry;
			yentrySubdivision.ViewModel = ViewModel.SubdivisionEntry;
		}	
	}
}
