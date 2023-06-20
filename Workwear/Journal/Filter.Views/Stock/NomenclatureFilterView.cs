using System;
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
			entityProtectionTools.ViewModel = viewModel.EntryProtectionTools;
			yShowArchival.Binding
				.AddBinding(viewModel, vm => vm.ShowArchival, w => w.Active)
				.InitializeFromSource();
			ycheckbuttonOnlyWithRating.Binding.AddSource(ViewModel)
				.AddBinding(wm => wm.OnlyWithRating, w => w.Active)
				.AddBinding(v => v.OnlyWithRatingVisible, w => w.Visible)
				.InitializeFromSource();
		}
	}
}
