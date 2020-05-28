using System;
using Gamma.Binding.Converters;
using QS.Views.Dialog;
using workwear.Domain.Regulations;
using workwear.ViewModels.Regulations;

namespace workwear.Views.Regulations
{
	public partial class ProfessionView : EntityDialogViewBase<ProfessionViewModel, Profession>
	{
		public ProfessionView(ProfessionViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			entryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();

			entryCode.ValidationMode = QS.Widgets.ValidationType.Numeric;
			entryCode.Binding.AddBinding(Entity, e => e.Code, w => w.Text, new UintToStringConverter()).InitializeFromSource();
		}
	}
}
