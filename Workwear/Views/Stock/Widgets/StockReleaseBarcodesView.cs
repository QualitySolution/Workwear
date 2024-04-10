using QS.Navigation;
using QS.Views.Dialog;
using Workwear.ViewModels.Stock.Widgets;

namespace Workwear.Views.Stock.Widgets 
{
	public partial class StockReleaseBarcodesView : DialogViewBase<StockReleaseBarcodesViewModel>
	{
		public StockReleaseBarcodesView(StockReleaseBarcodesViewModel viewModel) : base(viewModel)
		{
			this.Build();
			
			amountSpin.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.MaxAmount, w => w.Adjustment.Upper)
				.AddBinding(vm => vm.SelectedAmount, w => w.ValueAsInt)
				.InitializeFromSource();
			createBadrodesButton.Binding
				.AddBinding(ViewModel, vm => vm.ConfirmButtonSensetive, w => w.Sensitive)
				.InitializeFromSource();
		}

		protected void OnButtonCancel(object sender, System.EventArgs e)
		{
			ViewModel.Close(false, CloseSource.Self);
		}

		protected void OnCreateBarcodesButtonClicked(object sender, System.EventArgs e) 
		{
			ViewModel.CreateBarcodes();
		}
	}
}
