using QS.Views;
using workwear.Domain.Sizes;
using workwear.Journal.Filter.ViewModels.Stock;

namespace workwear.Journal.Filter.Views.Stock
{
	public partial class SizeTypeFilterView : ViewBase<SizeTypeFilterViewModel>
	{
		public SizeTypeFilterView(SizeTypeFilterViewModel viewModel): base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}
		private void ConfigureDlg()
		{
			yenumcomboCategory.ItemsEnum = typeof(Category);
			yenumcomboCategory.Binding
			.AddBinding(ViewModel, vm => vm.Category, v => v.SelectedItemOrNull)
				.InitializeFromSource();
		}
	}
}
