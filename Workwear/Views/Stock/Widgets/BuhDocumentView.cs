using System;
using QS.Views.Dialog;
using Workwear.ViewModels.Stock.Widgets;

namespace workwear.Views.Stock.Widgets
{
	public partial class BuhDocumentView : DialogViewBase<BuhDocumentViewModel>
	{
		public BuhDocumentView(BuhDocumentViewModel viewModel) : base(viewModel)
		{
			this.Build();
			yentryBuhDocText.Binding
				.AddBinding(ViewModel, vm => vm.BuhDocText, w => w.Text)
				.InitializeFromSource();
			
			ybuttonChange.Clicked += AddChange;
			ybuttonCancel.Clicked += Cancel;

		}
		private void AddChange(object sender, EventArgs eventArgs) => ViewModel.AddChange();
		private void Cancel(object sender, EventArgs eventArgs) => ViewModel.Cancel();
	}
}
