using System;
using QS.Views;
using workwear.Domain.Stock;
using workwear.Journal.Filter.ViewModels.Stock;
using workwear.Tools.Features;

namespace workwear.Journal.Filter.Views.Stock
{
	public partial class StockDocumentsFilterView : ViewBase<StockDocumentsFilterViewModel>
	{
		public StockDocumentsFilterView(StockDocumentsFilterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			enumcomboDocumentType.ItemsEnum = typeof(StokDocumentType);
			enumcomboDocumentType.Binding.AddBinding(ViewModel, v => v.StokDocumentType, w => w.SelectedItemOrNull).InitializeFromSource();

			if(!ViewModel.FeaturesService.Available(WorkwearFeature.MassExpense))
				enumcomboDocumentType.AddEnumToHideList(StokDocumentType.MassExpense);

			if(!ViewModel.FeaturesService.Available(WorkwearFeature.CollectiveExpense))
				enumcomboDocumentType.AddEnumToHideList(StokDocumentType.CollectiveExpense);

			if(!ViewModel.FeaturesService.Available(WorkwearFeature.Warehouses))
				enumcomboDocumentType.AddEnumToHideList(StokDocumentType.TransferDoc);


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
