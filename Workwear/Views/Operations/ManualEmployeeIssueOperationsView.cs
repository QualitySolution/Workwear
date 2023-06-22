using System;
using Gamma.GtkWidgets;
using QS.Views.Dialog;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.ViewModels.Operations;

namespace Workwear.Views.Operations 
{
	public partial class ManualEmployeeIssueOperationsView : SavedDialogViewBase<ManualEmployeeIssueOperationsViewModel> 
	{
		public ManualEmployeeIssueOperationsView(ManualEmployeeIssueOperationsViewModel viewModel) : base(viewModel)
		{
			this.Build();
			
			CommonButtonSubscription();
			
			ybuttonAdd.Clicked += ButtonAddOnClicked;
			ybuttonDelete.Clicked += ButtonDeleteOnClicked;

			ytreeviewOperations.ColumnsConfig = ColumnsConfigFactory.Create<EmployeeIssueOperation>()
				.AddColumn("Дата выдачи").AddTextRenderer(x => x.OperationTime.ToShortDateString())
				.AddColumn("Количество").AddNumericRenderer(x => x.Issued)
				.AddColumn("Окончание носки").AddTextRenderer(x => $"{x.ExpiryByNorm:d}")
				.Finish();
			
			ytreeviewOperations.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Operations, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectOperation, w => w.SelectedRow)
				.InitializeFromSource();

			ydatepicker.Binding
				.AddSource(ViewModel)
				.AddBinding(wm => wm.DateTime, w => w.Date)
				.AddBinding(wm => wm.CanEditOperation, w => w.Sensitive)
				.InitializeFromSource();

			entityNomenclature.ViewModel = ViewModel.NomenclatureEntryViewModel;

			labelSize.Binding.AddBinding(ViewModel, v => v.VisibleSize, w => w.Visible).InitializeFromSource();
			comboSize.SetRenderTextFunc<Size>(x => x.Name);
			comboSize.Binding.AddSource(ViewModel)
				.AddBinding(v => v.Sizes, w => w.ItemsList)
				.AddBinding(v => v.Size, w => w.SelectedItem)
				.AddBinding(v => v.VisibleSize, w => w.Visible)
				.InitializeFromSource();

			labelHeight.Binding.AddBinding(ViewModel, v => v.VisibleHeight, w => w.Visible).InitializeFromSource();
			comboHeight.SetRenderTextFunc<Size>(x => x.Name);
			comboHeight.Binding.AddSource(ViewModel)
				.AddBinding(v => v.Heights, w => w.ItemsList)
				.AddBinding(v => v.Height, w => w.SelectedItem)
				.AddBinding(v => v.VisibleHeight, w => w.Visible)
				.InitializeFromSource();

			yspinbuttonAmmount.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Issued, w => w.ValueAsInt)
				.AddBinding(wm => wm.CanEditOperation, w => w.Sensitive)
				.InitializeFromSource();
			
			ycheckOverrideBefore.Binding
				.AddSource(ViewModel)
				.AddBinding(wm => wm.OverrideBefore, w => w.Active)
				.AddBinding(wm => wm.CanEditOperation, w => w.Sensitive)
				.InitializeFromSource();

			ylabelBarcodeTitle.Binding.AddBinding(ViewModel, v => v.VisibleBarcodes, w => w.Visible).InitializeFromSource();
			labelBarcodes.Binding.AddBinding(ViewModel, v => v.VisibleBarcodes, w => w.Visible).InitializeFromSource();
			hboxBarcodeButtons.Binding.AddBinding(ViewModel, v => v.VisibleBarcodes, w => w.Visible).InitializeFromSource();
			
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
	}
}
