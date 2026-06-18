using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.Views.Dialog;
using Workwear.Domain.Stock.Documents;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock {
	public partial class BarcodingView : EntityDialogViewBase<BarcodingViewModel, Barcoding> {
		public BarcodingView(BarcodingViewModel viewModel) : base(viewModel) {
			Build();
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
			
			ylabelUser.Binding
				.AddFuncBinding(Entity, e => e.CreatedbyUser != null ? e.CreatedbyUser.Name : null, w => w.LabelProp).InitializeFromSource();
			ydateDoc.Binding
				.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();
			ytextComment.Binding
				.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			labelSum.Binding
				.AddBinding(ViewModel, vm => vm.Total, w => w.LabelProp).InitializeFromSource();
			entityWarehouse.ViewModel = ViewModel.EntryWarehouseViewModel;
			entityWarehouse.Binding.AddBinding(ViewModel, vm => vm.SensitiveWarehouse, w => w.Sensitive).InitializeFromSource();
			
			ytreeItems.Selection.Changed += Items_Selection_Changed;
			ybuttonAdd.Clicked += OnButtonAddClicked;
			buttonPrintAll.Clicked += OnButtonPrintAllClicked;
			buttonPrintSelect.Clicked += OnButtonPrintSelectClicked;
		}

		private void ConfigureItems()
		{
			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<BarcodingItem> ()
				.AddColumn ("Наименование").Resizable()
					.ToolTipText(x => $"ИД номенклатуры: {x.Nomenclature.Id}")
					.AddTextRenderer (e => e.Nomenclature.Name).WrapWidth(500)
				.AddColumn ("Размер")
					.AddTextRenderer (e => e.SizeName)
				.AddColumn ("Рост")
					.AddTextRenderer (e => e.HeightName)
				.AddColumn("Количество")
					.AddTextRenderer (e => e.Amount.ToString())
				.AddColumn("Собственник").Resizable()
					.Visible(ViewModel.OwnersVisible)
					.AddTextRenderer(e => e.OwnerName)
				.AddColumn("Износ")
					.AddTextRenderer(e => e.WearPercent.ToString("P0"))
				.AddColumn ("Название").Resizable()
					.AddTextRenderer (e => e.Barcodes.Select(b => b.Label)
					.Aggregate((a, b) => (a+'\n'+b)))
				.AddColumn ("Штрихкоды")
					.AddTextRenderer (e => e.Barcodes.Select(b => b.Title)
						.Aggregate((a, b) => (a+'\n'+b)))
				.Finish();
			ytreeItems.Binding
				.AddBinding(Entity, vm => vm.Items, w => w.ItemsDataSource)
				.InitializeFromSource();
		}

		void MakeMenu() {
			var delMenu = new Menu();
			var item = new yMenuItem("Все");
			item.Activated += (sender, e) => ViewModel.DeleteFromItem(ytreeItems.GetSelectedObject<BarcodingItem>());
			delMenu.Add(item);
			if(ytreeItems.GetSelectedObject<BarcodingItem>()?.Barcodes?.Any() ?? false)
				foreach(var barcode in ytreeItems.GetSelectedObject<BarcodingItem>().Barcodes) {
					item = new yMenuItem(barcode.Title);
					item.Activated += (sender, e) => ViewModel.DeleteFromItem(ytreeItems.GetSelectedObject<BarcodingItem>(), barcode);
					delMenu.Add(item);
				}

			buttonDel.Menu = delMenu;
			delMenu.ShowAll();
		}

		private void OnButtonAddClicked(object sender, EventArgs e) => ViewModel.AddItems();
		private void OnButtonPrintSelectClicked(object sender, EventArgs e) => 
			ViewModel.PrintBarcodesforItems(new List<BarcodingItem>() {ytreeItems.GetSelectedObject<BarcodingItem>()});
		private void OnButtonPrintAllClicked(object sender, EventArgs e) => ViewModel.PrintBarcodesforItems();
		private void Items_Selection_Changed(object sender, EventArgs e){
			buttonDel.Sensitive = buttonPrintSelect.Sensitive = ytreeItems.Selection.CountSelectedRows() > 0;
			MakeMenu();
		}
	}
}

