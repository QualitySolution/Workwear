using System;
using System.Linq;
using Gtk;
using NLog;
using QS.Dialog.Gtk;
using QS.Project.Domain;
using QSOrmProject;
using QSWidgetLib;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using workwear.Measurements;
using workwear.Representations.Organization;
using workwear.ViewModels.Stock;

namespace workwear
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WriteOffItemsView : WidgetOnDialogBase
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private enum ColumnTags
		{
			BuhDoc
		}

		private Writeoff writeoffDoc;

		public Writeoff WriteoffDoc {
			get {return writeoffDoc;}
			set {
				if (writeoffDoc == value)
					return;
				writeoffDoc = value;
				ytreeItems.ItemsDataSource = writeoffDoc.ObservableItems;
				writeoffDoc.ObservableItems.ListContentChanged += WriteoffDoc_ObservableItems_ListContentChanged;
				CalculateTotal();

				WriteoffDoc.Items.ToList().ForEach(item => item.PropertyChanged += Item_PropertyChanged);
			}
		}

		void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(WriteoffItem.BuhDocument)) {
				(MyEntityDialog as EntityDialogBase<Writeoff>).HasChanges = true;
			}
		}

		void WriteoffDoc_ObservableItems_ListContentChanged (object sender, EventArgs e)
		{
			CalculateTotal();
		}

		public EmployeeCard CurWorker { get; set;}

		public Subdivision CurObject { get; set;}

		public Warehouse CurWarehouse { get; set; }

		public WriteOffItemsView()
		{
			this.Build();

			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<WriteoffItem> ()
				.AddColumn ("Наименование").AddTextRenderer (e => e.Nomenclature.Name)
				.AddColumn("Размер").MinWidth(60)
					.AddComboRenderer(x => x.Size)
					.DynamicFillListFunc(x => SizeHelper.GetSizesListByStdCode(x.Nomenclature.SizeStd, SizeUse.HumanOnly))
					.AddSetter((c, n) => c.Editable = n.Nomenclature.SizeStd != null)
				.AddColumn("Рост").MinWidth(70)
					.AddComboRenderer(x => x.WearGrowth)
					.DynamicFillListFunc(x => SizeHelper.GetSizesListByStdCode(x.Nomenclature.WearGrowthStd, SizeUse.HumanOnly))
					.AddSetter((c, n) => c.Editable = n.Nomenclature.WearGrowthStd != null)
				.AddColumn ("Процент износа").AddTextRenderer (e => e.WearPercent.ToString("P0"))
				.AddColumn ("Списано из").AddTextRenderer (e => e.LastOwnText)
				.AddColumn ("Количество").AddNumericRenderer (e => e.Amount).Editing (new Adjustment(0, 0, 100000, 1, 10, 1)).WidthChars(7)
				.AddTextRenderer (e => e.Nomenclature.Type.Units.Name)
				.AddColumn("Бухгалтерский документ").Tag(ColumnTags.BuhDoc).AddTextRenderer(e => e.BuhDocument)
				.AddSetter((c, e) => c.Editable = e.WriteoffFrom == WriteoffFrom.Employye)
				.Finish ();
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
			ytreeItems.ButtonReleaseEvent += YtreeItems_ButtonReleaseEvent;
		}

		#region PopupMenu
		void YtreeItems_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3) {
				var menu = new Menu();
				var selected = ytreeItems.GetSelectedObject<WriteoffItem>();
				var item = new MenuItemId<WriteoffItem>("Открыть номеклатуру");
				item.ID = selected;
				item.Activated += Item_Activated;
				menu.Add(item);
				menu.ShowAll();
				menu.Popup();
			}
		}

		void Item_Activated(object sender, EventArgs e)
		{
			var item = (sender as MenuItemId<WriteoffItem>).ID;
			MainClass.MainWin.NavigationManager.OpenViewModelOnTdi<NomenclatureViewModel, IEntityUoWBuilder>(MyTdiDialog, EntityUoWBuilder.ForOpen(item.Nomenclature.Id));
		}

		#endregion

		void YtreeItems_Selection_Changed (object sender, EventArgs e)
		{
			buttonDel.Sensitive = ytreeItems.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonAddStoreClicked (object sender, EventArgs e)
		{
			var selectJournal = MainClass.MainWin.NavigationManager.OpenViewModelOnTdi<StockBalanceJournalViewModel>(MyTdiDialog, QS.Navigation.OpenPageOptions.AsSlave);

			if(CurWarehouse != null) {
				selectJournal.ViewModel.Filter.Warehouse = CurWarehouse;
			}

			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += SelectFromStock_OnSelectResult;
		}

		void SelectFromStock_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			var selectVM = sender as StockBalanceJournalViewModel;

			foreach(var node in e.GetSelectedObjects<StockBalanceJournalNode>()) {
				WriteoffDoc.AddItem(node.GetStockPosition(UoW), selectVM.Filter.Warehouse, node.Amount);
			}
			CalculateTotal();
		}

		protected void OnButtonDelClicked (object sender, EventArgs e)
		{
			WriteoffDoc.RemoveItem (ytreeItems.GetSelectedObject<WriteoffItem> ());
			CalculateTotal();
		}

		protected void OnButtonAddWorkerClicked(object sender, EventArgs e)
		{
			var filter = new EmployeeBalanceFilter (MyOrmDialog.UoW);
			var selectFromEmployeeDlg = new ReferenceRepresentation (new EmployeeBalanceVM (filter),
			                                                        "Выданное сотрудникам");
			filter.parrentTab = selectFromEmployeeDlg;

			if(CurWorker != null)
				filter.RestrictEmployee = CurWorker;

			selectFromEmployeeDlg.ShowFilter = CurWorker == null;
			selectFromEmployeeDlg.Mode = OrmReferenceMode.MultiSelect;
			selectFromEmployeeDlg.ObjectSelected += SelectFromEmployeeDlg_ObjectSelected;

			OpenSlaveTab(selectFromEmployeeDlg);
		}

		protected void OnButtonAddObjectClicked(object sender, EventArgs e)
		{
			var filter = new ObjectBalanceFilter (MyOrmDialog.UoW);
			if (CurObject != null)
				filter.RestrictObject = CurObject;

			var selectFromObjectDlg = new ReferenceRepresentation (new ViewModel.ObjectBalanceVM (filter),
			                                                      "Выданное на объекты");
			selectFromObjectDlg.ShowFilter = CurObject == null;
			selectFromObjectDlg.Mode = OrmReferenceMode.MultiSelect;
			selectFromObjectDlg.ObjectSelected += SelectFromObjectDlg_ObjectSelected;;

			OpenSlaveTab(selectFromObjectDlg);
		}

		void SelectFromObjectDlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			foreach(var node in e.GetNodes<ViewModel.ObjectBalanceVMNode> ())
			{
				WriteoffDoc.AddItem (MyOrmDialog.UoW.GetById<SubdivisionIssueOperation> (node.Id), node.Added - node.Removed);
			}
			CalculateTotal();
		}

		void SelectFromEmployeeDlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			foreach(var node in e.GetNodes<EmployeeBalanceVMNode> ())
			{
				WriteoffDoc.AddItem (MyOrmDialog.UoW.GetById<EmployeeIssueOperation> (node.Id), node.Added - node.Removed);
			}
			CalculateTotal();
		}

		private void CalculateTotal()
		{
			labelSum.Markup = String.Format ("Позиций в документе: <u>{0}</u>  Количество единиц: <u>{1}</u>",
				WriteoffDoc.Items.Count,
				WriteoffDoc.Items.Sum(x => x.Amount)
			);
			buttonFillBuhDoc.Sensitive = WriteoffDoc.Items.Count > 0;
		}

		protected void OnButtonFillBuhDocClicked(object sender, EventArgs e)
		{
			using (var dlg = new Dialog("Введите бухгалтерский документ", MainClass.MainWin, DialogFlags.Modal))
			{
				var docEntry = new Entry(80);
				if (writeoffDoc.Items.Any(x => x.WriteoffFrom == WriteoffFrom.Employye))
					docEntry.Text = writeoffDoc.Items.First(x => x.WriteoffFrom == WriteoffFrom.Employye).BuhDocument;
				docEntry.TooltipText = "Бухгалтерский документ по которому было произведено списание. Отобразится вместо подписи сотрудника в карточке.";
				docEntry.ActivatesDefault = true;
				dlg.VBox.Add(docEntry);
				dlg.AddButton("Заменить", ResponseType.Ok);
				dlg.AddButton("Отмена", ResponseType.Cancel);
				dlg.DefaultResponse = ResponseType.Ok;
				dlg.ShowAll();
				if (dlg.Run() == (int)ResponseType.Ok)
				{
					writeoffDoc.ObservableItems
						.Where(x => x.WriteoffFrom == WriteoffFrom.Employye)
						.ToList().ForEach(x => x.BuhDocument = docEntry.Text);
				}
				dlg.Destroy();
			}
		}
	}
}