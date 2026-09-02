using System;
using QS.Views;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	public partial class WearCardsReportView : ViewBase<WearCardsReportViewModel> {
		public WearCardsReportView(WearCardsReportViewModel viewModel) : base(viewModel) {
			this.Build();

			ycheckbuttonOnlyWorking.Binding.AddBinding(ViewModel, v=>v.OnlyWorking, w=>w.Active).InitializeFromSource();
			ydateActualEmployee.Binding.AddBinding(ViewModel, v=>v.ActualDate, w=>w.Date).InitializeFromSource();

			ycheckbuttonHire.Binding.AddBinding(ViewModel, v=>v.OnlyHired, w=>w.Active).InitializeFromSource();
			ydateperiodpicker_.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.HirePeriodStart, w => w.StartDate)
				.AddBinding(vm => vm.HirePeriodEnd, w => w.EndDate)
				.InitializeFromSource();

			ycheckbuttonDismiss.Binding.AddBinding(ViewModel, v=>v.OnlyDismissed, w=>w.Active).InitializeFromSource();
			ydateperiodpicker_dismiss.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DismissPeriodStart, w => w.StartDate)
				.AddBinding(vm => vm.DismissPeriodEnd, w => w.EndDate)
				.InitializeFromSource();

			ycheckbuttonOnlyWithoutNorms.Binding.AddBinding(ViewModel, v=>v.WithoutNorms, w=>w.Active).InitializeFromSource();
			ycheckbuttonOnlyWithNorms.Binding.AddBinding(ViewModel, v=>v.WithNorms, w=>w.Active).InitializeFromSource();

			ycheckbuttonShowPhone.Binding.AddBinding(ViewModel, v=>v.ShowPhone, w=>w.Active).InitializeFromSource();
		}
		protected void OnYbuttonRunClicked(object sender, EventArgs e) {
			ViewModel.LoadReport();
		}
	}
}
