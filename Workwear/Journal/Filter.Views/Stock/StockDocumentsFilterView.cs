using System.Linq;
using QS.Views;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.Filter.ViewModels.Stock;

namespace workwear.Journal.Filter.Views.Stock
{
	public partial class StockDocumentsFilterView : ViewBase<StockDocumentsFilterViewModel>
	{
		public StockDocumentsFilterView(StockDocumentsFilterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			enumcomboDocumentType.ItemsEnum = typeof(StockDocumentType);
			enumcomboDocumentType.Binding.AddBinding(ViewModel, v => v.StockDocumentType, w => w.SelectedItemOrNull).InitializeFromSource();
			enumcomboDocumentType.AddEnumToHideList(viewModel.HidenStockDocumentTypeList.ToArray());
			dateperiodDocs.Binding.AddSource(ViewModel)
				.AddBinding(v => v.StartDate, w => w.StartDateOrNull)
				.AddBinding(v => v.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			entityWarehouse.ViewModel = ViewModel.WarehouseEntry;
			entityWarehouse.Binding.AddBinding(viewModel, v => v.VisibleWarehouse, w => w.Visible).InitializeFromSource();
			labelWarehouse.Binding.AddBinding(viewModel, v => v.VisibleWarehouse, w => w.Visible).InitializeFromSource();
		}
	}
}
