using System;
using QS.Views;
using Workwear.Domain.ClothingService;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	public partial class ClothingServiceReportView : ViewBase<ClothingServiceReportViewModel> {
		public ClothingServiceReportView(ClothingServiceReportViewModel viewModel) : base (viewModel){
			this.Build();
			comboReportType.ItemsEnum = typeof(ClothingServiceReportType);
			if(ViewModel.HiddenReportType != null)
            	comboStatus.HiddenItems = ViewModel.HiddenReportType;
			comboReportType.Binding
				.AddBinding(ViewModel, vm => vm.ReportType, w => w.SelectedItem).InitializeFromSource();
			ylabelStatus.Binding
				.AddBinding(ViewModel, vm => vm.VisibleShowStatus, w => w.Visible).InitializeFromSource();
			comboStatus.ItemsEnum = typeof(ClaimState);
			if(ViewModel.HiddenStates != null)
				comboStatus.HiddenItems = ViewModel.HiddenStates;
			comboStatus.Binding
				.AddBinding(ViewModel, vm => vm.Status, w => w.SelectedItemOrNull)
				.AddBinding(ViewModel, vm => vm.VisibleShowStatus, w => w.Visible).InitializeFromSource();
			ylabelshowPhone.Binding
				.AddBinding(ViewModel, vm => vm.VisibleShowPhone, w => w.Visible).InitializeFromSource();
			ycheckbuttonshowPhone.Binding
				.AddBinding(ViewModel, vm => vm.VisibleShowPhone, w => w.Visible)
                .AddBinding(ViewModel, vm => vm.ShowPhone, w => w.Active).InitializeFromSource();
			ylabelshowComments.Binding
				.AddBinding(ViewModel, vm => vm.VisibleShowComment, w => w.Visible).InitializeFromSource();
			ycheckbuttonshowComments.Binding
				.AddBinding(ViewModel, vm => vm.VisibleShowComment, w => w.Visible)
                .AddBinding(ViewModel, vm => vm.ShowComments, w => w.Active).InitializeFromSource();
			ylabelPeriod.Binding
            	.AddBinding(ViewModel, vm => vm.ShowClosedLabel, w => w.Text)
            	.AddBinding(ViewModel, vm => vm.VisiblePeriodOfBegitn, w => w.Visible).InitializeFromSource();
            ydateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.AddBinding(vm => vm.VisiblePeriodOfBegitn, w => w.Visible).InitializeFromSource();
			ylabelGroupSubdivision.Binding
				.AddBinding(ViewModel, vm => vm.VisibleGroupSubdivision, v => v.Visible).InitializeFromSource();
			ycheckbuttonGroupSubdivision.Binding
				.AddBinding(ViewModel, vm => vm.VisibleGroupSubdivision, v => v.Visible)
				.AddBinding(ViewModel, vm => vm.GroupSubdivision, w => w.Active).InitializeFromSource();
			ylabelshowclosed.Binding
            	.AddBinding(ViewModel, vm => vm.VisibleShowClosed, v => v.Visible).InitializeFromSource();
            ycheckbuttonshowclosed.Binding
            	.AddBinding(ViewModel, vm => vm.VisibleShowClosed, v => v.Visible)
            	.AddBinding(ViewModel, vm => vm.ShowClosed, w => w.Active).InitializeFromSource();
            ylabelshowEmployee.Binding
            	.AddBinding(ViewModel, vm => vm.VisibleShowEmployees, v => v.Visible).InitializeFromSource();
            ycheckbuttonshowEmployees.Binding
            	.AddBinding(ViewModel, vm => vm.VisibleShowEmployees, v => v.Visible)
            	.AddBinding(ViewModel, vm => vm.ShowEmployees, w => w.Active).InitializeFromSource();
            ylabelshowZero.Binding
                .AddBinding(ViewModel, vm => vm.VisibleShowZero, v => v.Visible).InitializeFromSource();
            ycheckbuttonshowZero.Binding
                .AddBinding(ViewModel, vm => vm.VisibleShowZero, v => v.Visible)
                .AddBinding(ViewModel, vm => vm.ShowZero, w => w.Active).InitializeFromSource();
            buttonRun.Clicked += OnButtonRunClicked;
			buttonRun.Binding
				.AddBinding(ViewModel, vm=>vm.SensetiveLoad, w=>w.Sensitive).InitializeFromSource();
		}
		protected void OnButtonRunClicked(object sender, EventArgs e) => ViewModel.LoadReport();
	}
}
