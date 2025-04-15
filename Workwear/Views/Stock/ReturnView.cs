using System;
using Gtk;
using QS.Views.Dialog;
using QSOrmProject;
using Workwear.Domain.Stock.Documents;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ReturnView : EntityDialogViewBase<ReturnViewModel, Return> {
		public ReturnView(ReturnViewModel viewModel) : base(viewModel) {
			this.Build();
			ConfigureDlg();
			ConfigureItems();
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
			ylabelCreatedBy.Binding
				.AddFuncBinding(ViewModel, vm => vm.DocCreatedbyUser != null ? vm.DocCreatedbyUser.Name : null, w => w.LabelProp)
				.InitializeFromSource ();
			ydateDoc.Binding
				.AddBinding(ViewModel, vm => vm.DocumentDate, w => w.Date)
				.AddBinding(ViewModel,vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource ();
			ytextComment.Binding
				.AddBinding(ViewModel, vm => vm.DocComment, w => w.Buffer.Text)
				.AddBinding(ViewModel,vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			label_Warehouse.Visible = ViewModel.WarehouseVisible;
			entityWarehouseIncome.ViewModel = ViewModel.WarehouseEntryViewModel;
			entityWarehouseIncome.Visible = ViewModel.WarehouseVisible;
			labelSum.Binding
				.AddBinding(ViewModel, vm => vm.Total, w => w.LabelProp)
				.InitializeFromSource();
			ybuttonAddWorker.Binding
				.AddBinding(ViewModel, vm => vm.CanAddEmployee, w => w.Sensitive).InitializeFromSource();
			ybuttonAddDutyNorm.Binding
				.AddBinding(ViewModel, vm => vm.CanAddDutyNorms, w => w.Sensitive).InitializeFromSource();
			ybuttonDel.Binding
				.AddBinding(ViewModel, vm => vm.CanRemoveItem, w => w.Sensitive).InitializeFromSource();
			ybuttonSetNomenclature.Binding
				.AddBinding(ViewModel, vm => vm.CanSetNomenclature, w => w.Sensitive).InitializeFromSource();
			enumPrint.ItemsEnum = typeof(ReturnViewModel.ReturnDocReportEnum);
		}
		
		private void ConfigureItems() {
			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ReturnItem>()
				.AddColumn("Ном. №")
					.AddReadOnlyTextRenderer(x => x.Nomenclature?.Number)
				.AddColumn("Наименование").Resizable()
					.AddTextRenderer(i => i.ItemName).WrapWidth(700)
					.AddSetter((w, item) => w.Foreground = item.Nomenclature != null ? "black" : "red")
				.AddColumn("Размер").MinWidth(60)
				.AddReadOnlyTextRenderer(i => i.WearSize?.Name ?? "нет")
				.AddColumn("Рост").MinWidth(70)
					.AddReadOnlyTextRenderer(i => i.Height?.Name ?? "нет")
				.AddColumn("Собственник").Resizable()
					.Visible(ViewModel.OwnersVisible)
					.AddComboRenderer(i => i.Owner)
					.SetDisplayFunc(x => x.Name)
					.FillItems(ViewModel.Owners, "нет")
					.Editing(ViewModel.CanEdit)
				.AddColumn("Процент износа")
					.AddNumericRenderer(i => i.WearPercent, new MultiplierToPercentConverter())
					.Editing(new Adjustment(0, 0, 999, 1, 10, 0), ViewModel.CanEdit).WidthChars(6).Digits(0)
					.AddTextRenderer(i => "%", expand: false)
				.AddColumn("Количество")
					.AddNumericRenderer(i => i.Amount)
					.Editing(new Adjustment(0, 0, 100000, 1, 10, 1), ViewModel.CanEdit).WidthChars(2)
					.AddReadOnlyTextRenderer(e => e.Units?.Name)
				.AddColumn("Отметка о износе")
					.AddTextRenderer(e => e.СommentReturn)
					.Editable(ViewModel.CanEdit)
				.Finish();
			
			ytreeItems.ItemsDataSource = ViewModel.Items;
			ytreeItems.Selection.Changed += ytreeItems_Selection_Changed;
		}

		private void ytreeItems_Selection_Changed(object sender, EventArgs e) =>
			ViewModel.SelectedItem = ytreeItems.GetSelectedObject<ReturnItem>();
		protected void OnYbuttonAddDutyNormClicked(object sender, EventArgs e) {
			ViewModel.AddFromDutyNorm();
		}
		protected void OnYbuttonAddWorkerClicked(object sender, EventArgs e) =>
			ViewModel.AddFromEmployee();
		protected void OnYbuttonDelClicked(object sender, EventArgs e) =>
			ViewModel.DeleteItem(ytreeItems.GetSelectedObject<ReturnItem>());
		protected void OnYbuttonSetNomenclatureClicked(object sender, EventArgs e) =>
			ViewModel.SetNomenclature(ytreeItems.GetSelectedObject<ReturnItem>());
		protected void OnEnumPrintEnumItemClicked(object sender, QS.Widgets.EnumItemClickedEventArgs e) {
			var doc = (ReturnViewModel.ReturnDocReportEnum)e.ItemEnum;
			ViewModel.PrintReturnDoc(doc);
		}
	}
}
