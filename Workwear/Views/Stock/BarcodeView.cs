using QS.Views.Dialog;
using Workwear.Domain.Stock.Barcodes;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock 
{
	public partial class BarcodeView : EntityDialogViewBase<BarcodeViewModel, Barcode> 
	{
		public BarcodeView(BarcodeViewModel viewModel) : base(viewModel) 
		{
			this.Build();
			
			ylabelCodeValue.Binding
				.AddBinding(Entity, e => e.Value, w => w.Text)
				.InitializeFromSource();
			
			ylabelEmployeeIssueOperation.Binding
				.AddBinding(ViewModel, wm => wm.EmployeeIssueVisible, w => w.Visible)
				.InitializeFromSource();
			
			ylabelEmployeeIssueOperationValue.Binding
				.AddSource(ViewModel)
				.AddBinding(wm => wm.EmployeeIssueVisible, w => w.Visible)
				.AddBinding(wm => wm.EmployeeIssueTitle, w => w.Text)
				.InitializeFromSource();
		}
	}
}
