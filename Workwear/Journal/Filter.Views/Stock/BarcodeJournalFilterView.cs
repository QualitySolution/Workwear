using QS.Views;
using Workwear.Domain.Sizes;
using Workwear.Journal.Filter.ViewModels.Stock;

namespace Workwear.Journal.Filter.Views.Stock {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class BarcodeJournalFilterView : ViewBase<BarcodeJournalFilterViewModel>{
		public BarcodeJournalFilterView(BarcodeJournalFilterViewModel viewModel) : base(viewModel) {
			this.Build();
			
			entityWarehouse.ViewModel = ViewModel.WarehouseEntry;
			entryNomenclature.ViewModel = ViewModel.NomenclatureEntry;
			
			yspeccomboboxSize.SetRenderTextFunc<Size>(x => x.Title);
			yspeccomboboxSize.Binding.AddSource(ViewModel)
				.AddBinding(v => v.Size, w => w.SelectedItem)
				.AddBinding(v => v.Sizes, w => w.ItemsList)
				.AddBinding(v => v.HasSize, w => w.Sensitive)
				.InitializeFromSource();
			yspeccomboboxHeight.SetRenderTextFunc<Size>(x => x.Title);
			yspeccomboboxHeight.Binding.AddSource(ViewModel)
				.AddBinding(v => v.Height, w => w.SelectedItem)
				.AddBinding(v => v.Heights, w => w.ItemsList)
				.AddBinding(v => v.HasHeight, w => w.Sensitive)
				.InitializeFromSource();
			
		}
	}
}
