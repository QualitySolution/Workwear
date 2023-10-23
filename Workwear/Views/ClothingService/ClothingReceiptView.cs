using System;
using QS.Views;
using Workwear.ViewModels.ClothingService;

namespace Workwear.Views.ClothingService {

	public partial class ClothingReceiptView : ViewBase<ClothingReceiptViewModel> {
		public ClothingReceiptView(ClothingReceiptViewModel viewModel) : base(viewModel) {
			this.Build();
			
			barcodeinfoview1.ViewModel = ViewModel.BarcodeInfoViewModel;
			
			entrySearchBarcode.Binding
				.AddBinding(ViewModel.BarcodeInfoViewModel, e => e.BarcodeText, w => w.Text)
				.InitializeFromSource();
			
			checkNeedForRepair.Binding
				.AddBinding(ViewModel, v => v.NeedRepair, w => w.Active)
				.InitializeFromSource();
			
			textDefect.Binding
				.AddSource(ViewModel)
				.AddBinding(v => v.Defect, w => w.Buffer.Text)
				.AddBinding(v => v.SensitiveDefect, w => w.Sensitive)
				.InitializeFromSource();

			buttonAccept.Binding
				.AddBinding(ViewModel, v => v.SensitiveAccept, w => w.Sensitive)
				.InitializeFromSource();
		}

		protected void OnButtonAcceptClicked(object sender, EventArgs e) {
			ViewModel.Accept();
		}
	}
}
