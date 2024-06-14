using System;
using QS.Views.Dialog;
using Workwear.Domain.Stock.Documents;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ReturnView : EntityDialogViewBase<ReturnViewModel, Return> {
		public ReturnView(ReturnViewModel viewModel) : base(viewModel) {
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg() {
			/*
			entryId.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DocNumber, w => w.Text)
				.AddBinding(vm => vm.SensitiveDocNumber, w => w.Sensitive)
				.InitializeFromSource();
			checkAuto.Binding.AddBinding(ViewModel, vm => vm.AutoDocNumber, w => w.Active).InitializeFromSource();
			*/
			
		}
	}
}
