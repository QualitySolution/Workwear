using QS.Views;
using Workwear.ViewModels.Company;

namespace Workwear.Views.Company {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeGroupItemsView : ViewBase<EmployeeGroupItemsViewModel> {
		public EmployeeGroupItemsView(EmployeeGroupItemsViewModel viewModel) : base(viewModel) 
		{
			this.Build();
		}
	}
}
