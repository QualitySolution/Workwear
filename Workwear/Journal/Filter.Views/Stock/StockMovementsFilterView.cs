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
			entityWarehouse.ViewModel = ViewModel.WarehouseEntry;
			entityWarehouse.Binding.AddBinding(viewModel, v => v.VisibleWarehouse, w => w.Visible).InitializeFromSource();
			labelWarehouse.Binding.AddBinding(viewModel, v => v.VisibleWarehouse, w => w.Visible).InitializeFromSource();
		}
	}
}
