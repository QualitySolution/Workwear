using QS.Views.Dialog;
using Workwear.Domain.Stock.Barcodes;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock 
{
	public partial class BarcodeView : EntityDialogViewBase<BarcodeViewModel, Barcode> 
	{
		public BarcodeView(BarcodeViewModel viewModel) : base(viewModel) 
		{
			this.Build();
		}
	}
}
