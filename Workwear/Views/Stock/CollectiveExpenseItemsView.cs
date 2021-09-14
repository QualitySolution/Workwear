using System;
using System.Linq;
using Gtk;
using QSWidgetLib;
using workwear.Domain.Stock;
using workwear.Measurements;
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
			ytreeItems.ItemsDataSource = ViewModel.ObservableItems;
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
			ytreeItems.ButtonReleaseEvent += YtreeItems_ButtonReleaseEvent;
			ytreeItems.Binding.AddBinding(viewModel, v => v.SelectedItem, w => w.SelectedRow);

			labelSum.Binding.AddBinding(ViewModel, v => v.Sum, w => w.LabelProp).InitializeFromSource();
			buttonAdd.Binding.AddBinding(ViewModel, v => v.SensetiveAddButton, w => w.Sensitive).InitializeFromSource();
		}

		void CreateTable()
		{
			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<CollectiveExpenseItem>()
				.AddColumn("Сотрудник").AddTextRenderer(x => x.Employee.ShortName)
				.AddColumn("Номенаклатуры нормы").AddTextRenderer(node => node.ProtectionTools != null ? node.ProtectionTools.Name : "")
				.AddColumn("Номенклатура").AddComboRenderer(x => x.StockBalanceSetter)
				.SetDisplayFunc(x => x.Nomenclature?.Name)
					.SetDisplayListFunc(x => x.StockPosition.Title + " - " + x.Nomenclature.GetAmountAndUnitsText(x.Amount))
					.DynamicFillListFunc(x => x.EmployeeCardItem.BestChoiceInStock.ToList())
					.AddSetter((c, n) => c.Editable = n.EmployeeCardItem != null)
				.AddColumn("Размер")
					.AddComboRenderer(x => x.Size)
					.DynamicFillListFunc(x => ViewModel.SizeService.GetSizesForNomeclature(x.Nomenclature.SizeStd))
					.AddSetter((c, n) => c.Editable = n.Nomenclature?.SizeStd != null && n.EmployeeCardItem == null)
				.AddColumn("Рост")
					.AddComboRenderer(x => x.WearGrowth)
					.FillItems(ViewModel.SizeService.GetGrowthForNomenclature())
					.AddSetter((c, n) => c.Editable = n.Nomenclature?.Type?.WearCategory != null &&  SizeHelper.HasGrowthStandart(n.Nomenclature.Type.WearCategory.Value))
				.AddColumn("Процент износа").AddTextRenderer(e => (e.WearPercent).ToString("P0"))
				.AddColumn("Количество").AddNumericRenderer(e => e.Amount).Editing(new Adjustment(0, 0, 100000, 1, 10, 1))
					.AddTextRenderer(e => e.Nomenclature != null && e.Nomenclature.Type != null && e.Nomenclature.Type.Units != null ? e.Nomenclature.Type.Units.Name : null)
				.AddColumn("")
				.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = GetRowColor(n))
				.Finish();
		}

		#region PopupMenu
		void YtreeItems_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3) {
				var menu = new Menu();
				var selected = ytreeItems.GetSelectedObject<CollectiveExpenseItem>();

				var itemOpenEmployee = new MenuItemId<CollectiveExpenseItem>("Открыть сотрудника");
				itemOpenEmployee.ID = selected;
				itemOpenEmployee.Sensitive = selected.Employee != null && selected != null;
				itemOpenEmployee.Activated += ItemOpenEmployee_Activated;
				menu.Add(itemOpenEmployee);

				var itemOpenPtotection = new MenuItemId<CollectiveExpenseItem>("Открыть номеклатуру нормы");
				itemOpenPtotection.ID = selected;
				itemOpenPtotection.Sensitive = selected.ProtectionTools != null && selected != null;
				itemOpenPtotection.Activated += ItemOpenPtotection_Activated;
				menu.Add(itemOpenPtotection);

				var itemOpenNomenclature = new MenuItemId<CollectiveExpenseItem>("Открыть номеклатуру");
				itemOpenNomenclature.ID = selected;
				itemOpenNomenclature.Sensitive = selected.Nomenclature != null && selected != null;
				itemOpenNomenclature.Activated += Item_Activated;
				menu.Add(itemOpenNomenclature);

				menu.ShowAll();
				menu.Popup();
			}
		}

		void ItemOpenEmployee_Activated(object sender, EventArgs e)
		{
			viewModel.OpenEmployee((sender as MenuItemId<CollectiveExpenseItem>).ID);
		}	

		void Item_Activated(object sender, EventArgs e)
		{
			viewModel.OpenNomenclature((sender as MenuItemId<CollectiveExpenseItem>).ID);
		}

		void ItemOpenPtotection_Activated(object sender, EventArgs e)
		{
			viewModel.OpenProtectionTools((sender as MenuItemId<CollectiveExpenseItem>).ID);
		}
		#endregion

		#region private

		private string GetRowColor(CollectiveExpenseItem item)
		{
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
		protected void OnButtonDelClicked(object sender, EventArgs e)
		{
			viewModel.Delete(ytreeItems.GetSelectedObject<CollectiveExpenseItem>());
		}

		void YtreeItems_Selection_Changed(object sender, EventArgs e)
		{
			buttonDel.Sensitive = buttonShowAllSize.Sensitive = ytreeItems.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			ViewModel.AddItem();
		}

		protected void OnButtonShowAllSizeClicked(object sender, EventArgs e)
		{
			viewModel.ShowAllSize(ytreeItems.GetSelectedObject<CollectiveExpenseItem>());
		}
		#endregion
	}

}
