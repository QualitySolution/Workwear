using QS.Views;
using Workwear.Journal.Filter.ViewModels.Company;

namespace Workwear.Journal.Filter.Views.Company {

	public partial class DepartmentFilterView : ViewBase<DepartmentFilterViewModel> {
		public DepartmentFilterView(DepartmentFilterViewModel viewModel) : base(viewModel) {
			this.Build();

			entitySubdivision.ViewModel = viewModel.EntrySubdivision;
		}
	}
}
