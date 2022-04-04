
using QS.Views;
using workwear.Journal.Filter.ViewModels.Stock;
using Workwear.Measurements;

namespace workwear.Journal.Filter.Views.Stock
{
	public partial class SizeFilterView : ViewBase<SizeFilterViewModel>
	{
		public SizeFilterView(SizeFilterViewModel viewModel): base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}
		private void ConfigureDlg()
		{
			speciallistSizeType.ItemsList = SizeService.GetSizeType(ViewModel.UoW);
			speciallistSizeType.Binding
			.AddBinding(ViewModel, vm => vm.SizeType, v => v.SelectedItem)
				.InitializeFromSource();
		}
	}
}
