using System.Linq;
using Gamma.ColumnConfig;
using QS.Utilities;
using QS.Views;
using Workwear.Domain.ClothingService;
using Workwear.ViewModels.Stock.NomenclatureChildren;

namespace Workwear.Views.Stock.NomenclatureChildren {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NomenclatureServicesView  : ViewBase<NomenclatureServicesViewModel> {
		public NomenclatureServicesView(NomenclatureServicesViewModel viewModel) : base(viewModel) {
			this.Build();

			ytreeItems.ColumnsConfig = FluentColumnsConfig<Service>.Create()
				.AddColumn("ИД").AddReadOnlyTextRenderer(n => n.Id.ToString())
				.AddColumn("Название").AddTextRenderer(p => p.Name).WrapWidth(900)
				.AddColumn("Cтоимость").AddReadOnlyTextRenderer(n => CurrencyWorks.GetShortCurrencyString(n.Cost))
				.AddColumn("Комментарий").AddTextRenderer(p => p.Comment)
				.Finish();
			ytreeItems.ItemsDataSource = ViewModel.ObservableServices;

			buttonAdd.Clicked += (sender, args) => ViewModel.Add();
			buttonRemove.Clicked += (sender, args) => ViewModel.Remove(ytreeItems.GetSelectedObjects<Service>().First());
		}
	}
}
