using QS.Utilities;
using QS.Views.Dialog;
using QS.Views.Resolve;
using Workwear.Domain.Company;
using Workwear.ViewModels.Company;

namespace Workwear.Views.Company {
	public partial class EmployeeGroupView : EntityDialogViewBase<EmployeeGroupViewModel, EmployeeGroup> {
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public EmployeeGroupView(EmployeeGroupViewModel viewModel, IGtkViewResolver viewResolver) : base(viewModel) {
			this.Build();
			ConfigureDlg(viewResolver);
			CommonButtonSubscription();
		}
		
		private void ConfigureDlg(IGtkViewResolver viewResolver)
		{
			yentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			
			tabs.AppendPage(viewResolver.Resolve(ViewModel.ItemsViewModel), "Сотрудники");
			tabs.Binding.AddBinding(ViewModel, v => v.CurrentTab, w => w.CurrentPage).InitializeFromSource();
		}
	}
}
