using Gamma.Binding.Converters;
using QS.Views.Dialog;
using workwear.Domain.Sizes;
using workwear.ViewModels.Stock;

namespace workwear.Views.Stock
{
	public partial class SizeTypeView : EntityDialogViewBase<SizeTypeViewModel, SizeType>
	{
		public SizeTypeView(SizeTypeViewModel viewModel) : base(viewModel)
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
			specllistcomCategory.ItemsEnum = typeof(Category);
			specllistcomCategory.Binding
				.AddBinding(Entity, e => e.Category, w => w.SelectedItem)
				.InitializeFromSource();
			yspinPosition.Binding
			.AddBinding(Entity, e => e.Position, w => w.ValueAsInt)
				.InitializeFromSource();
		}
	}
}
