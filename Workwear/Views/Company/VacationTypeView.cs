using QS.Views.Dialog;
using workwear.Domain.Company;
using workwear.ViewModels.Company;

namespace workwear.Views.Company
{
	public partial class VacationTypeView : EntityDialogViewBase<VacationTypeViewModel,VacationType>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		public VacationTypeView(VacationTypeViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}
		private void ConfigureDlg()
		{
			yentryName.Binding
				.AddBinding(Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();
			
			ycheckExcludeTime.Binding
				.AddBinding(Entity, e => e.ExcludeFromWearing, w => w.Active)
				.InitializeFromSource();
			
			ytextviewComments.Binding
				.AddBinding(Entity, e => e.Comments, w => w.Buffer.Text)
				.InitializeFromSource();
		}
	}
}
