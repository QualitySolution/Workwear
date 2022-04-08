using System;
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
			ybuttonRemoveSize.Sensitive = false;
			ybuttonAddSize.Clicked += AddSize;
			ybuttonRemoveSize.Clicked += RemoveSize;
			ViewModel.Sizes.ListContentChanged += CreateSizeTable;
			CreateSizeTable(null, null);
			entityname.Binding
				.AddBinding(Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, v => v.Sensitive)
				.InitializeFromSource();
			labelId.Binding
				.AddBinding(Entity, e => e.Id, w => w.Text, new IdToStringConverter())
				.InitializeFromSource();
			specllistcomCategory.ItemsEnum = typeof(Category);
			specllistcomCategory.Binding
				.AddBinding(Entity, e => e.Category, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.IsNew, v => v.Sensitive)
				.InitializeFromSource();
			yspinPosition.Binding
			.AddBinding(Entity, e => e.Position, w => w.ValueAsInt)
				.InitializeFromSource();
			ytreeviewSizes.Selection.Changed += SelectionOnChanged;
		}

		private void RemoveSize(object sender, EventArgs e) {
			ViewModel.RemoveSize(ytreeviewSizes.GetSelectedObject<Size>());
		}

		void SelectionOnChanged(object sender, EventArgs e) {
			var sizeId = ytreeviewSizes.GetSelectedObject<Size>()?.Id;
			ybuttonRemoveSize.Sensitive = sizeId >= 1000;
		}

		private void AddSize(object sender, EventArgs e) {
			ViewModel.AddSize();
		} 
		private void CreateSizeTable(object aList, EventArgs eventArgs) {
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
