using System;
using Gtk;
using MySql.Data.MySqlClient;
using QSProjectsLib;
using workwear;

public partial class MainWindow : Gtk.Window
{
	Gtk.ListStore CardsListStore;
	Gtk.TreeModelFilter CardsFilter;

	void PrepareCards()
	{
		CardsListStore = new Gtk.ListStore (typeof (int),typeof (string), typeof (string));

		treeviewCards.AppendColumn("Код", new Gtk.CellRendererText (), "text", 0);
		treeviewCards.AppendColumn("Ф.И.О.", new Gtk.CellRendererText (), "text", 1);
		treeviewCards.AppendColumn("Объект", new Gtk.CellRendererText (), "text", 2);

		CardsFilter = new Gtk.TreeModelFilter (CardsListStore, null);
		CardsFilter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FilterTreeCards);
		treeviewCards.Model = CardsFilter;
		treeviewCards.ShowAll();
	}

	void UpdateCards()
	{
		MainClass.StatusMessage("Получаем таблицу Карточек...");

		string sql = "SELECT wear_cards.id, wear_cards.last_name, wear_cards.first_name, wear_cards.patronymic_name, objects.name as object FROM wear_cards " +
		"LEFT JOIN objects ON objects.id = wear_cards.object_id ";
		MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);

		MySqlDataReader rdr = cmd.ExecuteReader();

		CardsListStore.Clear();
		while (rdr.Read())
		{
			CardsListStore.AppendValues(rdr.GetInt32("id"),
			                            String.Format("{0} {1} {2}", rdr["last_name"].ToString(), rdr["first_name"].ToString(), rdr["patronymic_name"].ToString()),
			                            rdr["object"].ToString());
		}
		rdr.Close();
		MainClass.StatusMessage("Ok");
		bool isSelect = treeviewCards.Selection.CountSelectedRows() == 1;
		buttonEdit.Sensitive = isSelect;
		buttonDelete.Sensitive = isSelect;
	}

	private bool FilterTreeCards (Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		if (entryCardsSearch.Text == "")
			return true;
		bool filterName = true;
		//bool filterAddress = true;
		string cellvalue;

		if(model.GetValue (iter, 1) == null)
			return false;

		if (entryCardsSearch.Text != "" && model.GetValue (iter, 1) != null)
		{
			cellvalue  = model.GetValue (iter, 1).ToString();
			filterName = cellvalue.IndexOf (entryCardsSearch.Text, StringComparison.CurrentCultureIgnoreCase) > -1;
		}
		return (filterName);
	}

	protected void OnButtonCardsSearchClearClicked(object sender, EventArgs e)
	{
		entryCardsSearch.Text = "";
	}

	protected void OnEntryCardsSearchChanged(object sender, EventArgs e)
	{
		CardsFilter.Refilter();
	}

	protected void OnTreeviewCardsCursorChanged(object sender, EventArgs e)
	{
		bool isSelect = treeviewCards.Selection.CountSelectedRows() == 1;
		buttonEdit.Sensitive = isSelect;
		buttonDelete.Sensitive = isSelect;
	}

	protected void OnTreeviewCardsRowActivated(object o, RowActivatedArgs args)
	{
		buttonEdit.Click();
	}

}

