using System;
using Gtk;
using QSProjectsLib;

namespace workwear
{
	public partial class SelectWearCardRow : Gtk.Dialog
	{
		private TreeIter SelectedIter;
		public event EventHandler WorkerIdChanged;

		public bool WorkerComboActive
		{
			get{return comboWorker.Sensitive;}
			set{comboWorker.Sensitive = value;
				FillWorkerCombo();}
		}

		public int WorkerId
		{
			get{return ComboWorks.GetActiveId(comboWorker);}
			set{if (WorkerComboActive)
					ComboWorks.SetActiveItem(comboWorker, value);}
		}

		public SelectWearCardRow(TreeModel TableRows)
		{
			this.Build();

			treeviewCardRows.AppendColumn ("Наименование", new Gtk.CellRendererText (), "text", 2);
			treeviewCardRows.AppendColumn ("Дата выдачи", new Gtk.CellRendererText (), "text", 3);
			CellRendererText QuantityCell = new Gtk.CellRendererText();
			treeviewCardRows.AppendColumn ("Количество", QuantityCell, "text", 4);
			treeviewCardRows.AppendColumn ("Годность(выдано)", new Gtk.CellRendererText (), "text", 5);
			treeviewCardRows.AppendColumn ("Годность(на сегодня)", new Gtk.CellRendererText (), "text", 6);

			treeviewCardRows.Columns[2].SetCellDataFunc(QuantityCell, RenderQuantityColumn);

			treeviewCardRows.Model = TableRows;
			treeviewCardRows.ShowAll();
		}

		private void FillWorkerCombo()
		{
			string sql = "SELECT id, last_name, first_name, patronymic_name FROM wear_cards";
			string Display = "{1} {2} {3}";
			ComboWorks.ComboFillUniversal (comboWorker, sql, Display, null, 0, 1);
		}

		private void RenderQuantityColumn (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			int Quantity = (int) model.GetValue (iter, 4);
			string unit = (string) model.GetValue (iter, 10);
			(cell as Gtk.CellRendererText).Text = String.Format("{0} {1}", Quantity, unit);
		}

		public bool GetResult(out TreeIter iter)
		{
			bool Result = false;
			this.Show ();
			if((ResponseType) this.Run () == ResponseType.Ok)
			{
				iter = SelectedIter;
				Result = true;
			}
			else
				iter = new TreeIter();
			this.Destroy ();

			return Result;
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			treeviewCardRows.Selection.GetSelected(out SelectedIter);
		}

		protected void OnTreeviewCardRowsCursorChanged(object sender, EventArgs e)
		{
			bool isSelect = treeviewCardRows.Selection.CountSelectedRows() == 1;
			buttonOk.Sensitive = isSelect;
		}

		protected void OnTreeviewCardRowsRowActivated(object o, RowActivatedArgs args)
		{
			buttonOk.Click ();
		}

		protected void OnComboWorkerChanged(object sender, EventArgs e)
		{
			if(WorkerIdChanged != null)
				WorkerIdChanged(this, EventArgs.Empty);

		}
	}
}

