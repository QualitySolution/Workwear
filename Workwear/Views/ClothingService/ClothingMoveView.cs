using System;
using Gamma.Utilities;
using QS.ViewModels.Control;
using QS.Views;
using Workwear.Domain.ClothingService;
using Workwear.ViewModels.ClothingService;

namespace Workwear.Views.ClothingService {

	public partial class ClothingMoveView : ViewBase<ClothingMoveViewModel> {
		public ClothingMoveView(ClothingMoveViewModel viewModel) : base(viewModel) {
			this.Build();

			barcodeinfoview1.ViewModel = ViewModel.BarcodeInfoViewModel;

			entrySearchBarcode.Binding
				.AddBinding(ViewModel.BarcodeInfoViewModel, e => e.BarcodeText, w => w.Text)
				.AddBinding(ViewModel, v => v.SensitiveBarcode, w => w.Sensitive).InitializeFromSource();
			buttonAddClaim.Binding
				.AddBinding(ViewModel, vm => vm.CanAddClaim, w => w.Visible).InitializeFromSource();
			buttonAccept.Binding
				.AddBinding(ViewModel, v => v.SensitiveAccept, w => w.Sensitive).InitializeFromSource();
			ybuttonPrintLabel.Binding
				.AddBinding(ViewModel, v=>v.SensitivePrint, w=>w.Sensitive)
				.InitializeFromSource();
			ycheckbuttonNeedRepair.Binding
				.AddBinding(ViewModel, vm => vm.NeedRepair, w => w.Active).InitializeFromSource();
			textDefect.Binding
				.AddBinding(ViewModel, vm => vm.DefectText, w => w.Buffer.Text).InitializeFromSource();
			yhboxDefect.Binding
				.AddBinding(ViewModel, vm => vm.NeedRepair, w => w.Visible).InitializeFromSource();
			comboState.ItemsEnum = typeof(ClaimState);
			comboState.HiddenItems = new object[] { ClaimState.WaitService, ClaimState.InDispenseTerminal, ClaimState.InReceiptTerminal, ClaimState.DeliveryToDispenseTerminal };
			comboState.Binding
				.AddBinding(ViewModel, v => v.State, w => w.SelectedItem).InitializeFromSource();
			textComment.Binding
				.AddBinding(ViewModel, v => v.Comment, w => w.Buffer.Text).InitializeFromSource();

			treeOperations.CreateFluentColumnsConfig<StateOperation>()
				.AddColumn("Время").AddReadOnlyTextRenderer(x => x.OperationTime.ToString("g"))
				.AddColumn("Статус").AddReadOnlyTextRenderer(x => x.State.GetEnumTitle())
				.AddColumn("Пользователь").AddReadOnlyTextRenderer(x => x.User?.Name)
				.AddColumn("Комментарий").AddReadOnlyTextRenderer(x => x.Comment)
				.Finish();
			treeOperations.Binding.AddSource(ViewModel)
				.AddBinding(v => v.Operations, w => w.ItemsDataSource).InitializeFromSource();
			
			treeServices.CreateFluentColumnsConfig<SelectableEntity<Service>>()			
				.AddColumn("☑").AddToggleRenderer(x => x.Select).Editing()
				.AddColumn("Услуга").AddReadOnlyTextRenderer(x => x.Label)
				.Finish();
			treeServices.Binding.AddSource(ViewModel)
				.AddBinding(v => v.Services, w => w.ItemsDataSource).InitializeFromSource();
		}

		protected void OnButtonAcceptClicked(object sender, EventArgs e) =>
			ViewModel.Accept();
		protected void OnYbuttonPrintLabelClicked(object sender, EventArgs e) =>
			ViewModel.PrintLabel();
		protected void OnButtonAddClaimClicked(object sender, EventArgs e) => 
			ViewModel.CreateNew();
	}
}
