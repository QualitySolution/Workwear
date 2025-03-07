using System;
using QS.Views;
using Workwear.Domain.Stock;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class StockAllWearView : ViewBase<StockAllWearViewModel> {
		public StockAllWearView(StockAllWearViewModel viewModel) : base(viewModel) {
			this.Build();
			
			ydateReport.Binding.AddBinding(ViewModel, v => v.ReportDate, w => w.DateOrNull).InitializeFromSource();
			comboReportType.ItemsEnum = typeof(StockAllWearReportType);
			comboReportType.Binding.AddBinding(ViewModel, v => v.ReportType, w => w.SelectedItem).InitializeFromSource();
			yspeccomboboxWarehouse.SelectedItemStrictTyped = false;
			yspeccomboboxWarehouse.Binding
				.AddSource(ViewModel)
				.AddBinding(wh=>wh.Warehouses,w=>w.ItemsList)
				.AddBinding(wh=>wh.SelectWarehouse, w=>w.SelectedItem)
				.AddBinding(v=>v.VisibleWarehouse, w=>w.Visible)
				.InitializeFromSource();
			ylabel_warehouse.Binding
				.AddBinding(viewModel, v => v.VisibleWarehouse, w => w.Visible)
				.InitializeFromSource();
			ycheckbuttonShowSumm.Binding.AddBinding(viewModel, v => v.VisibleSumm, w => w.Visible).InitializeFromSource();
			ycheckbuttonShowSumm.Binding.AddBinding(viewModel, v => v.ShowSumm, w => w.Active).InitializeFromSource();
			ycheckbuttonShowSex.Binding.AddBinding(viewModel,v=>v.ShowSex,w=>w.Active).InitializeFromSource();
			buttonRun.Binding.AddBinding(ViewModel,v=>v.SensitiveLoad, w=>w.Sensitive).InitializeFromSource();
		}
		
		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			ViewModel.LoadReport();
		}
	}
}
