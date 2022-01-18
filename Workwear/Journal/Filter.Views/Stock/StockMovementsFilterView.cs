using System;
using QS.Views;
using workwear.Journal.Filter.ViewModels.Stock;

namespace workwear.Journal.Filter.Views.Stock
{
	public partial class StockMovementsFilterView : ViewBase<StockMovementsFilterViewModel>
	{
		public StockMovementsFilterView(StockMovementsFilterViewModel viewModel) : base(viewModel)
		{
			this.Build();

			dateperiodDocs.Binding.AddSource(ViewModel)
				.AddBinding(v => v.StartDate, w => w.StartDateOrNull)
				.AddBinding(v => v.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			labelStockPosition.Binding.AddFuncBinding(ViewModel, v => v.StockPositionTitle, w => w.LabelProp).InitializeFromSource();
			entityWarehouse.Binding.AddBinding(viewModel, v => v.VisibleWarehouse, w => w.Visible).InitializeFromSource();
			labelWarehouse.Binding.AddBinding(viewModel, v => v.VisibleWarehouse, w => w.Visible).InitializeFromSource();
			comboSize.Binding.AddSource(ViewModel)
				.AddBinding(v => v.SensitiveSize, w => w.Sensitive)
				.AddBinding(v => v.Size, w => w.ActiveText)
				.AddBinding(v => v.Sizes, w => w.ItemsList)
				.InitializeFromSource();
			comboGrowth.Binding.AddSource(ViewModel)
				.AddBinding(v => v.SensitiveGrowth, w => w.Sensitive)
				.AddBinding(v => v.Growth, w => w.ActiveText)
				.AddBinding(v => v.Growths, w => w.ItemsList)
				.InitializeFromSource();

			entryNomenclature.ViewModel = ViewModel.EntryNomenclature;
			entityWarehouse.ViewModel = ViewModel.WarehouseEntry;

			ycheckCollapse.Binding.AddBinding(viewModel, v => v.CollapseOperationItems, w => w.Active).InitializeFromSource();
		}
	}
}
