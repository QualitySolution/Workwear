using System;
using System.Collections.Generic;
using Gtk;
using QS.Views.Dialog;
using QSOrmProject;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock.Documents;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class IncomeView : EntityDialogViewBase<IncomeViewModel, Income> {
		public IncomeView(IncomeViewModel viewModel) : base(viewModel) {
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
			checkAuto.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.AutoDocNumber, w => w.Active)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource(); 
			ylabelCreatedBy.Binding
				.AddFuncBinding(ViewModel, vm => vm.DocCreatedbyUser != null ? vm.DocCreatedbyUser.Name : null, w => w.LabelProp)
				.InitializeFromSource ();
			ydateDoc.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DocumentDate, w => w.Date)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel,vm => vm.CanChangeDocDate, w => w.IsEditable)
				.InitializeFromSource ();
			yentryNumber.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.NumberTN, w => w.Text)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			ytextComment.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DocComment, w => w.Buffer.Text)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			ylabelWarehouse.Binding
				.AddBinding(ViewModel, vm => vm.WarehouseVisible, w => w.Visible)
				.InitializeFromSource();
			entityWarehouse.ViewModel = ViewModel.WarehouseEntryViewModel;
			entityWarehouse.Binding
				.AddBinding(ViewModel, vm => vm.WarehouseVisible, w => w.Visible)
				.InitializeFromSource();
			ylabelShipment.Binding
				.AddBinding(ViewModel, vm => vm.ShipmentLabel, w => w.LabelProp)
				.AddBinding(ViewModel, vm => vm.ShowShipment, w => w.Visible)
				.InitializeFromSource();
			entityShipment.ViewModel = ViewModel.ShipmentEntryViewModel;
			entityShipment.Binding
				.AddBinding(ViewModel, vm => vm.ShowShipment, w => w.Visible)
				.InitializeFromSource();
			ybuttonReadInFile.Binding
				.AddBinding(ViewModel, vm => vm.CanReadFile, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.ReadInFileVisible, w => w.Visible)
				.InitializeFromSource();
			
			labelSum.Binding
				.AddBinding(ViewModel, vm => vm.Total, w => w.LabelProp)
				.InitializeFromSource();
			
			ybuttonAdd.Binding.AddBinding(ViewModel, vm => vm.CanAddItem, w => w.Sensitive).InitializeFromSource();
			ybuttonDel.Binding.AddBinding(ViewModel, vm => vm.CanRemoveItem, w => w.Sensitive).InitializeFromSource();
			ybuttonAddSizes.Binding.AddBinding(ViewModel, vm => vm.CanAddSize, w => w.Sensitive).InitializeFromSource();
		}

		private void ConfigureItems() {
			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<IncomeItem>()
				.AddColumn("Ном. №")
					.AddReadOnlyTextRenderer(x => x.Nomenclature?.Number)
				.AddColumn("Наименование").Resizable()
					.AddTextRenderer(e => e.ItemName).WrapWidth(700)
					.AddSetter((w, item) => w.Foreground = item.Nomenclature != null ? "black" : "red")
				.AddColumn("Сертификат").Resizable()
					.AddTextRenderer(e => e.Certificate).Editable(ViewModel.CanEdit)
				.AddColumn("Размер").MinWidth(60)
					.AddComboRenderer(x => x.WearSize).SetDisplayFunc(x => x.Name)
					.FillItems(new List<Size>(), "Нет")
					.DynamicFillListFunc(x => ViewModel.GetSizeVariants(x))
					.AddSetter((c, n) => c.Editable = ViewModel.CanEdit && n.WearSizeType != null)
				.AddColumn("Рост").MinWidth(70)
					.AddComboRenderer(x => x.Height).SetDisplayFunc(x => x.Name)
					.FillItems(new List<Size>(), "Нет")
					.DynamicFillListFunc(x => ViewModel.GetHeightVariants(x))
					.AddSetter((c, n) => c.Editable = ViewModel.CanEdit && n.HeightType != null)
				.AddColumn("Собственники").Resizable()
					.Visible(ViewModel.OwnersVisible)
					.AddComboRenderer(x => x.Owner)
					.SetDisplayFunc(x => x.Name)
					.FillItems(ViewModel.Owners, "Нет")
					.Editing(ViewModel.CanEdit)
				.AddColumn("Процент износа")
					.AddNumericRenderer(e => e.WearPercent, new MultiplierToPercentConverter())
					.Editing(new Adjustment(0, 0, 999, 1, 10, 0), ViewModel.CanEdit).WidthChars(6).Digits(0)
					.AddTextRenderer(e => "%", expand: false)
				.AddColumn("Количество")
					.AddNumericRenderer(e => e.Amount)
					.Editing(new Adjustment(0, 0, 100000, 1, 10, 1), ViewModel.CanEdit).WidthChars(8)
					.AddReadOnlyTextRenderer(e => e.Units?.Name)
				.AddColumn("Стоимость")
					.AddNumericRenderer(e => e.Cost)
					.Editing(new Adjustment(0, 0, 100000000, 100, 1000, 0), ViewModel.CanEdit).Digits(2).WidthChars(12)
				.AddColumn("Сумма")
					.AddNumericRenderer(x => x.Total).Digits(2) 
				.Finish();
			
			ytreeItems.Selection.Changed += ytreeItems_Selection_Changed;
			ytreeItems.ItemsDataSource = ViewModel.Items;
			
		}

		protected void OnYbuttonReadInFileClicked(object sender, EventArgs e) =>
			ViewModel.ReadInFile();
		protected void OnYbuttonAddClicked(object sender, EventArgs e) =>
			ViewModel.AddItem();
		protected void OnYbuttonDelClicked(object sender, EventArgs e) =>
			ViewModel.DeleteItem(ytreeItems.GetSelectedObject<IncomeItem>());
		protected void OnYbuttonAddSizesClicked(object sender, EventArgs e) =>
			ViewModel.AddSize(ytreeItems.GetSelectedObject<IncomeItem>());
		private void ytreeItems_Selection_Changed(object sender, EventArgs e) =>
			ViewModel.SelectedItem = ytreeItems.GetSelectedObject<IncomeItem>();
	}
}
