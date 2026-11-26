using System;
using QS.Views;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	public partial class NotIssuedDutyNormsView : ViewBase<NotIssuedDutyNormsViewModel> {
		public NotIssuedDutyNormsView(NotIssuedDutyNormsViewModel viewModel) : base(viewModel) {
			this.Build();

			comboReportType.ItemsEnum = typeof(NotIssuedDutyNormsReportType);
			comboReportType.Binding.AddBinding(ViewModel, v => v.ReportType, w => w.SelectedItem).InitializeFromSource();

			ydateReport.Binding.AddBinding(ViewModel, v => v.ReportDate, w => w.DateOrNull).InitializeFromSource();
			dateExcludeBefore.Binding.AddBinding(ViewModel, v => v.ExcludeBefore, w => w.DateOrNull).InitializeFromSource();

			ycomboboxWarehouse.Binding.AddSource(ViewModel)
				.AddBinding(v => v.WarehousesList, w => w.ItemsList)
				.AddBinding(v => v.Warehouse, w => w.SelectedItem)
				.InitializeFromSource();
			ycheckDontShowZeroStock.Binding.AddSource(ViewModel)
				.AddBinding(v => v.ExcludeZeroStock, w => w.Active)
				.AddBinding(v => v.StockElementsSensetive, w => w.Sensitive)
				.InitializeFromSource();
			ycheckHideWorn.Binding.AddSource(ViewModel)
				.AddBinding(v => v.HideWorn, w => w.Active)
				.AddBinding(v => v.StockElementsSensetive, w => w.Sensitive)
				.InitializeFromSource();
			ycheckShowShipment.Binding.AddSource(ViewModel)
				.AddBinding(v => v.ShowShipment, w => w.Active)
				.AddBinding(v => v.StockElementsSensetive, w => w.Sensitive)
				.AddBinding(v => v.VisibleShowShipment, w => w.Visible)
				.InitializeFromSource();

			choiceprotectiontoolsview1.ViewModel = ViewModel.ChoiceProtectionToolsViewModel;
			choicesubdivisionview1.ViewModel = ViewModel.ChoiceSubdivisionViewModel;

			buttonRun.Binding.AddBinding(ViewModel, v => v.SensetiveLoad, w => w.Sensitive).InitializeFromSource();
			buttonRun.Clicked += (sender, e) => ViewModel.LoadReport();
			expander1.Activated += OnExpanderActivated;
			expander2.Activated += OnExpanderActivated;
		}

		private void OnExpanderActivated(object sender, EventArgs e) {
			var expander = (Gtk.Expander)sender;
			((Gtk.Box.BoxChild)dialog1_VBox[expander]).Expand = expander.Expanded;
		}
	}
}
