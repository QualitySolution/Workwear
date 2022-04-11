using System;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QSWidgetLib;
using workwear.Domain.Stock;
using Workwear.Measurements;
using workwear.ViewModels.Stock;

namespace workwear.Views.Stock
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
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
			ytreeItems.ButtonReleaseEvent += YtreeItems_ButtonReleaseEvent;
			ytreeItems.Binding
				.AddSource(ViewModel)
					.AddBinding(v => v.SelectedItem, w => w.SelectedRow)
				.AddSource(ViewModel.Entity)
					.AddBinding(e => e.ObservableItems, w => w.ItemsDataSource)
				.InitializeFromSource();

			labelSum.Binding.AddBinding(ViewModel, v => v.Sum, w => w.LabelProp).InitializeFromSource();
			buttonAdd.Binding.AddBinding(ViewModel, v => v.SensetiveAddButton, w => w.Sensitive).InitializeFromSource();

			ViewModel.PropertyChanged += ViewModel_PropertyChanged;
			MakeMenu();
		}

		void CreateTable()
		{
			ytreeItems.ColumnsConfig = ColumnsConfigFactory.Create<CollectiveExpenseItem>()
				.AddColumn("Сотрудник").AddTextRenderer(x => x.Employee.ShortName)
				.AddColumn("Номенаклатуры нормы").AddTextRenderer(node => node.ProtectionTools != null ? node.ProtectionTools.Name : "")
				.AddColumn("Номенклатура").AddComboRenderer(x => x.StockBalanceSetter)
					.SetDisplayFunc(x => x.Nomenclature?.Name)
					.SetDisplayListFunc(x => x.StockPosition.Title + " - " + x.Nomenclature.GetAmountAndUnitsText(x.Amount))
					.DynamicFillListFunc(x => x.EmployeeCardItem.BestChoiceInStock.ToList())
					.AddSetter((c, n) => c.Editable = n.EmployeeCardItem != null)
				.AddColumn("Размер")
					.AddComboRenderer(x => x.WearSize).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => 
						SizeService.GetSize(viewModel.сollectiveExpenseViewModel.UoW, x.Nomenclature?.Type?.SizeType, true).ToList())
					.AddSetter((c, n) => c.Editable = n.Nomenclature?.Type?.SizeType == null)
				.AddColumn("Рост")
					.AddComboRenderer(x => x.Height).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => 
						SizeService.GetSize(viewModel.сollectiveExpenseViewModel.UoW, x.Nomenclature?.Type?.HeightType, true).ToList())
					.AddSetter((c, n) => c.Editable = n.Nomenclature?.Type?.HeightType != null)
				.AddColumn("Процент износа").AddTextRenderer(e => (e.WearPercent).ToString("P0"))
				.AddColumn("Количество").AddNumericRenderer(e => e.Amount).Editing(new Adjustment(0, 0, 100000, 1, 10, 1))
					.AddTextRenderer(e => e.Nomenclature != null && e.Nomenclature.Type != null && 
					                      e.Nomenclature.Type.Units != null ? e.Nomenclature.Type.Units.Name : null)
				.AddColumn("")
				.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = GetRowColor(n))
				.Finish();
		}

		void MakeMenu() {
			var menu = new Menu();
			var item = new MenuItem("Удалить строку");
			item.Activated += (sender, e) => ViewModel.Delete(ytreeItems.GetSelectedObject<CollectiveExpenseItem>());
			menu.Add(item);
			item = new MenuItem("Удалить все строки сотрудника");
			item.Activated += (sender, e) => ViewModel.DeleteEmployee(ytreeItems.GetSelectedObject<CollectiveExpenseItem>());
			menu.Add(item);
			buttonDel.Menu = menu;
			menu.ShowAll();
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

			var itemOpenProtection = new MenuItemId<CollectiveExpenseItem>("Открыть номеклатуру нормы");
			itemOpenProtection.ID = selected;
			itemOpenProtection.Sensitive = selected.ProtectionTools != null;
			itemOpenProtection.Activated += ItemOpenProtection_Activated;
			menu.Add(itemOpenProtection);

			var itemOpenNomenclature = new MenuItemId<CollectiveExpenseItem>("Открыть номеклатуру");
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

		#region private

		private string GetRowColor(CollectiveExpenseItem item) {
			var requiredIssue = item.EmployeeCardItem?.CalculateRequiredIssue(ViewModel.BaseParameters);
			if(requiredIssue > 0 && item.Nomenclature == null)
				return item.Amount == 0 ? "red" : "Dark red";
			if(requiredIssue > 0 && item.Amount == 0)
				return "blue";
			if(requiredIssue <= 0 && item.Amount == 0)
				return "gray";
			return null;
		}

		#endregion

		#region Кнопки

		private void OnButtonDelClicked(object sender, EventArgs e)
		{
			viewModel.Delete(ytreeItems.GetSelectedObject<CollectiveExpenseItem>());
		}

		void YtreeItems_Selection_Changed(object sender, EventArgs e)
		{
			buttonDel.Sensitive = buttonShowAllSize.Sensitive = buttonRefreshEmployee.Sensitive = ytreeItems.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			ViewModel.AddItem();
		}

		protected void OnButtonShowAllSizeClicked(object sender, EventArgs e)
		{
			viewModel.ShowAllSize(ytreeItems.GetSelectedObject<CollectiveExpenseItem>());
		}

		protected void OnButtonRefreshEmployeesClicked(object sender, EventArgs e)
		{
			ViewModel.RefreshAll();
		}

		protected void OnButtonRefreshEmployeeClicked(object sender, EventArgs e)
		{
			ViewModel.RefreshItem(ytreeItems.GetSelectedObject<CollectiveExpenseItem>());
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
