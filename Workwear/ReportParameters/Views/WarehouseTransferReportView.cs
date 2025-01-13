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
			yspeccomboboxWarehouseExpense.SelectedItemStrictTyped = false;
			yspeccomboboxWarehouseExpense.Binding.AddSource(ViewModel)
				.AddBinding(w=>w.WarehousesExpense,v=>v.ItemsList)
				.AddBinding(w=>w.SelectExpenseWarehouse,v=>v.SelectedItem)
				.InitializeFromSource();
			yspeccomboboxWarehouseReceipt.SelectedItemStrictTyped = false;
			yspeccomboboxWarehouseReceipt.Binding.AddSource(ViewModel)
				.AddBinding(w=>w.WarehousesReceipt,v=>v.ItemsList)
				.AddBinding(w=>w.SelectReceiptWarehouse,v=>v.SelectedItem)
				.InitializeFromSource();
			yspeccomboboxOwner.SelectedItemStrictTyped = false;
			yspeccomboboxOwner.Binding.AddSource(ViewModel)
				.AddBinding(w=>w.Owners, v=>v.ItemsList)
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
