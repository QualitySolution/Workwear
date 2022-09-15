using QS.Views.Dialog;
using Workwear.Domain.Company;
using workwear.ViewModels.Company;

namespace workwear.Views.Company
{
	public partial class EmployeeVacationView : EntityDialogViewBase<EmployeeVacationViewModel, EmployeeVacation>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		public EmployeeVacationView(EmployeeVacationViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}
		private void ConfigureDlg()
		{
			yentryVacationType.ItemsList = ViewModel.UoW.GetAll<VacationType>();
			yentryVacationType.Binding
				.AddBinding(Entity, e => e.VacationType, w => w.SelectedItem)
				.InitializeFromSource();
			
			ydateperiodVacation.Binding
				.AddSource(Entity)
				.AddBinding(e => e.BeginDate, w => w.StartDate)
				.AddBinding(e => e.EndDate, w => w.EndDate)
				.InitializeFromSource();
			
			ytextviewComments.Binding
				.AddBinding(Entity, e => e.Comments, w => w.Buffer.Text)
				.InitializeFromSource();
		}
	}
}
