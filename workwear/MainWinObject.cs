using System;
using Gtk;
using MySql.Data.MySqlClient;
using QSProjectsLib;
using workwear;

public partial class MainWindow : Gtk.Window
{
	Gtk.ListStore ObjectListStore;
	Gtk.TreeModelFilter ObjectFilter;

	void PrepareObject()
	{
		ObjectListStore = new Gtk.ListStore (typeof (int),typeof (string), typeof (string));

		treeviewObjects.AppendColumn("Код", new Gtk.CellRendererText (), "text", 0);
		treeviewObjects.AppendColumn("Название", new Gtk.CellRendererText (), "text", 1);
		treeviewObjects.AppendColumn("Адрес", new Gtk.CellRendererText (), "text", 2);

		ObjectFilter = new Gtk.TreeModelFilter (ObjectListStore, null);
		ObjectFilter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FilterTreeObject);
		treeviewObjects.Model = ObjectFilter;
		treeviewObjects.ShowAll();
	}

	void UpdateObject()
	{
		logger.Info("Получаем таблицу объектов...");

		string sql = "SELECT objects.* FROM objects";
		MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);

		MySqlDataReader rdr = cmd.ExecuteReader();

		ObjectListStore.Clear();
		while (rdr.Read())
		{
			ObjectListStore.AppendValues(rdr.GetInt32("id"),
			                              rdr["name"].ToString(),
			                              rdr["address"].ToString());
		}
		rdr.Close();
		logger.Info("Ok");
		bool isSelect = treeviewObjects.Selection.CountSelectedRows() == 1;
		buttonEdit.Sensitive = isSelect;
		buttonDelete.Sensitive = isSelect;
	}

	private bool FilterTreeObject (Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		if (entryObjectSearch.Text == "")
			return true;
		bool filterName = true;
		bool filterAddress = true;
		string cellvalue;

		if(model.GetValue (iter, 1) == null)
			return false;

		if (entryObjectSearch.Text != "" && model.GetValue (iter, 1) != null)
		{
			cellvalue  = model.GetValue (iter, 1).ToString();
			filterName = cellvalue.IndexOf (entryObjectSearch.Text, StringComparison.CurrentCultureIgnoreCase) > -1;
		}
		if (entryObjectSearch.Text != "" && model.GetValue (iter, 2) != null)
		{
			cellvalue  = model.GetValue (iter, 2).ToString();
			filterAddress = cellvalue.IndexOf (entryObjectSearch.Text, StringComparison.CurrentCultureIgnoreCase) > -1;
		}

		return (filterName || filterAddress);
	}

	protected void OnButtonObjectSearchCleanClicked(object sender, EventArgs e)
	{
		entryObjectSearch.Text = "";
	}

	protected void OnEntryObjectSearchChanged(object sender, EventArgs e)
	{
		ObjectFilter.Refilter();
	}

	protected void OnTreeviewObjectsCursorChanged(object sender, EventArgs e)
	{
		bool isSelect = treeviewObjects.Selection.CountSelectedRows() == 1;
		buttonEdit.Sensitive = isSelect;
		buttonDelete.Sensitive = isSelect;
	}

	protected void OnTreeviewObjectsRowActivated(object o, RowActivatedArgs args)
	{
		buttonEdit.Click();
	}

}

