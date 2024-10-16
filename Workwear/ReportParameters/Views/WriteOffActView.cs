using System;
using QS.Views;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	public partial class WriteOffActView : ViewBase<WriteOffActViewModel> {
		public WriteOffActView(WriteOffActViewModel viewModel): base(viewModel) {
			this.Build();
			ydateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(w=>w.StartDate, v=>v.StartDateOrNull)
				.AddBinding(w=>w.EndDate, v=>v.EndDateOrNull)
				.InitializeFromSource();
			buttonRun.Clicked += OnButtonRunClicked;
			buttonRun.Binding.AddBinding(ViewModel,v=>v.SensetiveLoad,w=>w.Sensitive).InitializeFromSource();
		}

		protected void OnButtonRunClicked(object sender, EventArgs e) {
			ViewModel.LoadReport();
		}
	}
}
