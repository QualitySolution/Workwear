using QS.Views;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	public partial class ClothingServicesCodeView : ViewBase<ClothingServicesCodeViewModel> {
		public ClothingServicesCodeView(ClothingServicesCodeViewModel viewModel) : base(viewModel) {
			this.Build();
			
			choiceServices.ViewModel = ViewModel.ChoiceServiceViewModel;

			buttonRun.Clicked += (sender, args) => ViewModel.LoadReport();
		}
	}
}
