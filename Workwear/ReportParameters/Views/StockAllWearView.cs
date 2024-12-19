using System;
using QS.Views;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class StockAllWearView : ViewBase<StockAllWearViewModel> {
		public StockAllWearView(StockAllWearViewModel viewModel) : base(viewModel) {
			this.Build();
			
			ydateReport.Binding.AddBinding(ViewModel, v => v.ReportDate, w => w.DateOrNull).InitializeFromSource();
			comboReportType.ItemsEnum = typeof(StockAllWearReportType);
			comboReportType.Binding.AddBinding(ViewModel, v => v.ReportType, w => w.SelectedItem).InitializeFromSource();
			yenumcomboboxWarehouse.Binding
				.AddBinding(viewModel, v => v.VisibleWarehouse, w => w.Visible)
				.InitializeFromSource();
			ylabel_warehouse.Binding
				.AddBinding(viewModel, v => v.VisibleWarehouse, w => w.Visible)
				.InitializeFromSource();
			ycheckbuttonShowSumm.Binding.AddBinding(viewModel, v => v.VisibleSumm, w => w.Visible).InitializeFromSource();
			ycheckbuttonShowSumm.Binding.AddBinding(viewModel, v => v.ShowSumm, w => w.Active).InitializeFromSource();
			ycheckbuttonShowSex.Binding.AddBinding(viewModel,v=>v.ShowSex,w=>w.Active).InitializeFromSource();
		}
		
		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			ViewModel.LoadReport();
		}
	}
}
