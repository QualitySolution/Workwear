using QS.Views;
using Workwear.Domain.Sizes;
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

			entityWarehouse.Binding
				.AddBinding(viewModel, v => v.VisibleWarehouse, w => w.Visible)
				.InitializeFromSource();
			labelWarehouse.Binding
				.AddBinding(viewModel, v => v.VisibleWarehouse, w => w.Visible)
				.InitializeFromSource();
			comboSize.SetRenderTextFunc<Size>(x => x.Title);
			comboSize.Binding.AddSource(ViewModel)
				.AddBinding(v => v.Size, w => w.SelectedItem)
				.AddBinding(v => v.Sizes, w => w.ItemsList)
				.AddBinding(v => v.SensitiveSize, w => w.Sensitive)
				.InitializeFromSource();
			comboGrowth.SetRenderTextFunc<Size>(x => x.Title);
			comboGrowth.Binding.AddSource(ViewModel)
				.AddBinding(v => v.Height, w => w.SelectedItem)
				.AddBinding(v => v.Growths, w => w.ItemsList)
				.AddBinding(v => v.SensitiveGrowth, w => w.Sensitive)
				.InitializeFromSource();

			entryNomenclature.ViewModel = ViewModel.EntryNomenclature;
			entityWarehouse.ViewModel = ViewModel.WarehouseEntry;

			ycheckCollapse.Binding
				.AddBinding(viewModel, v => v.CollapseOperationItems, w => w.Active)
				.InitializeFromSource();
			
			yenumShowDirectionOperation.ItemsEnum = typeof(DirectionOfOperation);
			yenumShowDirectionOperation.Binding
				.AddBinding(viewModel, v => v.Direction, w => w.SelectedItem)
				.InitializeFromSource();
		}
	}
}
