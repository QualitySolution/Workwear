using QS.Views.Dialog;
using workwear.Domain.Company;
using workwear.ViewModels.Company;

namespace workwear.Views.Company
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class LeadersView : EntityDialogViewBase<LeadersViewModel, Leader>
	{
		public LeadersView(LeadersViewModel viewModel ) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			tbFirstName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			tbSurName.Binding.AddBinding(Entity, e => e.Surname, w => w.Text).InitializeFromSource();
			tbPatronymic.Binding.AddBinding(Entity, e => e.Patronymic, w=> w.Text).InitializeFromSource();
			tbPosition.Binding.AddBinding(Entity, e => e.Position, w => w.Text).InitializeFromSource();
		}
	}
}
