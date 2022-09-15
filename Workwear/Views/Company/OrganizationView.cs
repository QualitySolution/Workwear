using QS.Views.Dialog;
using Workwear.Domain.Company;
using workwear.ViewModels.Company;

namespace workwear.Views.Company
{

	public partial class OrganizationView : EntityDialogViewBase<OrganizationViewModel, Organization>
	{
		public OrganizationView(OrganizationViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			entryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			textviewAddress.Binding.AddBinding(Entity, e => e.Address, w => w.Buffer.Text).InitializeFromSource();
		}
	}
}
