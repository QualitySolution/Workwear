using System;
using QS.Views;
using workwear.ReportParameters.ViewModels;

namespace workwear.ReportParameters.Views
{
	public partial class ListBySizeView : ViewBase<ListBySizeViewModel>
	{
		public ListBySizeView(ListBySizeViewModel viewModel) : base(viewModel)
		{
			this.Build();
			buttonRun.Clicked += OnButtonRunClicked;
			comboReportType.ItemsEnum = typeof(ListBySizeViewModel.SizeReportType);
			comboReportType.Binding.AddBinding(ViewModel, v => v.ReportType, w => w.SelectedItem).InitializeFromSource();
			ycheckbuttongroupbysubdivision.Binding.AddBinding(ViewModel, w => w.GroupBySubdivision, v => v.Active).InitializeFromSource();
			ylabel1.Binding.AddBinding(ViewModel,v => v.VisibleShowGroup, w => w.Visible).InitializeFromSource();
			ycheckbuttongroupbysubdivision.Binding.AddBinding(ViewModel, v => v.VisibleShowGroup, w => w.Visible).InitializeFromSource();
		}
		protected void OnButtonRunClicked(object sender, EventArgs e) => ViewModel.LoadReport();
		
	}
}
