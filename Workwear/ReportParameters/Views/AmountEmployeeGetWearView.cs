using QS.Views;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AmountEmployeeGetWearView : ViewBase<AmountEmployeeGetWearViewModel> {
		public AmountEmployeeGetWearView(AmountEmployeeGetWearViewModel viewModel) : base(viewModel) {
			this.Build();

			ydateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();
			buttonPrintReport.Sensitive = ViewModel.SensitiveLoad;
			ViewModel.PropertyChanged += (sender, args) => {
				if(args.PropertyName == nameof(ViewModel.SensitiveLoad))
					buttonPrintReport.Sensitive = ViewModel.SensitiveLoad;
			};
			buttonPrintReport.Clicked += (sender, args) => ViewModel.LoadReport();
		}
	}
}
