using QS.Views;
using Workwear.Domain.Sizes;
using workwear.Journal.Filter.ViewModels.Sizes;

namespace workwear.Journal.Filter.Views.Sizes
{
	public partial class SizeTypeFilterView : ViewBase<SizeTypeFilterViewModel>
	{
		public SizeTypeFilterView(SizeTypeFilterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}
		private void ConfigureDlg()
		{
			yenumcomboCategory.ItemsEnum = typeof(CategorySizeType);
			yenumcomboCategory.Binding
			.AddBinding(ViewModel, vm => vm.Category, v => v.SelectedItemOrNull)
				.InitializeFromSource();
		}
	}
}
