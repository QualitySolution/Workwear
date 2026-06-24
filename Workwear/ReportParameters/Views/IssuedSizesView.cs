using QS.Views;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class IssuedSizesView  : ViewBase<IssuedSizesViewModel> {
		public IssuedSizesView(IssuedSizesViewModel viewModel) : base(viewModel) {
			this.Build();
			
			ydateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();
			choicenomenclature1.ViewModel = ViewModel.ChoiceNomenclatureViewModel;
			buttonRun.Clicked += (sender, args) => ViewModel.LoadReport();
		}
	}
}
