using System;
using QS.Views;
using Workwear.Domain.Users;
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
			yenumcomboboxAmount.ItemsEnum = typeof(AddedAmount);
			yenumcomboboxAmount.Binding.AddBinding(viewModel, v => v.CanChooseAmount, w => w.Visible).InitializeFromSource();
			yenumcomboboxAmount.Binding.AddBinding(viewModel, v => v.AddAmount, w => w.SelectedItem).InitializeFromSource();
			labelAmount.Binding.AddBinding(viewModel, v => v.CanChooseAmount, w => w.Visible).InitializeFromSource();
		}
	}
}
