using System;
using QS.ViewModels.Control.EEVM;
using QS.Views;
using workwear.Journal.Filter.ViewModels.Stock;

namespace workwear.Journal.Filter.Views.Stock
{
	public partial class NomenclatureFilterView : ViewBase<NomenclatureFilterViewModel>
	{
		public NomenclatureFilterView(NomenclatureFilterViewModel viewModel) : base(viewModel)
		{
			this.Build();

			entityItemsType.ViewModel = viewModel.EntryItemsType;
			yShowArchival.Binding.AddBinding(viewModel, vm => vm.ShowArchival, w => w.Active).InitializeFromSource();
		}
	}
}
