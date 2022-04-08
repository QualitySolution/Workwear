using System;
using Gamma.Binding.Converters;
using Gamma.GtkWidgets;
using QS.Views.Dialog;
using workwear.Domain.Sizes;
using Workwear.Measurements;
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
			CreateSuitableTable();
			entityname.Binding
				.AddBinding(Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, v => v.Sensitive)
				.InitializeFromSource();
			labelId.Binding
				.AddBinding(Entity, e => e.Id, w => w.Text, new IdToStringConverter())
				.InitializeFromSource();
			specllistcomSizeType.SetRenderTextFunc<SizeType>(x => x.Name);
			specllistcomSizeType.ItemsList = SizeService.GetSizeType(ViewModel.UoW);
			specllistcomSizeType.Binding
				.AddBinding(Entity, e => e.SizeType, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.IsNew, v => v.Sensitive)
				.InitializeFromSource();
			ycheckbuttonUseInEmployee.Binding
				.AddBinding(Entity, e => e.UseInEmployee, w => w.Active)
				.InitializeFromSource();
			ycheckbuttonUseInNomenclature.Binding
				.AddBinding(Entity, e => e.UseInNomenclature, w => w.Active)
				.InitializeFromSource();

			ybuttonAddSuitable.Clicked += AddAnalog;
			ybuttonRemoveSuitable.Clicked += RemoveAnalog;
			ytreeviewSuitableSizes.Selection.Changed += SelectionOnChanged;
		}

		private void CreateSuitableTable() {
			ytreeviewSuitableSizes.ColumnsConfig = ColumnsConfigFactory.Create<Size>()
				.AddColumn("Значение").AddTextRenderer(x => x.Name)
				.Finish();
			ytreeviewSuitableSizes.HeadersVisible = false;
			ytreeviewSuitableSizes.Binding
				.AddSource(Entity)
				.AddBinding(e => e.ObservableSuitableSizes, w => w.ItemsDataSource)
				.InitializeFromSource();
		}
		private void AddAnalog(object sender, EventArgs eventArgs) => ViewModel.AddAnalog();
		private void RemoveAnalog(object sender, EventArgs eventArgs) {
			var analog = ytreeviewSuitableSizes.GetSelectedObject<Size>();
			ViewModel.RemoveAnalog(analog);
		}
		void SelectionOnChanged(object sender, EventArgs e) => 
			ybuttonRemoveSuitable.Sensitive = ytreeviewSuitableSizes.Selection.CountSelectedRows() > 0;
	}
}
