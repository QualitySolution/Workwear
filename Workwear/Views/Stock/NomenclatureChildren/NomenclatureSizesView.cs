using System;
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
				.AddColumn("Размер").MinWidth(60)
					.AddComboRenderer(x => x.Size).SetDisplayFunc(x => x.Name).Editing()
					.DynamicFillListFunc(x => ViewModel.SizeService.GetSize(ViewModel.UoW, x.Nomenclature.Type.SizeType).ToList())
				.AddColumn("Размер").MinWidth(60)
                	.AddComboRenderer(x => x.Height).SetDisplayFunc(x => x.Name).Editing()
                	.DynamicFillListFunc(x => ViewModel.SizeService.GetSize(ViewModel.UoW, x.Nomenclature.Type.HeightType).ToList())
				.AddColumn("Комментарий").AddTextRenderer(p => p.Comment).Editable()
				.Finish();
			ytreeItems.ItemsDataSource = ViewModel.Items;
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
			
			buttonAdd.Clicked += (sender, args) => ViewModel.Add();
			buttonRemove.Clicked += (sender, args) => ViewModel.Remove(ytreeItems.GetSelectedObjects<NomenclatureSizes>().First());
		}
		private void YtreeItems_Selection_Changed(object sender, EventArgs e) {
			buttonRemove.Sensitive = ytreeItems.Selection.CountSelectedRows () > 0;
		}
	}
}
