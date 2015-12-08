using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using NLog;
using QSOrmProject;
using workwear.Domain.Stock;

namespace workwear
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ExpenseDocItemsView : WidgetOnDialogBase
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private Expense expenceDoc;

		public Expense ExpenceDoc {
			get {return expenceDoc;}
			set {
				if (expenceDoc == value)
					return;
				expenceDoc = value;
				ytreeItems.ItemsDataSource = expenceDoc.ObservableItems;
			}
		}

/*		public int ObjectId {
			get {return _ObjectId;}
			set {_ObjectId = value;

				string sql = "SELECT name, id FROM object_places WHERE object_id = @id";
				MySqlParameter[] param = new MySqlParameter[]{ new MySqlParameter("@id", value)};
				ComboBox PlacementCombo = new ComboBox();
				ComboWorks.ComboFillUniversal(PlacementCombo, sql, "{0}", param, 1, ComboWorks.ListMode.WithNo);
				CellPlacement.Model = PlacementList = PlacementCombo.Model;
				PlacementCombo.Destroy ();

				FillStockList ();}
		}
*/
/*		public ExpenseOperations Operation {
			get {return _Operation;}
			set {
				buttonAdd.Sensitive = (ExpenceDoc > 0  && value == ExpenseOperations.Employee) || 
					(ObjectId > 0 && value == ExpenseOperations.Object);
				PlacementColumn.Visible = value == ExpenseOperations.Object;

				if (_Operation == value)
					return;
				_Operation = value;
				ItemsListStore.Clear();
			}
		}
*/
		public ExpenseDocItemsView()
		{
			this.Build();

			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ExpenseItem> ()
				.AddColumn ("Наименование").AddTextRenderer (e => e.Nomenclature.Name)
				.AddColumn ("Состояние").AddTextRenderer (e => (e.IncomeOn.LifePercent).ToString ("P0"))
				.AddColumn ("Количество").AddNumericRenderer (e => e.Amount).Editing (new Adjustment(0, 0, 100000, 1, 10, 1))
					.AddTextRenderer (e => e.Nomenclature.Type.Units.Name)
				//.AddColumn ("Расположение").AddTextRenderer (e => e.AvgCostText)
				.Finish ();
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
		}

		void YtreeItems_Selection_Changed (object sender, EventArgs e)
		{
			buttonDel.Sensitive = ytreeItems.Selection.CountSelectedRows () > 0;
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			if(ExpenceDoc.Operation == ExpenseOperations.Employee)
			{
				var selectDlg = new ReferenceRepresentation (new ViewModel.StockBalanceVM (MyOrmDialog.UoW));
				selectDlg.Mode = OrmReferenceMode.MultiSelect;
				selectDlg.ObjectSelected += SelectDlg_ObjectSelected;

				var dialog = new OneWidgetDialog (selectDlg);
				dialog.Show ();
				dialog.Run ();
				dialog.Destroy ();
			}
		}

		void SelectDlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			foreach(var node in e.GetNodes<ViewModel.StockBalanceVMNode> ())
			{
				ExpenceDoc.AddItem (MyOrmDialog.UoW.GetById<IncomeItem> (node.Id));
			}
			CalculateTotal();
		}

		protected void OnButtonDelClicked (object sender, EventArgs e)
		{
			ExpenceDoc.RemoveItem (ytreeItems.GetSelectedObject<ExpenseItem> ());
			CalculateTotal();
		}

		private void CalculateTotal()
		{
			labelSum.Text = String.Format ("Количество: {0}", ExpenceDoc.Items.Count);
		} 
	}
}

