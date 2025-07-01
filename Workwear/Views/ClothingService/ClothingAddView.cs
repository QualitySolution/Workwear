using QS.Views;
using Workwear.ViewModels.ClothingService;

namespace Workwear.Views.ClothingService {
	public partial class ClothingAddView  : ViewBase<ClothingAddViewModel> {
		public ClothingAddView(ClothingAddViewModel viewModel) : base(viewModel) {
			this.Build();
			
			barcodeinfoview1.ViewModel = ViewModel.BarcodeInfoViewModel;

			entrySearchBarcode.Binding
				.AddBinding(ViewModel.BarcodeInfoViewModel, e => e.BarcodeText, w => w.Text).InitializeFromSource();
		}
	}
}
