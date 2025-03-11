using System;
using QS.Views;
using Workwear.Journal.Filter.ViewModels.Regulations;

namespace Workwear.Journal.Filter.Views.Regulations {
	public partial class DutyNormBalanceFilterView : ViewBase<DutyNormBalanceFilterViewModel> {
		public DutyNormBalanceFilterView(DutyNormBalanceFilterViewModel viewModel): base(viewModel) {
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg() {
			datepicker.Binding
				.AddSource(ViewModel)
				.AddBinding(vm=>vm.Date, w=>w.Date)
				.AddBinding(vm=>vm.DateSensitive, w=>w.Sensitive)
				.InitializeFromSource();
			yentryDutyNorm.Binding.AddBinding(ViewModel, vm=>vm.DutyNormSensitive, w=>w.Sensitive)
				.InitializeFromSource();
			yentrySubdivision.Binding.AddBinding(ViewModel, vm=>vm.SubdivisionSensitive, w=>w.Sensitive)
				.InitializeFromSource();
			yentryDutyNorm.ViewModel = ViewModel.DutyNormEntry;
			yentrySubdivision.ViewModel=ViewModel.SubdivisionEntry;
		}
	}
}
