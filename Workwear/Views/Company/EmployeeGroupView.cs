using QS.Views.Dialog;
using QS.Views.Resolve;
using Workwear.Domain.Company;
using Workwear.ViewModels.Company;

namespace Workwear.Views.Company {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeGroupView : EntityDialogViewBase<EmployeeGroupViewModel, EmployeeGroup> {
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public EmployeeGroupView(EmployeeGroupViewModel viewModel, IGtkViewResolver viewResolver) : base(viewModel) {
			this.Build();
			//ConfigureDlg(viewResolver);
			CommonButtonSubscription();
		}
	}
}
