using System.Linq;
using Gamma.ColumnConfig;
using QS.Views;
using Workwear.Domain.Stock;
using Workwear.ViewModels.Stock.NomenclatureChildren;

namespace Workwear.Views.Stock.NomenclatureChildren {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NomenclatureSizesView : ViewBase<NomenclatureSizesViewModel> {
		public NomenclatureSizesView(NomenclatureSizesViewModel viewModel) : base(viewModel) {
			this.Build();

			ytreeItems.ColumnsConfig = FluentColumnsConfig<NomenclatureSizes>.Create()
				.AddColumn("Размер").AddTextRenderer(p => p.Size.Name)
				.AddColumn("Рост").AddTextRenderer(p => p.Height.Name)
				.AddColumn("Комментарий").AddTextRenderer(p => p.Comment)
				.Finish();
			ytreeItems.ItemsDataSource = ViewModel.ObservableItems;

			
			buttonAdd.Clicked += (sender, args) => ViewModel.Add();
			buttonRemove.Clicked += (sender, args) => ViewModel.Remove(ytreeItems.GetSelectedObjects<NomenclatureSizes>().First());
		}
	}
}
