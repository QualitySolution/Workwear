using NLog;
using QS.Views.Dialog;
using Workwear.Domain.Company;
using Workwear.ViewModels.Company;

namespace Workwear.Views.Company {
	public partial class SubdivisionView : EntityDialogViewBase<SubdivisionViewModel, Subdivision>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public SubdivisionView(SubdivisionViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		void ConfigureDlg()
		{
			entryCode.Binding.AddBinding(Entity, e => e.Code, w => w.Text).InitializeFromSource();
			entryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			textviewAddress.Binding.AddBinding(Entity, e => e.Address, w => w.Buffer.Text).InitializeFromSource();

			entitywarehouse.ViewModel = ViewModel.EntryWarehouse;
			entitySubdivision.ViewModel = ViewModel.EntrySubdivisionViewModel;

			lbWarehouse.Visible = entitywarehouse.Visible = ViewModel.VisibleWarehouse;
		}
	}
}
 
