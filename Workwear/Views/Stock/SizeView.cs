using Gamma.Binding.Converters;
using QS.Views.Dialog;
using workwear.Domain.Sizes;
using workwear.ViewModels.Stock;

namespace workwear.Views.Stock
{
	public partial class SizeView : EntityDialogViewBase<SizeViewModel, Size>
	{
		public SizeView(SizeViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			entityname.Binding
				.AddBinding(Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();
			labelId.Binding
				.AddBinding(Entity, e => e.Id, w => w.Text, new IdToStringConverter())
				.InitializeFromSource();
			specllistcomSizeType.Binding
				.AddBinding(Entity, e => e.SizeType, w => w.SelectedItem)
				.InitializeFromSource();
			ycheckbuttonUseInEmployee.Binding
				.AddBinding(Entity, e => e.UseInEmployee, w => w.Active)
				.InitializeFromSource();
			ycheckbuttonUseInNomenclature.Binding
				.AddBinding(Entity, e => e.UseInNomenclature, w => w.Active)
				.InitializeFromSource();
		}
	}
}
