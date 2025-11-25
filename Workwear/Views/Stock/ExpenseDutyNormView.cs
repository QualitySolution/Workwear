using System;
using System.Linq;
using Gamma.ColumnConfig;
using Gtk;
using QS.Views.Dialog;
using QS.Widgets;
using Workwear.Domain.Statements;
using Workwear.Domain.Stock.Documents;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ExpenseDutyNormView : EntityDialogViewBase<ExpenseDutyNormViewModel, ExpenseDutyNorm> {
		public ExpenseDutyNormView(ExpenseDutyNormViewModel viewModel) : base(viewModel) {
			Build();
			ConfigureDlg();
			CommonButtonSubscription();
			
		}

		private void ConfigureDlg() {
			entryId.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DocNumberText, w => w.Text)
				.AddBinding(vm => vm.SensitiveDocNumber, w => w.Sensitive)
				.InitializeFromSource();
			checkAuto.Binding
				.AddBinding(ViewModel, vm => vm.AutoDocNumber, w => w.Active)
				.AddBinding(ViewModel,vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource(); 
			
			ylabelCreatedBy.Binding.AddFuncBinding(Entity, e => e.CreatedbyUser != null ? e.CreatedbyUser.Name : null, w => w.LabelProp).InitializeFromSource();
			ydateDoc.Binding
				.AddBinding(ViewModel, vm => vm.DocumentDate, w => w.Date)
				.AddBinding(ViewModel,vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel,vm => vm.CanChangeDocDate, w => w.IsEditable)
				.InitializeFromSource();
			ytextComment.Binding
				.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text)
				.AddBinding(ViewModel,vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			yentryNorm.ViewModel = ViewModel.DutyNormEntryViewModel;  			
			yentryWarehouseExpense.ViewModel = ViewModel.WarehouseEntryViewModel;
			yentryResponsible.ViewModel = ViewModel.ResponsibleEmployeeCardEntryViewModel;
			
			ybuttonAdd.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			ybuttonDel.Binding
				.AddBinding(ViewModel, vm => vm.CanDelSelectedItem, w => w.Sensitive).InitializeFromSource();
			ybuttonChoosePositions.Binding
				.AddBinding(ViewModel, vm => vm.CanChooseStockPositionsSelectedItem, w => w.Sensitive).InitializeFromSource();
			buttonIssuanceSheetOpen.Binding
				.AddBinding(ViewModel,v=>v.IssuanceSheetOpenVisible, w=>w.Visible)
				.AddBinding(ViewModel,v=>v.IssuanceSheetCreateSensitive, w=>w.Sensitive).InitializeFromSource();
			buttonIssuanceSheetCreate.Binding
				.AddBinding(ViewModel, v=>v.IssuanceSheetCreateVisible, w=>w.Visible).InitializeFromSource();
			enumPrint.ItemsEnum=typeof(IssuedSheetPrint);
			enumPrint.Binding
				.AddBinding(ViewModel, v=>v.IssuanceSheetPrintVisible, w=>w.Visible).InitializeFromSource();

			ytreeItems.Binding
				.AddBinding(ViewModel, vm => vm.SelectedItem, w => (ExpenseDutyNormItem)w.SelectedRow);
			ytreeItems.ItemsDataSource = Entity.Items;
			ytreeItems.ColumnsConfig = FluentColumnsConfig<ExpenseDutyNormItem>.Create()
				.AddColumn("Номенклатура нормы").Resizable().AddComboRenderer(x => x.ProtectionTools)
					.SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => ViewModel.ProtectionToolsListFromNorm.ToList())
					.Editing(ViewModel.CanEdit)
				.AddColumn("Номенклатура").Resizable()
					.AddTextRenderer(node => node.Nomenclature != null ? node.Nomenclature.Name : "")
					.WrapWidth(700)
				.AddColumn("Размер")
					.AddComboRenderer(x => x.WearSize).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => ViewModel.SizeService.GetSize(ViewModel.UoW, x.Nomenclature?.Type?.SizeType, onlyUseInNomenclature:true).ToList())
					.AddSetter((c, n) => c.Editable = ViewModel.CanEdit && n.Nomenclature?.Type?.SizeType != null)
				.AddColumn("Рост")
					.AddComboRenderer(x => x.Height).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => ViewModel.SizeService.GetSize(ViewModel.UoW, x.Nomenclature?.Type?.HeightType, onlyUseInNomenclature:true).ToList())
					.AddSetter((c, n) => c.Editable = ViewModel.CanEdit && n.Nomenclature?.Type?.HeightType != null) 
					.AddColumn("Износ").AddTextRenderer(e => (e.WearPercent).ToString("P0"))
				.AddColumn("Количество").AddNumericRenderer(e => e.Amount).Editing(new Adjustment(0, 0, 100000, 1, 10, 1), ViewModel.CanEdit)
					.AddTextRenderer(e => 
					e.Nomenclature != null && e.Nomenclature.Type != null && e.Nomenclature.Type.Units != null ? e.Nomenclature.Type.Units.Name : null)
				.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = ViewModel.GetRowColor(n))
				.Finish();

		}
		
		protected void OnButtonColorsLegendClicked(object sender, EventArgs e) => 
			ViewModel.ShowLegend();
		protected void OnYbuttonChoosePositionsClicked(object sender, EventArgs e) =>
			ViewModel.ChooseStockPosition(ytreeItems.GetSelectedObject<ExpenseDutyNormItem>());
		protected void OnYbuttonAddClicked(object sender, EventArgs e) => 
			ViewModel.AddItems();
		protected void OnYbuttonDelClicked(object sender, EventArgs e) {
			foreach(var item in ytreeItems.GetSelectedObjects<ExpenseDutyNormItem>())
				ViewModel.DeleteItem(item);
		}

		protected void OnButtonIssuanceSheetCreateClicked(object sender, EventArgs e) =>
			ViewModel.CreateIssuanceSheet();
		protected void OnButtonIssuanceSheetOpenClicked(object sender, EventArgs e) =>
			ViewModel.OpenIssuanceSheet();
		protected void OnEnumPrintEnumItemClicked(object sender, EnumItemClickedEventArgs e) =>
			ViewModel.PrintIssuanceSheet((IssuedSheetPrint)e.ItemEnum);
	}
}
