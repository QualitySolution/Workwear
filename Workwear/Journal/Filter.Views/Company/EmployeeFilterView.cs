using System;
using QS.Views;
using workwear.Journal.Filter.ViewModels.Company;

namespace workwear.Journal.Filter.Views.Company
{
	public partial class EmployeeFilterView : ViewBase<EmployeeFilterViewModel>
	{
		public EmployeeFilterView(EmployeeFilterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			checkShowOnlyWork.Binding.AddBinding(ViewModel, vm => vm.ShowOnlyWork, w => w.Active).InitializeFromSource();
			entitySubdivision.ViewModel = viewModel.SubdivisionEntry;
			entityDepartment.ViewModel = viewModel.DepartmentEntry;
		}
	}
}
