using System;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QSWidgetLib;
using Workwear.Domain.Stock.Documents;
using Workwear.Tools.Features;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CollectiveExpenseItemsView : Gtk.Bin
	{
		public CollectiveExpenseItemsView()
		{
			this.Build();
		}

		private CollectiveExpenseItemsViewModel viewModel;

		public CollectiveExpenseItemsViewModel ViewModel {
			get => viewModel;
			set {
				viewModel = value;
				Configure();
			}
		}

		public void Configure()
		{
			CreateTable();
			ytreeItems.ButtonReleaseEvent += YtreeItems_ButtonReleaseEvent;
			ytreeItems.Binding
				.AddSource(ViewModel)
					.AddBinding(v => v.SelectedItem, w => w.SelectedRow)
				.AddSource(ViewModel.Entity)
					.AddBinding(e => e.ObservableItems, w => w.ItemsDataSource)
				.InitializeFromSource();
			buttonAdd.Binding.AddBinding(viewModel,v =>v.SensitiveAddButton,w=>w.Sensitive).InitializeFromSource();
			buttonDel.Binding.AddBinding(viewModel,v =>v.SensitiveButtonDel,w=>w.Sensitive).InitializeFromSource();
			buttonChange.Binding.AddBinding(viewModel,v =>v.SensitiveButtonChange,w=>w.Sensitive).InitializeFromSource();
			labelSum.Binding.AddBinding(ViewModel, v => v.Sum, w => w.LabelProp).InitializeFromSource();
			ViewModel.PropertyChanged += ViewModel_PropertyChanged;
			MakeMenu();
		}

		void CreateTable()
		{
			ytreeItems.ColumnsConfig = ColumnsConfigFactory.Create<CollectiveExpenseItem>()
				.AddColumn("Сотрудник").AddTextRenderer(x => x.Employee.ShortName)
				.AddColumn("Номенклатура нормы").AddTextRenderer(node => node.ProtectionTools != null ? node.ProtectionTools.Name : "")
					.WrapWidth(700)
				.AddColumn("Номенклатура").AddComboRenderer(x => x.StockBalanceSetter)
					.WrapWidth(700)
					.SetDisplayFunc(x => x.Nomenclature?.Name)
					.SetDisplayListFunc(x => x.StockPosition.Title + " - " + x.Nomenclature.GetAmountAndUnitsText(x.Amount))
					.DynamicFillListFunc(x => x.EmployeeCardItem.BestChoiceInStock.ToList())
					.AddSetter((c, n) => c.Editable = n.EmployeeCardItem != null)
				.AddColumn("Размер")
					.AddComboRenderer(x => x.WearSize).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => 
					ViewModel.SizeService.GetSize(viewModel.сollectiveExpenseViewModel.UoW, x.Nomenclature?.Type?.SizeType, onlyUseInNomenclature:true).ToList())
					.AddSetter((c, n) => c.Editable = n.Nomenclature?.Type?.SizeType == null)
				.AddColumn("Рост")
					.AddComboRenderer(x => x.Height).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => 
					ViewModel.SizeService.GetSize(viewModel.сollectiveExpenseViewModel.UoW, x.Nomenclature?.Type?.HeightType, onlyUseInNomenclature:true).ToList())
					.AddSetter((c, n) => c.Editable = n.Nomenclature?.Type?.HeightType != null)
				.AddColumn("Собственники")
					.Visible(ViewModel.featuresService.Available(WorkwearFeature.Owners))
					.AddComboRenderer(x => x.Owner)
					.SetDisplayFunc(x => x.Name)
					.FillItems(ViewModel.Owners, "Нет")
					.Editing()
				.AddColumn("Процент износа").AddTextRenderer(e => (e.WearPercent).ToString("P0"))
				.AddColumn("Количество").AddNumericRenderer(e => e.Amount).Editing(new Adjustment(0, 0, 100000, 1, 10, 1))
					.AddTextRenderer(e => e.Nomenclature != null && e.Nomenclature.Type != null && 
					                      e.Nomenclature.Type.Units != null ? e.Nomenclature.Type.Units.Name : null)
				.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = ViewModel.GetRowColor(n))
				.Finish();
		}

		void MakeMenu() {
			var delMenu = new Menu();
			var item = new yMenuItem("Удалить строку");
			item.Activated += (sender, e) => ViewModel.Delete(ytreeItems.GetSelectedObject<CollectiveExpenseItem>());
			delMenu.Add(item);
			item = new yMenuItem("Удалить все строки сотрудника");
			item.Activated += (sender, e) => ViewModel.DeleteEmployee(ytreeItems.GetSelectedObject<CollectiveExpenseItem>());
			delMenu.Add(item);
			buttonDel.Menu = delMenu;
			delMenu.ShowAll();

			var addMenu = new Menu();
			item = new yMenuItem("Сотрудников");
			item.Activated += (sender, e) => ViewModel.AddEmployees();
			addMenu.Add(item);
			item = new yMenuItem("Подразделения");
			item.Activated += (sender, e) => ViewModel.AddSubdivisions();
			addMenu.Add(item);
			addMenu.Add(new SeparatorMenuItem());
			item = new yMenuItem("Дополнительно выбранному сотруднику");
			item.Activated += (sender, e) => ViewModel.Refresh(ytreeItems.GetSelectedObjects<CollectiveExpenseItem>());
			item.Binding.AddBinding(ViewModel, v => v.SensitiveRefreshMenuItem, w => w.Sensitive).InitializeFromSource();
			addMenu.Add(item);
			item = new yMenuItem("Дополнительно всем");
			item.Activated += (sender, e) => ViewModel.RefreshAll();
			item.Binding.AddBinding(ViewModel, v => v.SensitiveRefreshAllMenuItem, w => w.Sensitive).InitializeFromSource();
			addMenu.Add(item);

			buttonAdd.Menu = addMenu;
			addMenu.ShowAll();
			
//todo Выводить сообщение если не хватило всем
			var changeMenu = new Menu();
			item = new yMenuItem("В выделенной строке");
			item.Activated += (sender, e) => ViewModel.ChangeStockPosition(ytreeItems.GetSelectedObject<CollectiveExpenseItem>());
			changeMenu.Add(item);
			item = new yMenuItem("Аналгичные в документе");
			item.Activated += (sender, e) => ViewModel.ChangeManyStockPositions(ytreeItems.GetSelectedObject<CollectiveExpenseItem>());
			changeMenu.Add(item);
			buttonChange.Menu = changeMenu;
			changeMenu.ShowAll();
		}

		#region PopupMenu
		void YtreeItems_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args) {
			if (args.Event.Button != 3) return;
			var menu = new Menu();
			var selected = ytreeItems.GetSelectedObject<CollectiveExpenseItem>();

			var itemOpenEmployee = new MenuItemId<CollectiveExpenseItem>("Открыть сотрудника");
			itemOpenEmployee.ID = selected;
			itemOpenEmployee.Sensitive = selected.Employee != null;
			itemOpenEmployee.Activated += ItemOpenEmployee_Activated;
			menu.Add(itemOpenEmployee);

			var itemOpenProtection = new MenuItemId<CollectiveExpenseItem>("Открыть номенклатуру нормы");
			itemOpenProtection.ID = selected;
			itemOpenProtection.Sensitive = selected.ProtectionTools != null;
			itemOpenProtection.Activated += ItemOpenProtection_Activated;
			menu.Add(itemOpenProtection);

			var itemOpenNomenclature = new MenuItemId<CollectiveExpenseItem>("Открыть номенклатуру");
			itemOpenNomenclature.ID = selected;
			itemOpenNomenclature.Sensitive = selected.Nomenclature != null;
			itemOpenNomenclature.Activated += Item_Activated;
			menu.Add(itemOpenNomenclature);
			
			menu.ShowAll();
			menu.Popup();
		}

		void ItemOpenEmployee_Activated(object sender, EventArgs e)
		{
			viewModel.OpenEmployee(((MenuItemId<CollectiveExpenseItem>) sender).ID);
		}	

		void Item_Activated(object sender, EventArgs e)
		{
			viewModel.OpenNomenclature(((MenuItemId<CollectiveExpenseItem>) sender).ID);
		}

		void ItemOpenProtection_Activated(object sender, EventArgs e)
		{
			viewModel.OpenProtectionTools(((MenuItemId<CollectiveExpenseItem>) sender).ID);
		}

		#endregion

		#region Кнопки

		private void OnButtonDelClicked(object sender, EventArgs e)
		{
			viewModel.Delete(ytreeItems.GetSelectedObject<CollectiveExpenseItem>());
		}
		#endregion

		#region События
		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof(ViewModel.SelectedItem) || ViewModel.SelectedItem == null) return;
			var iter = ytreeItems.YTreeModel.IterFromNode(ViewModel.SelectedItem);
			var path = ytreeItems.YTreeModel.GetPath(iter);
			ytreeItems.ScrollToCell(path, ytreeItems.Columns.First(), false, 0.5f, 0);
		}
		#endregion
	}

}
