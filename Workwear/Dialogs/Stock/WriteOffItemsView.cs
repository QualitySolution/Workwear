using System;
using System.ComponentModel;
using System.Linq;
using Gtk;
using NLog;
using QS.Dialog.Gtk;
using QS.Project.Domain;
using QS.Project.Journal;
using QSOrmProject;
using QSWidgetLib;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using workwear.Representations.Organization;
using workwear.ViewModels.Stock;
using Workwear.Measurements;

namespace workwear
{
	[ToolboxItem(true)]
	public partial class WriteOffItemsView : WidgetOnDialogBase
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private enum ColumnTags { BuhDoc }
		private Writeoff writeOffDoc;
		public Writeoff WriteOffDoc {
			get => writeOffDoc;
			set { if (writeOffDoc == value)
					return;
				writeOffDoc = value;
				ytreeItems.ItemsDataSource = writeOffDoc.ObservableItems;
				writeOffDoc.ObservableItems.ListContentChanged += WriteoffDoc_ObservableItems_ListContentChanged;
				CalculateTotal();
				WriteOffDoc.Items.ToList().ForEach(item => item.PropertyChanged += Item_PropertyChanged);
			}
		}

		private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(e.PropertyName == nameof(WriteoffItem.BuhDocument))
				((EntityDialogBase<Writeoff>) MyEntityDialog).HasChanges = true;
		}

		private void WriteoffDoc_ObservableItems_ListContentChanged (object sender, EventArgs e) {
			CalculateTotal();
		}

		public EmployeeCard CurWorker { get; set;}

		public Subdivision CurObject { get; set;}

		public Warehouse CurWarehouse { get; set; }

		public WriteOffItemsView()
		{
			Build();

			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<WriteoffItem> ()
				.AddColumn ("Наименование").AddTextRenderer (e => e.Nomenclature.Name)
				.AddColumn("Размер").MinWidth(60)
					.AddComboRenderer(x => x.WearSize).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => SizeService.GetSize(UoW, x.Nomenclature?.Type?.SizeType, true, true))
					.AddSetter((c, n) => c.Editable = n.Nomenclature?.Type?.SizeType != null)
				.AddColumn("Рост").MinWidth(70)
					.AddComboRenderer(x => x.Height).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => SizeService.GetSize(UoW, x.Nomenclature?.Type?.HeightType, true, true))
					.AddSetter((c, n) => c.Editable = n.Nomenclature?.Type?.SizeType != null)
				.AddColumn ("Процент износа").AddNumericRenderer(e => e.WearPercent, new MultiplierToPercentConverter())
					.Editing(new Adjustment(0, 0, 999, 1, 10, 0)).WidthChars(6).Digits(0)
					.AddTextRenderer(e => "%", expand: false)
				.AddColumn ("Списано из").AddTextRenderer (e => e.LastOwnText)
				.AddColumn ("Количество").AddNumericRenderer (e => e.Amount).Editing (new Adjustment(0, 0, 100000, 1, 10, 1)).WidthChars(7)
					.AddTextRenderer (e => e.Nomenclature.Type.Units.Name)
				.AddColumn("Бухгалтерский документ").Tag(ColumnTags.BuhDoc).AddTextRenderer(e => e.BuhDocument)
					.AddSetter((c, e) => c.Editable = e.WriteoffFrom == WriteoffFrom.Employee)
				.Finish ();
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
			ytreeItems.ButtonReleaseEvent += YtreeItems_ButtonReleaseEvent;
		}
		#region PopupMenu

		private void YtreeItems_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args) {
			if (args.Event.Button != 3) return;
			var menu = new Menu();
			var selected = ytreeItems.GetSelectedObject<WriteoffItem>();
			var item = new MenuItemId<WriteoffItem>("Открыть номеклатуру");
			item.ID = selected;
			if(selected == null)
				item.Sensitive = false;
			else
				item.Activated += Item_Activated;
			menu.Add(item);
			menu.ShowAll();
			menu.Popup();
		}

		private void Item_Activated(object sender, EventArgs e) {
			var item = (sender as MenuItemId<WriteoffItem>).ID;
			MainClass.MainWin.NavigationManager.
				OpenViewModelOnTdi<NomenclatureViewModel, IEntityUoWBuilder>(MyTdiDialog, EntityUoWBuilder.ForOpen(item.Nomenclature.Id));
		}

		#endregion
		private void YtreeItems_Selection_Changed (object sender, EventArgs e) {
			buttonDel.Sensitive = ytreeItems.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonAddStoreClicked (object sender, EventArgs e) {
			var selectJournal = 
				MainClass.MainWin.NavigationManager
					.OpenViewModelOnTdi<StockBalanceJournalViewModel>(MyTdiDialog, QS.Navigation.OpenPageOptions.AsSlave);

			if(CurWarehouse != null) {
				selectJournal.ViewModel.Filter.Warehouse = CurWarehouse;
			}

			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += SelectFromStock_OnSelectResult;
		}

		private void SelectFromStock_OnSelectResult(object sender, JournalSelectedEventArgs e) {
			var selectVM = sender as StockBalanceJournalViewModel;
			foreach(var node in e.GetSelectedObjects<StockBalanceJournalNode>()) {
				WriteOffDoc.AddItem(node.GetStockPosition(UoW), selectVM.Filter.Warehouse, node.Amount);
			}
			CalculateTotal();
		}

		private void OnButtonDelClicked (object sender, EventArgs e) {
			WriteOffDoc.RemoveItem (ytreeItems.GetSelectedObject<WriteoffItem> ());
			CalculateTotal();
		}

		private void OnButtonAddWorkerClicked(object sender, EventArgs e) {
			var filter = new EmployeeBalanceFilter (MyOrmDialog.UoW);
			var selectFromEmployeeDlg = 
				new ReferenceRepresentation (new EmployeeBalanceVM (filter), "Выданное сотрудникам");
			filter.parrentTab = selectFromEmployeeDlg;

			if(CurWorker != null)
				filter.RestrictEmployee = CurWorker;

			selectFromEmployeeDlg.ShowFilter = CurWorker == null;
			selectFromEmployeeDlg.Mode = OrmReferenceMode.MultiSelect;
			selectFromEmployeeDlg.ObjectSelected += SelectFromEmployeeDlg_ObjectSelected;
			OpenSlaveTab(selectFromEmployeeDlg);
		}

		private void OnButtonAddObjectClicked(object sender, EventArgs e) {
			var filter = new ObjectBalanceFilter (MyOrmDialog.UoW);
			if (CurObject != null)
				filter.RestrictObject = CurObject;

			var selectFromObjectDlg = 
				new ReferenceRepresentation (new ViewModel.ObjectBalanceVM (filter), "Выданное на объекты");
			selectFromObjectDlg.ShowFilter = CurObject == null;
			selectFromObjectDlg.Mode = OrmReferenceMode.MultiSelect;
			selectFromObjectDlg.ObjectSelected += SelectFromObjectDlg_ObjectSelected;;
			OpenSlaveTab(selectFromObjectDlg);
		}

		private void SelectFromObjectDlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e) {
			foreach(var node in e.GetNodes<ViewModel.ObjectBalanceVMNode> ()) {
				WriteOffDoc.AddItem(MyOrmDialog.UoW.GetById<SubdivisionIssueOperation> (node.Id), node.Added - node.Removed);
			}
			CalculateTotal();
		}

		private void SelectFromEmployeeDlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e) {
			foreach(var node in e.GetNodes<EmployeeBalanceVMNode> ()) {
				WriteOffDoc.AddItem(MyOrmDialog.UoW.GetById<EmployeeIssueOperation> (node.Id), node.Added - node.Removed);
			}
			CalculateTotal();
		}

		private void CalculateTotal() {
			labelSum.Markup = String.Format ("Позиций в документе: <u>{0}</u>  Количество единиц: <u>{1}</u>",
				WriteOffDoc.Items.Count,
				WriteOffDoc.Items.Sum(x => x.Amount)
			);
			buttonFillBuhDoc.Sensitive = WriteOffDoc.Items.Count > 0;
		}

		private void OnButtonFillBuhDocClicked(object sender, EventArgs e) {
			using (var dlg = new Dialog("Введите бухгалтерский документ", MainClass.MainWin, DialogFlags.Modal)) {
				var docEntry = new Entry(80);
				if (writeOffDoc.Items.Any(x => x.WriteoffFrom == WriteoffFrom.Employee))
					docEntry.Text = writeOffDoc.Items.First(x => x.WriteoffFrom == WriteoffFrom.Employee).BuhDocument;
				docEntry.TooltipText = "Бухгалтерский документ по которому было произведено списание. Отобразится вместо подписи сотрудника в карточке.";
				docEntry.ActivatesDefault = true;
				dlg.VBox.Add(docEntry);
				dlg.AddButton("Заменить", ResponseType.Ok);
				dlg.AddButton("Отмена", ResponseType.Cancel);
				dlg.DefaultResponse = ResponseType.Ok;
				dlg.ShowAll();
				if (dlg.Run() == (int)ResponseType.Ok) {
					writeOffDoc.ObservableItems
						.Where(x => x.WriteoffFrom == WriteoffFrom.Employee)
						.ToList().ForEach(x => x.BuhDocument = docEntry.Text);
				}
				dlg.Destroy();
			}
		}
	}
}