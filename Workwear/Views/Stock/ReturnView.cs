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
			checkAuto.Binding.AddBinding(ViewModel, vm => vm.AutoDocNumber, w => w.Active).InitializeFromSource(); 
			ylabelCreatedBy.Binding
				.AddFuncBinding(ViewModel, vm => vm.DocCreatedbyUser != null ? vm.DocCreatedbyUser.Name : null, w => w.LabelProp)
				.InitializeFromSource ();
			ydateDoc.Binding
				.AddBinding(ViewModel, vm => vm.DocDate, w => w.Date)
				.InitializeFromSource ();
			ytextComment.Binding
				.AddBinding(ViewModel, vm => vm.DocComment, w => w.Buffer.Text)
				.InitializeFromSource();
			yentryEmployee.ViewModel = ViewModel.EmployeeCardEntryViewModel;
			entityWarehouseIncome.ViewModel = ViewModel.WarehouseEntryViewModel; 
			entityWarehouseIncome.Binding.
				AddBinding(ViewModel, vm => vm.WarehouseVisible, w => w.Visible)
				.InitializeFromSource();

			ybuttonAdd.Binding.AddBinding(ViewModel, vm => vm.CanAddItem, w => w.Sensitive).InitializeFromSource();
			ybuttonDel.Binding.AddBinding(ViewModel, vm => vm.CanRemoveItem, w => w.Sensitive).InitializeFromSource();
			ybuttonSetNomenclature.Binding.AddBinding(ViewModel, vm => vm.CanSetNomenclature, w => w.Sensitive).InitializeFromSource();
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
					.Editing()
				.AddColumn("Процент износа")
					.AddNumericRenderer(i => i.WearPercent, new MultiplierToPercentConverter())
					.Editing(new Adjustment(0, 0, 999, 1, 10, 0)).WidthChars(6).Digits(0)
					.AddTextRenderer(i => "%", expand: false)
				.AddColumn("Количество")
					.AddNumericRenderer(i => i.Amount)
					.Editing(new Adjustment(0, 0, 100000, 1, 10, 1)).WidthChars(2)
					.AddSetter((w, item) => w.Adjustment.Upper = item.MaxAmount)
					.AddReadOnlyTextRenderer(e => e.Units?.Name)
				.AddColumn("Отметка о износе")
					.AddTextRenderer(e => e.СommentReturn)
					.Editable()
				.Finish();
			//ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
			//ytreeItems.ButtonReleaseEvent += YtreeItemsButtonReleaseEvent;
			
			ytreeItems.ItemsDataSource = ViewModel.Items;
			//yAutoNumber

			//.Sensitive => ViewModel.CanEditItems;
		}

		protected void OnYbuttonAddClicked(object sender, EventArgs e) {
			ViewModel.AddFromEmployee();
		}

		protected void OnYbuttonDelClicked(object sender, EventArgs e) {
		}

		protected void OnYbuttonSetNomenclatureClicked(object sender, EventArgs e) {
		}

		protected void OnEnumPrintEnumItemClicked(object sender, QS.Widgets.EnumItemClickedEventArgs e) {
		}
	}
}
