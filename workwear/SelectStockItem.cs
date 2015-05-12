using System;
using System.Collections.Generic;
using Gtk;

namespace workwear
{
	public partial class SelectStockItem : Gtk.Dialog
	{
		private List<TreeIter> selectedItems;
		public event EventHandler SearchTextChanged;
		public string SearchText;

		public SelectStockItem(TreeModel TableRows)
		{
			this.Build();

			treeviewStock.AppendColumn ("Наименование", new Gtk.CellRendererText (), "text", 2);
			treeviewStock.AppendColumn ("Размер", new Gtk.CellRendererText (), "text", 3);
			treeviewStock.AppendColumn ("Рост", new Gtk.CellRendererText (), "text", 4);
			CellRendererText QuantityCell = new Gtk.CellRendererText();
			treeviewStock.AppendColumn ("В наличии", QuantityCell, "text", 5);
			CellRendererText LifeCell = new Gtk.CellRendererText();
			treeviewStock.AppendColumn ("Годность", LifeCell, "text", 7);

			treeviewStock.Columns[3].SetCellDataFunc(QuantityCell, RenderQuantityColumn);
			treeviewStock.Columns[4].SetCellDataFunc(LifeCell, RenderLifeColumn);

			treeviewStock.Model = TableRows;
			treeviewStock.Selection.Changed += OnTreeviewStock_Selection_Changed;
			treeviewStock.Selection.Mode = SelectionMode.Multiple;
			treeviewStock.ShowAll();
		}

		void OnTreeviewStock_Selection_Changed (object sender, EventArgs e)
		{
			buttonOk.Sensitive = treeviewStock.Selection.CountSelectedRows() > 0;
		}

		public bool GetResult(out TreeIter[] items)
		{
			bool Result = false;
			this.Show ();
			if((ResponseType) this.Run () == ResponseType.Ok)
			{
				items = selectedItems.ToArray();
				Result = true;
			}
			else
				items = null;
			this.Destroy ();

			return Result;
		}

		private void RenderQuantityColumn (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			int Quantity = (int) model.GetValue (iter, 5);
			string Unit = (string) model.GetValue (iter, 6);
			(cell as Gtk.CellRendererText).Text = String.Format("{0} {1}", Quantity, Unit);
		}

		private void RenderLifeColumn (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			double Life = (double) model.GetValue (iter, 7);
			(cell as Gtk.CellRendererText).Text = String.Format("{0} %", Life);
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			selectedItems = new List<TreeIter>();
			treeviewStock.Selection.SelectedForeach((model, path, iter) => selectedItems.Add(iter));
		}

		protected void OnTreeviewStockRowActivated(object o, RowActivatedArgs args)
		{
			buttonOk.Click ();
		}

		protected void OnButtonSearchCleanClicked(object sender, EventArgs e)
		{
			entrySearch.Text = "";
		}

		protected void OnEntrySearchChanged(object sender, EventArgs e)
		{
			SearchText = entrySearch.Text;
			if(SearchTextChanged != null)
				SearchTextChanged(this, EventArgs.Empty);
		}
	}
}

