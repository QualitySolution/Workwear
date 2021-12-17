using QS.Views.Dialog;
using workwear.Domain.Operations;
using workwear.ViewModels.Operations;

namespace workwear.Views.Operations
{
	public partial class ManualEmployeeIssueOperationView : EntityDialogViewBase<ManualEmployeeIssueOperationViewModel, EmployeeIssueOperation>
	{
		public ManualEmployeeIssueOperationView(ManualEmployeeIssueOperationViewModel viewModel) : base(viewModel)
		{
			this.Build();
			CommonButtonSubscription();

			dateIssue.Binding.AddBinding(ViewModel, e => e.IssueDate, w => w.Date).InitializeFromSource();
			spinAmount.Binding.AddBinding(Entity, e => e.Issued, w => w.ValueAsInt).InitializeFromSource();
			labelAmountUnits.Binding.AddBinding(ViewModel, e => e.Units, w => w.LabelProp).InitializeFromSource();
			buttonDelete.Binding.AddBinding(ViewModel, v => v.SensitiveDeleteButton, w => w.Sensitive).InitializeFromSource();
		}

		protected void OnButtonDeleteClicked(object sender, System.EventArgs e)
		{
			ViewModel.Delete();
		}
	}
}
