using System;
using Gtk;
using NLog;
using QSOrmProject;
using workwear.Domain;
using workwear.Domain.Stock;
using System.Linq;

namespace workwear
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WriteOffItemsView : WidgetOnDialogBase
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

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
			}
		}

		void WriteoffDoc_ObservableItems_ListContentChanged (object sender, EventArgs e)
		{
			CalculateTotal();
		}

		public EmployeeCard CurWorker { get; set;}

		public Facility CurObject { get; set;}

		public WriteOffItemsView()
		{
			this.Build();

			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<WriteoffItem> ()
				.AddColumn ("Наименование").AddTextRenderer (e => e.Nomenclature.Name)
				.AddColumn ("Размер").AddTextRenderer (e => e.Nomenclature.Size)
				.AddColumn ("Рост").AddTextRenderer (e => e.Nomenclature.WearGrowth)
				.AddColumn ("% годности").AddTextRenderer (e => e.IncomeOn != null ? e.IncomeOn.LifePercent.ToString ("P0") : String.Empty)
				.AddColumn ("Списано из").AddTextRenderer (e => e.LastOwnText)
				.AddColumn ("Количество").AddNumericRenderer (e => e.Amount).Editing (new Adjustment(0, 0, 100000, 1, 10, 1)).WidthChars(7)
				.AddTextRenderer (e => e.Nomenclature.Type.Units.Name)
				.AddColumn("")
				.Finish ();
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
		}

		void YtreeItems_Selection_Changed (object sender, EventArgs e)
		{
			buttonDel.Sensitive = ytreeItems.Selection.CountSelectedRows () > 0;
		}

		protected void OnButtonAddStoreClicked (object sender, EventArgs e)
		{
			var selectFromStockDlg = new ReferenceRepresentation (new ViewModel.StockBalanceVM ());
			selectFromStockDlg.Mode = OrmReferenceMode.MultiSelect;
			selectFromStockDlg.ObjectSelected += SelectFromStockDlg_ObjectSelected;;

			var dialog = new OneWidgetDialog (selectFromStockDlg);
			dialog.Show ();
			dialog.Run ();
			dialog.Destroy ();
		}

		void SelectFromStockDlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			foreach(var node in e.GetNodes<ViewModel.StockBalanceVMNode> ())
			{
				WriteoffDoc.AddItem (MyOrmDialog.UoW.GetById<IncomeItem> (node.Id), node.Income - node.Expense);
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

			var selectFromEmployeeDlg = new ReferenceRepresentation (new ViewModel.EmployeeBalanceVM (filter));
			selectFromEmployeeDlg.ShowFilter = CurWorker == null;
			selectFromEmployeeDlg.Mode = OrmReferenceMode.MultiSelect;
			selectFromEmployeeDlg.ObjectSelected += SelectFromEmployeeDlg_ObjectSelected;

			var dialog = new OneWidgetDialog (selectFromEmployeeDlg);
			dialog.Show ();
			dialog.Run ();
			dialog.Destroy ();
		}

		protected void OnButtonAddObjectClicked(object sender, EventArgs e)
		{
			var filter = new ObjectBalanceFilter (MyOrmDialog.UoW);
			if (CurObject != null)
				filter.RestrictObject = CurObject;

			var selectFromObjectDlg = new ReferenceRepresentation (new ViewModel.ObjectBalanceVM (filter));
			selectFromObjectDlg.ShowFilter = CurObject == null;
			selectFromObjectDlg.Mode = OrmReferenceMode.MultiSelect;
			selectFromObjectDlg.ObjectSelected += SelectFromObjectDlg_ObjectSelected;;

			var dialog = new OneWidgetDialog (selectFromObjectDlg);
			dialog.Show ();
			dialog.Run ();
			dialog.Destroy ();
		}

		void SelectFromObjectDlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			foreach(var node in e.GetNodes<ViewModel.ObjectBalanceVMNode> ())
			{
				WriteoffDoc.AddItem (MyOrmDialog.UoW.GetById<ExpenseItem> (node.Id), node.Added - node.Removed);
			}
			CalculateTotal();
		}

		void SelectFromEmployeeDlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			foreach(var node in e.GetNodes<ViewModel.EmployeeBalanceVMNode> ())
			{
				WriteoffDoc.AddItem (MyOrmDialog.UoW.GetById<ExpenseItem> (node.Id), node.Added - node.Removed);
			}
			CalculateTotal();
		}

		private void CalculateTotal()
		{
			labelSum.Markup = String.Format ("Позиций в документе: <u>{0}</u>  Количество единиц: <u>{1}</u>",
				WriteoffDoc.Items.Count,
				WriteoffDoc.Items.Sum(x => x.Amount)
			);
		} 

	}
}

