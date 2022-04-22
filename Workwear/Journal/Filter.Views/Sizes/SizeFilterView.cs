using QS.Views;
using workwear.Journal.Filter.ViewModels.Sizes;
using workwear.Journal.Filter.ViewModels.Stock;

namespace workwear.Journal.Filter.Views.Sizes
{
	public partial class SizeFilterView : ViewBase<SizeFilterViewModel>
	{
		public SizeFilterView(SizeFilterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}
		private void ConfigureDlg()
		{
			speciallistSizeType.ItemsList = ViewModel.SizeService.GetSizeType(ViewModel.UoW);
			speciallistSizeType.Binding
				.AddSource(ViewModel)
			.AddBinding(vm => vm.SelectedSizeType, v => v.SelectedItem)
			.AddBinding(vm => vm.SensitiveSizeType, v => v.Sensitive)
				.InitializeFromSource();
		}
	}
}
