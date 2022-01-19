using System;
using QS.Views;
using workwear.Journal.Filter.ViewModels.Stock;

namespace workwear.Journal.Filter.Views.Stock
{
	public partial class StockBalanceFilterView : ViewBase<StockBalanceFilterViewModel>
	{
		public StockBalanceFilterView(StockBalanceFilterViewModel viewModel) : base(viewModel)
		{
			this.Build();

			chShowNegative.Binding.AddBinding(viewModel, vm => vm.ShowNegativeBalance, w => w.Active).InitializeFromSource();
			entityWarehouse.ViewModel = ViewModel.WarehouseEntry;
			entityWarehouse.Binding.AddBinding(viewModel, v => v.VisibleWarehouse, w => w.Visible).InitializeFromSource();
			labelWarehouse.Binding.AddBinding(viewModel, v => v.VisibleWarehouse, w => w.Visible).InitializeFromSource();
			ydateDate.Binding.AddBinding(viewModel, v=> v.Date, w => w.Date).InitializeFromSource();
		}
	}
}
