using QS.Views.Dialog;
using workwear.Domain.Regulations;
using workwear.ViewModels.Regulations;

namespace workwear.Views.Regulations
{
	public partial class NormConditionView : EntityDialogViewBase<NormConditionViewModel, NormCondition>
	{
		public NormConditionView(NormConditionViewModel viewModel) : base(viewModel)
		{
			yenumSex.ItemsEnum = typeof(SexNormCondition);
			yenumSex.Binding.AddBinding(ViewModel, v => v.sexNormCondition, w => w.SelectedItem).InitializeFromSource();

			yentryName.Binding.AddBinding(viewModel, v => v.Name, w => w.Text).InitializeFromSource();
		}
	}
}
