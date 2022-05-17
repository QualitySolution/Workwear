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
		}
		protected void OnButtonRunClicked(object sender, EventArgs e) => ViewModel.LoadReport();
	}
}
