using QS.Navigation;
using QS.Views.Dialog;
using Workwear.ViewModels.Communications;

namespace Workwear.Views.Communications {
	public partial class DeductSpecCoinsView : DialogViewBase<DeductSpecCoinsViewModel>
	{
		public DeductSpecCoinsView(DeductSpecCoinsViewModel viewModel) : base(viewModel) 
		{
			Build();
			
			ycoinsAmountSpin.Binding.AddBinding(ViewModel, vm => vm.DeductCoinsAmount, w => w.ValueAsInt).InitializeFromSource();
			ydescriptionText.Binding.AddBinding(ViewModel, vm => vm.Description, w => w.Buffer.Text).InitializeFromSource();
			ybuttonConfirm.Binding.AddBinding(ViewModel, vm => vm.SensitiveDeductButton, w => w.Sensitive).InitializeFromSource();
		}

		protected void OnConfirmButtonClicked(object sender, System.EventArgs e) 
		{
			ViewModel.DeductCoins();
		}

		protected void OnButtonCancel(object sender, System.EventArgs e) 
		{
			ViewModel.Close(false, CloseSource.Self);
		}
	}
}
