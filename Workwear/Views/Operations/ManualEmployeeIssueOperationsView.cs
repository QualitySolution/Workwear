using System;
using Gamma.GtkWidgets;
using QS.Views.Dialog;
using workwear.Domain.Operations;
using workwear.ViewModels.Operations;

namespace workwear.Views.Operations 
{
	public partial class ManualEmployeeIssueOperationsView : DialogViewBase<ManualEmployeeIssueOperationsViewModel> 
	{
		public ManualEmployeeIssueOperationsView(ManualEmployeeIssueOperationsViewModel viewModel) : base(viewModel)
		{
			this.Build();
			
			buttonCancel.Clicked += ButtonCancelOnClicked;
			buttonSave.Clicked += ButtonSaveOnClicked;
			ybuttonAdd.Clicked += ButtonAddOnClicked;
			ybuttonDelete.Clicked += ButtonDeleteOnClicked;

			ytreeviewOperations.ColumnsConfig = ColumnsConfigFactory.Create<EmployeeIssueOperation>()
				.AddColumn("Дата").AddTextRenderer(x => x.OperationTime.ToShortDateString())
				.AddColumn("Дата окончания носки")
					.AddTextRenderer(x => x.ExpiryByNorm.Value.ToShortDateString())
				.AddColumn("Количество").AddNumericRenderer(x => x.Issued)
				.Finish();
			
			ytreeviewOperations.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Operations, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectOperation, w => w.SelectedRow)
				.InitializeFromSource();

			ydatepicker.Binding
				.AddSource(ViewModel)
				.AddBinding(wm => wm.DateTime, w => w.DateOrNull)
				.AddBinding(wm => wm.CanEditOperation, w => w.Sensitive)
				.InitializeFromSource();
			
			yspinbuttonAmmount.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Issued, w => w.ValueAsInt)
				.AddBinding(wm => wm.CanEditOperation, w => w.Sensitive)
				.InitializeFromSource();
			
			ybuttonDelete.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanEditOperation, w => w.Sensitive)
				.InitializeFromSource();
			
			ybuttonAdd.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanAddOperation, w => w.Sensitive)
				.InitializeFromSource();
		}
		private void ButtonDeleteOnClicked(object sender, EventArgs e) => ViewModel.DeleteOnClicked(
			ytreeviewOperations.GetSelectedObject<EmployeeIssueOperation>());
		private void ButtonAddOnClicked(object sender, EventArgs e) => ViewModel.AddOnClicked();
		private void ButtonSaveOnClicked(object sender, EventArgs e) => ViewModel.SaveOnClicked();
		private void ButtonCancelOnClicked(object sender, EventArgs e) => ViewModel.CancelOnClicked();
	}
}
