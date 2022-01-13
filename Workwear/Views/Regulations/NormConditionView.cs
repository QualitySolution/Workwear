using QS.Views.Dialog;
using workwear.Domain.Regulations;
using workwear.ViewModels.Regulations;

namespace workwear.Views.Regulations
{
	public partial class NormConditionView : EntityDialogViewBase<NormConditionViewModel, NormCondition>
	{
		public NormConditionView(NormConditionViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		public void ConfigureDlg() 
		{
			yenumSex.ItemsEnum = typeof(SexNormCondition);
			yenumSex.Binding.AddBinding(Entity, e => e.SexNormCondition, w => w.SelectedItem).InitializeFromSource();

			yentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
		}
	}
}