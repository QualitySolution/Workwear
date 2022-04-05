using System;
using System.ComponentModel;
using Gamma.Binding.Converters;
using Gamma.GtkWidgets;
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
			ybuttonAddSize.Sensitive = !ViewModel.IsNew;
			ybuttonRemoveSize.Sensitive = !ViewModel.IsNew;
			ybuttonAddSize.Clicked += AddSize;
			ybuttonRemoveSize.Visible = false;
			ViewModel.Sizes.PropertyOfElementChanged += CreateSizeTable;
			CreateSizeTable(null, null);
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
			ytreeviewSizes.Selection.Changed += SelectionOnChanged;
		}

		void SelectionOnChanged(object sender, EventArgs e) {
			ybuttonRemoveSize.Sensitive = ytreeviewSizes.Selection.CountSelectedRows() > 0;
		}
		private void AddSize(object sender, EventArgs e) => ViewModel.AddSize();
		private void CreateSizeTable(object sender, PropertyChangedEventArgs propertyChangedEventArgs) {
			ytreeviewSizes.ColumnsConfig = ColumnsConfigFactory.Create<Size>()
				.AddColumn("Название").AddTextRenderer(x => x.Title)
				.Finish();
			ytreeviewSizes.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Sizes, w => w.ItemsDataSource)
				.InitializeFromSource();
		}
	}
}
