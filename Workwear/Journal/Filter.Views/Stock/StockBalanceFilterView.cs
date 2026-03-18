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
			ylabelOwner.Binding.AddBinding(viewModel, v => v.VisibleOwners, w => w.Visible).InitializeFromSource();
			yspeccomboboxOwners.Binding.AddBinding(viewModel, v => v.VisibleOwners, w => w.Visible).InitializeFromSource();
			yspeccomboboxOwners.SelectedItemStrictTyped = false;
			yspeccomboboxOwners.Binding
				.AddBinding(viewModel, v => v.Owners, w => w.ItemsList)
				.AddBinding(viewModel, v => v.SelectOwner, w => w.SelectedItem)
				.InitializeFromSource();

			ydateDate.Binding.AddBinding(viewModel, v=> v.Date, w => w.Date).InitializeFromSource();
			ydateDate.Binding.AddBinding(viewModel, v => v.SensitiveDate, w => w.Sensitive).InitializeFromSource();
			yenumcomboboxAmount.ItemsEnum = typeof(AddedAmount);
			yenumcomboboxAmount.Binding.AddBinding(viewModel, v => v.CanChooseAmount, w => w.Visible).InitializeFromSource();
			yenumcomboboxAmount.Binding.AddBinding(viewModel, v => v.AddAmount, w => w.SelectedItem).InitializeFromSource();
			labelAmount.Binding.AddBinding(viewModel, v => v.CanChooseAmount, w => w.Visible).InitializeFromSource();
		}
	}
}
