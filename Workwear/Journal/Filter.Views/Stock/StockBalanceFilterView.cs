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

			DisableFeatures();
		}
		public void DisableFeatures()
		{
			if(!ViewModel.FeaturesService.Available(Tools.Features.WorkwearFeature.Warehouses)) {
				table1.Visible = false;
			}
		}
	}
}
