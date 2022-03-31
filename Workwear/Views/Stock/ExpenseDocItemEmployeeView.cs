using System;
using System.Linq;
using System.Reflection;
using Gtk;
using QS.Dialog.GtkUI;
using QSWidgetLib;
using workwear.Domain.Stock;
using workwear.Measurements;
using workwear.ViewModels.Stock;
using Workwear.Measurements;

namespace workwear.Views.Stock
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
				.AddBinding(ViewModel, v => v.Sum, w => w.LabelProp)
				.InitializeFromSource();

			buttonAdd.Sensitive = ViewModel.Warehouse != null;

			ViewModel.expenseEmployeeViewModel.Entity.PropertyChanged += ExpenseDoc_PropertyChanged;

			ViewModel.PropertyChanged += PropertyChanged;
			ViewModel.CalculateTotal();
		}

		void CreateTable()
		{
			var cardIcon = new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "workwear.icon.buttons.smart-card.png");
			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ExpenseItem>()
				.AddColumn("Номенаклатуры нормы")
					.AddTextRenderer(node => node.ProtectionTools != null ? node.ProtectionTools.Name : "")
				.AddColumn("Номенклатура").AddComboRenderer(x => x.StockBalanceSetter)
				.SetDisplayFunc(x => x.Nomenclature?.Name)
					.SetDisplayListFunc(x => x.StockPosition.Title + " - " + x.Nomenclature.GetAmountAndUnitsText(x.Amount))
					.DynamicFillListFunc(x => x.EmployeeCardItem.BestChoiceInStock.ToList())
					.AddSetter((c, n) => c.Editable = n.EmployeeCardItem != null)
				.AddColumn("Размер")
					.AddComboRenderer(x => x.WearSize).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => SizeService.GetSize(viewModel.expenseEmployeeViewModel.UoW, x.Nomenclature?.Type?.SizeType))
					.AddSetter((c, n) => c.Editable = n.Nomenclature?.Type?.SizeType != null)
				.AddColumn("Рост")
					.AddComboRenderer(x => x.Height)
					.DynamicFillListFunc(x => SizeService.GetSize(viewModel.expenseEmployeeViewModel.UoW, x.Nomenclature?.Type?.HeightType))
					.AddSetter((c, n) => c.Editable = n.Nomenclature?.Type?.HeightType != null)
				.AddColumn("Процент износа").AddTextRenderer(e => (e.WearPercent).ToString("P0"))
				.AddColumn("Количество").AddNumericRenderer(e => e.Amount).Editing(new Adjustment(0, 0, 100000, 1, 10, 1))
					.AddTextRenderer(e => 
					e.Nomenclature != null && e.Nomenclature.Type != null && e.Nomenclature.Type.Units != null ? e.Nomenclature.Type.Units.Name : null)
				.AddColumn("Списание").AddToggleRenderer(e => e.IsWriteOff).Editing()
				.AddSetter((c, e) => c.Visible = e.IsEnableWriteOff)
				.AddColumn("Номер акта").AddTextRenderer(e => e.AktNumber).Editable().AddSetter((c, e) => c.Visible = e.IsWriteOff)
				.AddColumn("Бухгалтерский документ").AddTextRenderer(e => e.BuhDocument).Editable()
				.AddColumn("Отметка о выдаче").Visible(ViewModel.VisibleSignColumn)
						.AddPixbufRenderer(x => x.EmployeeIssueOperation == null || 
						                        String.IsNullOrEmpty(x.EmployeeIssueOperation.SignCardKey) ? null : cardIcon)
						.AddTextRenderer(x => x.EmployeeIssueOperation != null && 
						                      !String.IsNullOrEmpty(x.EmployeeIssueOperation.SignCardKey) ? 
					x.EmployeeIssueOperation.SignCardKey + " " + x.EmployeeIssueOperation.SignTimestamp.Value.ToString("dd.MM.yyyy HH:mm:ss") : null)
				.AddColumn("")
				.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = GetRowColor(n))
				.Finish();
		}

		#region PopupMenu
		void YtreeItems_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if (args.Event.Button != 3) return;
			var menu = new Menu();
			var selected = ytreeItems.GetSelectedObject<ExpenseItem>();

			var itemOpenProtection = new MenuItemId<ExpenseItem>("Открыть номеклатуру нормы");
			itemOpenProtection.ID = selected;
			itemOpenProtection.Sensitive = selected.ProtectionTools != null;
			itemOpenProtection.Activated += ItemOpenProtection_Activated;;
			menu.Add(itemOpenProtection);

			var itemNomenclature = new MenuItemId<ExpenseItem>("Открыть номеклатуру");
			itemNomenclature.ID = selected;
			itemNomenclature.Sensitive = selected.Nomenclature != null;
			itemNomenclature.Activated += Item_Activated;
			menu.Add(itemNomenclature);
			menu.ShowAll();
			menu.Popup();
		}

		void ItemOpenProtection_Activated(object sender, EventArgs e)
		{
			viewModel.OpenProtectionTools((sender as MenuItemId<ExpenseItem>).ID);
		}

		void Item_Activated(object sender, EventArgs e)
		{
			viewModel.OpenNomenclature((sender as MenuItemId<ExpenseItem>).ID);
		}
		#endregion

		#region private

		private string GetRowColor(ExpenseItem item)
		{
			var requiredIssue = item.EmployeeCardItem?.CalculateRequiredIssue(ViewModel.BaseParameters);
			if(item.ProtectionTools?.Type.IssueType == IssueType.Collective) 
				return item.Amount > 0 ? "#7B3F00" : "Burlywood";
			if(requiredIssue > 0 && item.Nomenclature == null)
				return item.Amount == 0 ? "red" : "Dark red";
			if(requiredIssue <= 0 && item.Amount == 0)
				return "gray";
			if(requiredIssue > item.Amount)
				return "blue";
			if(requiredIssue < item.Amount)
				return "Purple";
			if(item.EmployeeCardItem?.NextIssue > DateTime.Today)
				return "darkgreen";
			return null;
		}

		#endregion

		#region События
		void ExpenseDoc_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.Warehouse))
				buttonAdd.Sensitive = ViewModel.Warehouse != null;
		}

		void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			buttonFillBuhDoc.Sensitive = ViewModel.SensetiveFillBuhDoc;
			if(e.PropertyName == nameof(ViewModel.SelectedItem) && ViewModel.SelectedItem != null) {
				var iter = ytreeItems.YTreeModel.IterFromNode(ViewModel.SelectedItem);
				var path = ytreeItems.YTreeModel.GetPath(iter);
				ytreeItems.ScrollToCell(path, ytreeItems.Columns.First(), false, 0.5f, 0);
			}
		}
		#endregion

		#region Кнопки
		protected void OnButtonFillBuhDocClicked(object sender, EventArgs e)
		{
			ViewModel.FillBuhDoc();
		}

		protected void OnButtonDelClicked(object sender, EventArgs e)
		{
			viewModel.Delete(ytreeItems.GetSelectedObject<ExpenseItem>());
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
			viewModel.ShowAllSize(ytreeItems.GetSelectedObject<ExpenseItem>());
		}

		protected void OnButtonColorsLegendClicked(object sender, EventArgs e)
		{
			MessageDialogHelper.RunInfoDialog(
				"<span color='black'>●</span> — обычная выдача\n" +
				"<span color='darkgreen'>●</span> — выдача раньше срока\n" +
				"<span color='gray'>●</span> — выдача не требуется\n" +
				"<span color='blue'>●</span> — выдаваемого количества не достаточно\n" +
				"<span color='Purple'>●</span> — выдается больше необходимого\n" +
				"<span color='red'>●</span> — отсутсвует номеклатура\n" +
				"<span color='Dark red'>●</span> — указано количество без номенклатуры\n" +
				"<span color='Burlywood'>●</span> — позиция выдается коллективно\n" +
				"<span color='#7B3F00'>●</span> — выдача коллективной номеклатуры"
			);
		}
		#endregion
	}

}
