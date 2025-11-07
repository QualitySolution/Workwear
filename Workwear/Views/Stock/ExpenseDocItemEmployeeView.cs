using System;
using System.Linq;
using System.Reflection;
using Gtk;
using QS.Dialog.GtkUI;
using QSWidgetLib;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock.Documents;
using Workwear.Tools.Features;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ExpenseDocItemEmployeeView : Gtk.Bin
	{
		public ExpenseDocItemEmployeeView()
		{
			this.Build();
		}

		private ExpenseDocItemsEmployeeViewModel viewModel;

		public ExpenseDocItemsEmployeeViewModel ViewModel {
			get => viewModel;
			set {
				viewModel = value;
				Configure();
			}
		}

		public void Configure()
		{
			CreateTable();
			ytreeItems.ItemsDataSource = ViewModel.ObservableItems;
			
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
			ytreeItems.ButtonReleaseEvent += YtreeItems_ButtonReleaseEvent;
			ytreeItems.Binding.AddBinding(viewModel, v => v.SelectedItem, w => w.SelectedRow);

			labelSum.Binding
				.AddBinding(ViewModel, v => v.Sum, w => w.LabelProp).InitializeFromSource();
			buttonAdd.Binding
				.AddBinding(ViewModel, vm => vm.CanAddItems, w => w.Sensitive).InitializeFromSource();
			buttonCreateOrRenewBarcodes.Binding.AddSource(ViewModel)
				.AddBinding(v => v.CanCreateBarcode, w => w.Visible)
				.AddBinding(v => v.NeedCreateBarcodes, w => w.Sensitive)
				.AddBinding(v => v.ButtonCreateOrRenewBarcodesTitle, w => w.Label).InitializeFromSource();
			buttonPrintBarcodes.Binding.AddSource(ViewModel)
				.AddBinding(v => v.CanPrintBarcode, w => w.Visible)
				.AddBinding(v => v.SensitiveBarcodesPrint, w => w.Sensitive).InitializeFromSource();
			buttonSetBarcodes.Binding.AddSource(ViewModel)
                .AddBinding(v => v.CanSetBarcode, w => w.Visible)
                .AddBinding(v => v.CanAddBarcodeForSelected, w => w.Sensitive).InitializeFromSource();

			ViewModel.PropertyChanged += PropertyChanged;
			ViewModel.CalculateTotal();
		}

		void CreateTable()
		{
			var cardIcon = new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "Workwear.icon.buttons.smart-card.png");
			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ExpenseItem>()
				.AddColumn("Номенклатура нормы").Resizable().AddTextRenderer(node => node.ProtectionTools != null ? node.ProtectionTools.Name : "")
					.WrapWidth(700)
				.AddColumn("Номенклатура").Resizable()
					.AddComboRenderer(x => x.StockBalanceSetter).WrapWidth(700)
					.SetDisplayFunc(x => x.Position.Nomenclature?.Name)
					.SetDisplayListFunc(x => x.Position.Title + " - " + x.Position.Nomenclature.GetAmountAndUnitsText(x.Amount))
					.DynamicFillListFunc(x => x.EmployeeCardItem.BestChoiceInStock.ToList())
					.AddSetter((c, n) => c.Editable = n.EmployeeCardItem != null)
				.AddColumn("Размер")
					.AddComboRenderer(x => x.WearSize).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => ViewModel.SizeService.GetSize(viewModel.expenseEmployeeViewModel.UoW, x.Nomenclature?.Type?.SizeType, onlyUseInNomenclature:true).ToList())
					.AddSetter((c, n) => c.Editable = ViewModel.CanEdit && n.Nomenclature?.Type?.SizeType != null)
				.AddColumn("Рост")
					.AddComboRenderer(x => x.Height).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => ViewModel.SizeService.GetSize(viewModel.expenseEmployeeViewModel.UoW, x.Nomenclature?.Type?.HeightType, onlyUseInNomenclature:true).ToList())
					.AddSetter((c, n) => c.Editable = ViewModel.CanEdit && n.Nomenclature?.Type?.HeightType != null)
				.AddColumn("Собственники")
					.Visible(ViewModel.featuresService.Available(WorkwearFeature.Owners))
					.AddComboRenderer(x => x.Owner)
					.SetDisplayFunc(x => x.Name)
					.FillItems(ViewModel.Owners, "Нет")
					.Editing(ViewModel.CanEdit)	
				.AddColumn("Износ").AddTextRenderer(e => (e.WearPercent).ToString("P0"))
				.AddColumn("Количество").AddNumericRenderer(e => e.Amount).Editing(new Adjustment(0, 0, 100000, 1, 10, 1), ViewModel.CanEdit)
					.AddTextRenderer(e => 
					e.Nomenclature != null && e.Nomenclature.Type != null && e.Nomenclature.Type.Units != null ? e.Nomenclature.Type.Units.Name : null)
				.AddColumn("Маркировка").Visible(ViewModel.VisibleBarcodes)
					.AddTextRenderer(x => x.BarcodeTextFunc())
					.AddSetter((c,n) => c.Foreground = ViewModel.BarcodesTextColor(n))
				.AddColumn("Отметка о выдаче").Visible(ViewModel.VisibleSignColumn)
						.AddPixbufRenderer(x => x.EmployeeIssueOperation == null || 
						                        String.IsNullOrEmpty(x.EmployeeIssueOperation.SignCardKey) ? null : cardIcon)
						.AddTextRenderer(x => x.EmployeeIssueOperation != null && 
						                      !String.IsNullOrEmpty(x.EmployeeIssueOperation.SignCardKey) ? 
					x.EmployeeIssueOperation.SignCardKey + " " + x.EmployeeIssueOperation.SignTimestamp.Value.ToString("dd.MM.yyyy HH:mm:ss") : null)
				.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = ViewModel.GetRowColor(n))
				.Finish();
		}

		#region PopupMenu
		void YtreeItems_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if (args.Event.Button != 3) return;
			var menu = new Menu();
			var selected = ytreeItems.GetSelectedObject<ExpenseItem>();

			var itemOpenProtection = new MenuItemId<ExpenseItem>("Открыть номенклатуру нормы");
			itemOpenProtection.ID = selected;
			itemOpenProtection.Sensitive = selected?.ProtectionTools != null;
			itemOpenProtection.Activated += ItemOpenProtection_Activated;;
			menu.Add(itemOpenProtection);

			var itemNomenclature = new MenuItemId<ExpenseItem>("Открыть номенклатуру");
			itemNomenclature.ID = selected;
			itemNomenclature.Sensitive = selected?.Nomenclature != null;
			itemNomenclature.Activated += Item_Activated;
			menu.Add(itemNomenclature);
			
			var itemChangeProtectionTools = new MenuItem("Изменить номенклатуру нормы");
			var subItemChangeProtectionTools = new Menu();
				
				var itemMakeEmptyProtectionTools = new MenuItemId<ExpenseItem>("Очистить");
				itemMakeEmptyProtectionTools.ID = selected;
				itemMakeEmptyProtectionTools.ButtonPressEvent += (sender, e) => ViewModel.MakeEmptyProtectionTools(((MenuItemId<ExpenseItem>)sender).ID);
				subItemChangeProtectionTools.Append(itemMakeEmptyProtectionTools);
					
				var menuItemChangePT = new MenuItem("Из списка потребностей");
				var submenuChangePT = new Menu();
				foreach(ProtectionTools protectionTools in ViewModel.EmployeesProtectionToolsList) {
					var ptItem = new MenuItem(protectionTools.Name);
					ptItem.ButtonPressEvent += (sender, e) => ViewModel.ChangeProtectionTools(selected,protectionTools);
					submenuChangePT.Append(ptItem);
				}
				menuItemChangePT.Submenu = submenuChangePT;
				subItemChangeProtectionTools.Append(menuItemChangePT);
					
				var menuItemChangePtFull = new MenuItemId<ExpenseItem>("Из полного списка ...");
				menuItemChangePtFull.ID = selected;
				menuItemChangePtFull.ButtonPressEvent += (sender, e) => ViewModel.OpenJournalChangeProtectionTools(((MenuItemId<ExpenseItem>)sender).ID);
				
				subItemChangeProtectionTools.Append(menuItemChangePtFull);
					
			itemChangeProtectionTools.Submenu = subItemChangeProtectionTools;
			itemChangeProtectionTools.Sensitive = ViewModel.CanEdit;
			menu.Add(itemChangeProtectionTools);
			
			if(ViewModel.featuresService.Available(WorkwearFeature.Barcodes) && selected.Nomenclature.UseBarcode) {
				var itemRemoveBarcode = new MenuItem("Отвязать метку");
				var subItemRemoveBarcode = new Menu();

				foreach(BarcodeOperation bOpeation in selected.EmployeeIssueOperation.BarcodeOperations) {
					var opItem = new MenuItem(bOpeation.Barcode.Title);
					opItem.ButtonPressEvent += (sender, e) => ViewModel.RemoveBarcodeOperation(selected, bOpeation);
					subItemRemoveBarcode.Append(opItem);
				}

				itemRemoveBarcode.Submenu = subItemRemoveBarcode;
				menu.Add(itemRemoveBarcode);
			}

			menu.ShowAll();
			menu.Popup();
		}

		void ItemOpenProtection_Activated(object sender, EventArgs e) =>
			viewModel.OpenProtectionTools((sender as MenuItemId<ExpenseItem>).ID);
		void Item_Activated(object sender, EventArgs e) =>
			viewModel.OpenNomenclature((sender as MenuItemId<ExpenseItem>).ID);
		
		#endregion
		
		#region События
		void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.SelectedItem) && ViewModel.SelectedItem != null) {
				var iter = ytreeItems.YTreeModel.IterFromNode(ViewModel.SelectedItem);
				var path = ytreeItems.YTreeModel.GetPath(iter);
				ytreeItems.ScrollToCell(path, ytreeItems.Columns.First(), false, 0.5f, 0);
			}
		}
		#endregion

		#region Кнопки
		protected void OnButtonDelClicked(object sender, EventArgs e) =>
			viewModel.Delete(ytreeItems.GetSelectedObject<ExpenseItem>());

		void YtreeItems_Selection_Changed(object sender, EventArgs e) =>
			buttonDel.Sensitive = buttonShowAllSize.Sensitive = ViewModel.CanEdit && ytreeItems.Selection.CountSelectedRows() > 0;

		protected void OnButtonAddClicked(object sender, EventArgs e) =>
			ViewModel.AddItem();

		protected void OnButtonShowAllSizeClicked(object sender, EventArgs e) =>
			viewModel.ShowAllSize(ytreeItems.GetSelectedObject<ExpenseItem>());

		protected void OnButtonColorsLegendClicked(object sender, EventArgs e)
		{
			MessageDialogHelper.RunInfoDialog(
				"<span color='black'>●</span> — обычная выдача\n" +
				"<span color='darkgreen'>●</span> — выдача раньше срока\n" +
				"<span color='gray'>●</span> — выдача не требуется\n" +
				"<span color='blue'>●</span> — выдаваемого количества не достаточно\n" +
				"<span color='Purple'>●</span> — выдается больше необходимого\n" +
				"<span color='red'>●</span> — отсутствует номенклатура\n" +
				"<span color='Dark red'>●</span> — указано количество без номенклатуры\n" +
				"<span color='Burlywood'>●</span> — позиция выдается коллективно\n" +
				"<span color='#7B3F00'>●</span> — выдача коллективной номенклатуры"
			);
		}

		protected void OnButtonCreateOrDeleteBarcodesClicked(object sender, EventArgs e) {
			ViewModel.ReleaseBarcodes();
			ytreeItems.YTreeModel.EmitModelChanged();
		}

		protected void OnButtonPrintBarcodesClicked(object sender, EventArgs e) {
			ViewModel.PrintBarcodesEAN13();
			ytreeItems.YTreeModel.EmitModelChanged();
		}

		protected void OnButtonSetBarcodesClicked(object sender, EventArgs e) {
			ViewModel.AddBarcodeFromScan(ytreeItems.GetSelectedObject<ExpenseItem>());
		}
		#endregion
	}

}
