using System;
using QS.Views;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	public partial class WarehouseTransferReportView : ViewBase<WarehouseTransferReportViewModel> {
		public WarehouseTransferReportView(WarehouseTransferReportViewModel viewModel): base(viewModel) {
			this.Build();
			ydateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(w=>w.StartDate, v=>v.StartDateOrNull)
				.AddBinding(w=>w.EndDate, v=>v.EndDateOrNull)
				.InitializeFromSource();
			yenumcomboboxExpenseWarehouse.Binding.AddSource(ViewModel)
				.AddBinding(w=>w.Warehouses,v=>v.ItemsEnum)
				.AddBinding(w=>w.SelectWarehouse, v=>v.SelectedItem)
				.InitializeFromSource();
			yenumcomboboxReceiptWarehouse.Binding.AddSource(ViewModel)
				.AddBinding(w=>w.Warehouses,v=>v.ItemsEnum)
				.AddBinding(w=>w.SelectWarehouse, v=>v.SelectedItem)
				.InitializeFromSource();
			yenumcomboboxOwner.Binding.AddSource(ViewModel)
				.AddBinding(w=>w.Owners,v=>v.ItemsEnum)
				.AddBinding(w=>w.SelectOwner, v=>v.SelectedItem)
				.InitializeFromSource();
			buttonRun.Clicked += OnButtonRunClicked;
			buttonRun.Binding.AddBinding(ViewModel,v=>v.SensetiveLoad,w=>w.Sensitive).InitializeFromSource();
		}
		protected void OnButtonRunClicked(object sender, EventArgs e) {
			ViewModel.LoadReport();
		}
	}
}
