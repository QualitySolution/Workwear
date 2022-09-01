using QS.Views.Dialog;
using workwear.ViewModels.Operations;

namespace workwear.Views.Operations 
{
	public partial class ManualEmployeeIssueOperationsView : DialogViewBase<ManualEmployeeIssueOperationsViewModel> 
	{
		public ManualEmployeeIssueOperationsView(ManualEmployeeIssueOperationsViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}
	}
}
