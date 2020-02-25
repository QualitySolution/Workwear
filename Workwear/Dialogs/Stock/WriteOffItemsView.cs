using System;
using System.Linq;
using Gtk;
using NLog;
using QS.Dialog.Gtk;
using QSOrmProject;
using workwear.Domain.Operations;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.Representations.Organization;
using workwear.JournalFilters;
using workwear.ViewModel;

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
				.AddColumn ("Размер").AddTextRenderer (e => e.Nomenclature.Size)
				.AddColumn ("Рост").AddTextRenderer (e => e.Nomenclature.WearGrowth)
				.AddColumn ("% износа").AddTextRenderer (e => e.WearPercent != null ? e.WearPercent.Value.ToString("P2") : null)
				.AddColumn ("Списано из").AddTextRenderer (e => e.LastOwnText)
				.AddColumn ("Количество").AddNumericRenderer (e => e.Amount).Editing (new Adjustment(0, 0, 100000, 1, 10, 1)).WidthChars(7)
				.AddTextRenderer (e => e.Nomenclature.Type.Units.Name)
				.AddColumn("Бухгалтерский документ").Tag(ColumnTags.BuhDoc).AddTextRenderer(e => e.BuhDocument)
				.AddSetter((c, e) => c.Editable = e.IssuedOn?.ExpenseDoc.Operation == ExpenseOperations.Employee)
				.Finish ();

			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
		}

		void YtreeItems_Selection_Changed (object sender, EventArgs e)
		{
			buttonDel.Sensitive = ytreeItems.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonAddStoreClicked (object sender, EventArgs e)
		{
			var filter = new WarehouseFilter(MyOrmDialog.UoW);

			if(CurWarehouse != null)
				filter.RestrictWarehouse = CurWarehouse;
	
			var selectFromStockDlg = new ReferenceRepresentation(new StockBalanceVM(filter),
																	"Остатки на складе");
			selectFromStockDlg.ShowFilter = CurWarehouse == null;
			selectFromStockDlg.Mode = OrmReferenceMode.MultiSelect;
			selectFromStockDlg.ObjectSelected += SelectFromStockDlg_ObjectSelected;;

			OpenSlaveTab(selectFromStockDlg);
		}

		void SelectFromStockDlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			foreach(var node in e.GetNodes<ViewModel.StockBalanceVMNode> ())
			{
				WriteoffDoc.AddItem (UoW, MyOrmDialog.UoW.GetById<Nomenclature> (node.Id), node.Size, node.Growth, node.WearPercent, node.Amount);
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
			if (CurWorker != null)
				filter.RestrictEmployee = CurWorker;

			var selectFromEmployeeDlg = new ReferenceRepresentation (new EmployeeBalanceVM (filter),
			                                                        "Выданное сотрудникам");
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
				//WriteoffDoc.AddItem (MyOrmDialog.UoW.GetById<ExpenseItem> (node.Id), node.Added - node.Removed);
			}
			CalculateTotal();
		}

		void SelectFromEmployeeDlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			foreach(var node in e.GetNodes<EmployeeBalanceVMNode> ())
			{
				WriteoffDoc.AddItem (UoW, MyOrmDialog.UoW.GetById<EmployeeIssueOperation> (node.Id), node.Added - node.Removed);
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
				if (writeoffDoc.Items.Any(x => x.IssuedOn?.ExpenseDoc.Operation == ExpenseOperations.Employee))
					docEntry.Text = writeoffDoc.Items.First(x => x.IssuedOn?.ExpenseDoc.Operation == ExpenseOperations.Employee).BuhDocument;
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
						.Where(x => x.IssuedOn?.ExpenseDoc.Operation == ExpenseOperations.Employee)
						.ToList().ForEach(x => x.BuhDocument = docEntry.Text);
				}
				dlg.Destroy();
			}
		}
	}
}

