using System;
using Gtk;

namespace workwear
{
	public partial class SelectWearCardRow : Gtk.Dialog
	{
		private TreeIter SelectedIter;

		public SelectWearCardRow(TreeModel TableRows)
		{
			this.Build();

			treeviewCardRows.AppendColumn ("Наименование", new Gtk.CellRendererText (), "text", 2);
			treeviewCardRows.AppendColumn ("Дата выдачи", new Gtk.CellRendererText (), "text", 3);
			treeviewCardRows.AppendColumn ("Количество", new Gtk.CellRendererText (), "text", 4);
			treeviewCardRows.AppendColumn ("Годность(выдано)", new Gtk.CellRendererText (), "text", 5);
			treeviewCardRows.AppendColumn ("Годность(на сегодня)", new Gtk.CellRendererText (), "text", 6);

			treeviewCardRows.Model = TableRows;
			treeviewCardRows.ShowAll();
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
	}
}

