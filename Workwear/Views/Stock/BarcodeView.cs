using System;
using QS.Views.Dialog;
using Workwear.Domain.Stock;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock 
{
	public partial class BarcodeView : EntityDialogViewBase<BarcodeViewModel, Barcode> 
	{
		public BarcodeView(BarcodeViewModel viewModel) : base(viewModel) 
		{
			this.Build();
			CommonButtonSubscription();
			
			ylabelCodeValue.Binding
				.AddBinding(Entity, e => e.Title, w => w.Text)
				.InitializeFromSource();
			
			ylabelEmployeeIssueOperation.Binding
				.AddBinding(ViewModel, wm => wm.EmployeeIssueVisible, w => w.Visible)
				.InitializeFromSource();
			
			ylabelEmployeeIssueOperationValue.Binding
				.AddSource(ViewModel)
				.AddBinding(wm => wm.EmployeeIssueVisible, w => w.Visible)
				.AddBinding(wm => wm.EmployeeIssueTitle, w => w.Text)
				.InitializeFromSource();
			
			ybuttonDeleteEmployeeIssueOperation.Binding
				.AddBinding(ViewModel, wm => wm.EmployeeIssueVisible, w => w.Visible)
				.InitializeFromSource();
			
			ylabelOperations.Binding
				.AddBinding(ViewModel, wm => wm.OperationsTitle, w => w.Text)
				.InitializeFromSource();

			ybuttonDeleteEmployeeIssueOperation.Clicked += YButtonDeleteEmployeeIssueOperationOnActivated;
		}

		private void YButtonDeleteEmployeeIssueOperationOnActivated(object sender, EventArgs e) => ViewModel.DeleteEmployeeIssue();
	}
}
