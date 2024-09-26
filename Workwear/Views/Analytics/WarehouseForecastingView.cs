using System;
using QS.Views;
using Workwear.ViewModels.Analytics;

namespace Workwear.Views.Analytics {

	public partial class WarehouseForecastingView : ViewBase<WarehouseForecastingViewModel> {

		public WarehouseForecastingView(WarehouseForecastingViewModel viewModel) : base(viewModel) {
			this.Build();

			treeItems.CreateFluentColumnsConfig<WarehouseForecastingItem>()
				.AddColumn("Номенклатура нормы").AddReadOnlyTextRenderer(x => x.ProtectionTool.Name).WrapWidth(500)
				.AddColumn("Размер/Рост").AddReadOnlyTextRenderer(x => x.SizeText)
				.AddColumn("На складе").AddReadOnlyTextRenderer(x => x.InStock.ToString())
				.AddColumn("Просрочено").AddReadOnlyTextRenderer(x => x.Unissued.ToString())
				.Finish();

			treeItems.Binding.AddBinding(ViewModel, v => v.Items, w => w.ItemsDataSource).InitializeFromSource();

			ViewModel.ProgressTotal = progressTotal;
			ViewModel.ProgressLocal = progressLocal;

			buttonFill.Binding.AddBinding(ViewModel, v => v.SensitiveFill, w => w.Sensitive).InitializeFromSource();

			entryWarehouse.ViewModel = ViewModel.WarehouseEntry;
			dateEnd.Binding.AddBinding(ViewModel, v => v.EndDate, w => w.Date).InitializeFromSource();
		}

		protected void OnButtonFillClicked(object sender, EventArgs e) {
			ViewModel.Fill();
		}
	}
}
