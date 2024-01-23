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
			buttonCalculateExpence.Clicked += (sender, args) => ViewModel.CalculateExpense();

			ytreeviewOperations.ColumnsConfig = ColumnsConfigFactory.Create<EmployeeIssueOperation>()
				.AddColumn("ИД").AddReadOnlyTextRenderer(x => x.Id.ToString())
				.AddColumn("Дата выдачи").AddTextRenderer(x => x.OperationTime.ToShortDateString())
				.AddColumn("Количество").AddNumericRenderer(x => x.Issued)
				.AddColumn("Окончание носки").AddTextRenderer(x => $"{x.AutoWriteoffDate:d}")
				.AddColumn("Процент износа").AddTextRenderer(x => x.WearPercent.ToString("P0"))
				.Finish();
			
			ytreeviewOperations.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Operations, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectOperation, w => w.SelectedRow)
				.InitializeFromSource();

			ydatepicker.Binding
				.AddSource(ViewModel)
				.AddBinding(wm => wm.IssueDate, w => w.Date)
				.AddBinding(wm => wm.CanEditOperation, w => w.Sensitive)
				.InitializeFromSource();
			
			dateExpense.Binding
				.AddSource(ViewModel)
				.AddBinding(wm => wm.AutoWriteoffDate, w => w.DateOrNull)
				.AddBinding(wm => wm.CanEditOperation, w => w.Sensitive)
				.InitializeFromSource();
			
			spinExpenseMonths.Binding
				.AddSource(ViewModel)
				.AddBinding(wm => wm.ExpiredMonths, w => w.ValueAsInt)
				.AddBinding(wm => wm.CanEditOperation, w => w.Sensitive)
				.InitializeFromSource();
			
			hboxManualCalculate.Binding.AddBinding(ViewModel, v => v.VisibleManualCalculate, w => w.Visible).InitializeFromSource();

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
			
			yspinbuttonWearPercent.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.WearPercent, w => w.ValueAsDecimal)
				.AddBinding(wm => wm.CanEditOperation, w => w.Sensitive)
				.InitializeFromSource();
			
			ycheckOverrideBefore.Binding
				.AddSource(ViewModel)
				.AddBinding(wm => wm.OverrideBefore, w => w.Active)
				.AddBinding(wm => wm.CanEditOperation, w => w.Sensitive)
				.InitializeFromSource();
			
			ytextComment.Binding.
				AddBinding(ViewModel , v => v.Comment, w => w.Buffer.Text)
				.InitializeFromSource();

			#region Штрихкоды
			ylabelBarcodeTitle.Binding.AddBinding(ViewModel, v => v.VisibleBarcodes, w => w.Visible).InitializeFromSource();
			labelBarcodes.Binding.AddSource(ViewModel)
				.AddBinding(v => v.VisibleBarcodes, w => w.Visible)
				.AddBinding(v => v.BarcodesText, w => w.Text)
				.AddBinding(v => v.BarcodesColor, w => w.ForegroundColor)
				.InitializeFromSource();
			hboxBarcodeButtons.Binding.AddBinding(ViewModel, v => v.VisibleBarcodes, w => w.Visible).InitializeFromSource();
			buttonPrintBarcodes.Binding.AddBinding(ViewModel, v => v.SensitiveBarcodesPrint, w => w.Sensitive).InitializeFromSource();
			buttonPrintBarcodes.Clicked += (sender, args) => ViewModel.PrintBarcodes();
			buttonCreateOrDeleteBarcodes.Binding.AddSource(ViewModel)
				.AddBinding(v => v.ButtonCreateOrRemoveBarcodesTitle, w => w.Label)
				.AddBinding(v => v.SensitiveCreateBarcodes, w => w.Sensitive)
				.InitializeFromSource();
			buttonCreateOrDeleteBarcodes.Clicked += (sender, args) => ViewModel.ReleaseBarcodes();
			#endregion

			#region Кнопки журнала
			ybuttonDelete.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanEditOperation, w => w.Sensitive)
				.InitializeFromSource();
			
			ybuttonAdd.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanAddOperation, w => w.Sensitive)
				.InitializeFromSource();
			#endregion
		}
		private void ButtonDeleteOnClicked(object sender, EventArgs e) => ViewModel.DeleteOnClicked(
			ytreeviewOperations.GetSelectedObject<EmployeeIssueOperation>());
		private void ButtonAddOnClicked(object sender, EventArgs e) => ViewModel.AddOnClicked();
	}
}
