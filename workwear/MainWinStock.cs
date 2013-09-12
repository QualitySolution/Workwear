using System;
using Gtk;
using MySql.Data.MySqlClient;
using QSProjectsLib;
using workwear;

public partial class MainWindow : Gtk.Window
{
	Gtk.ListStore IncomeListStore;
	Gtk.TreeModelFilter IncomeFilter;

	Gtk.ListStore ExpenseListStore;
	Gtk.TreeModelFilter ExpenseFilter;

	Gtk.ListStore WriteOffListStore;
	Gtk.TreeModelFilter WriteOffFilter;

	void PrepareStock()
	{
		//Приход
		IncomeListStore = new Gtk.ListStore (typeof (int),
		                                     typeof(string),
		                                     typeof (string),
		                                     typeof (string),
		                                     typeof (string));

		treeviewIncome.AppendColumn("Номер", new Gtk.CellRendererText (), "text", 0);
		treeviewIncome.AppendColumn("Дата", new Gtk.CellRendererText (), "text", 1);
		treeviewIncome.AppendColumn("Документ", new Gtk.CellRendererText (), "text", 2);
		treeviewIncome.AppendColumn("ТТН", new Gtk.CellRendererText (), "text", 3);
		treeviewIncome.AppendColumn("Сотрудник", new Gtk.CellRendererText (), "text", 4);

		IncomeFilter = new Gtk.TreeModelFilter (IncomeListStore, null);
		IncomeFilter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FilterTreeIncome);
		treeviewIncome.Model = IncomeFilter;
		treeviewIncome.ShowAll();

		//Расход
		ExpenseListStore = new Gtk.ListStore (typeof (int),
		                                      typeof(string),
		                                     typeof (string),
		                                     typeof (string));

		treeviewExpense.AppendColumn("Номер", new Gtk.CellRendererText (), "text", 0);
		treeviewExpense.AppendColumn("Дата", new Gtk.CellRendererText (), "text", 1);
		treeviewExpense.AppendColumn("Документ", new Gtk.CellRendererText (), "text", 2);
		treeviewExpense.AppendColumn("Сотрудник", new Gtk.CellRendererText (), "text", 3);

		ExpenseFilter = new Gtk.TreeModelFilter (ExpenseListStore, null);
		ExpenseFilter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FilterTreeIncome); //FIXME
		treeviewExpense.Model = ExpenseFilter;
		treeviewExpense.ShowAll();

		//Списание
		WriteOffListStore = new Gtk.ListStore (typeof (int),
		                                       typeof(string),
		                                      typeof (string),
		                                      typeof (string));

		treeviewWriteOff.AppendColumn("Номер", new Gtk.CellRendererText (), "text", 0);
		treeviewWriteOff.AppendColumn("Дата", new Gtk.CellRendererText (), "text", 1);
		treeviewWriteOff.AppendColumn("Документ", new Gtk.CellRendererText (), "text", 2);
		treeviewWriteOff.AppendColumn("Пользователь", new Gtk.CellRendererText (), "text", 3);

		WriteOffFilter = new Gtk.TreeModelFilter (WriteOffListStore, null);
		WriteOffFilter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FilterTreeIncome); //FIXME
		treeviewWriteOff.Model = WriteOffFilter;
		treeviewWriteOff.ShowAll();
	}

	void UpdateStock()
	{
		switch (notebookStock.CurrentPage)
		{
			case 0:
				UpdateStockIncome();
				break;
			case 1:
				UpdateStockExpense();
				break;
			case 3:
				UpdateStockWriteOff();
				break;
		}
	}

	void UpdateStockIncome()
	{
		MainClass.StatusMessage("Получаем таблицу приходных документов...");

		string sql = "SELECT stock_income.*, wear_cards.last_name, wear_cards.first_name, wear_cards.patronymic_name FROM stock_income " +
			"LEFT JOIN wear_cards ON wear_cards.id = stock_income.wear_card_id ";
		if (!selectStockDates.IsAllTime)
		{
			sql += "WHERE stock_income.date BETWEEN @start AND @end";
		}
		MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
		cmd.Parameters.AddWithValue("@start", selectStockDates.DateBegin);
		cmd.Parameters.AddWithValue("@end", selectStockDates.DateEnd);

		using (MySqlDataReader rdr = cmd.ExecuteReader())
		{
			IncomeListStore.Clear();
			string doc = "";
			while (rdr.Read())
			{
				switch (rdr.GetString("operation"))
				{
					case "enter":
						doc = "Приходная накладная";
						break;
					case "return":
						doc = "Возврат от сотрудника";
						break;
				}
				IncomeListStore.AppendValues(rdr.GetInt32("id"),
				                            String.Format("{0:d}", rdr.GetDateTime("date")),
				                            doc,
				                            rdr["number"].ToString(),
				                            String.Format("{0} {1} {2}", rdr["last_name"].ToString(), rdr["first_name"].ToString(), rdr["patronymic_name"].ToString())
				);
			}
		}
		MainClass.StatusMessage("Ok");
		bool isSelect = treeviewIncome.Selection.CountSelectedRows() == 1;
		buttonEdit.Sensitive = isSelect;
		buttonDelete.Sensitive = isSelect;
	}

	void UpdateStockExpense()
	{
		MainClass.StatusMessage("Получаем таблицу расходных документов...");

		string sql = "SELECT stock_expense.*, wear_cards.last_name, wear_cards.first_name, wear_cards.patronymic_name FROM stock_expense " +
			"LEFT JOIN wear_cards ON wear_cards.id = stock_expense.wear_card_id ";
		if (!selectStockDates.IsAllTime)
		{
			sql += "WHERE stock_expense.date BETWEEN @start AND @end";
		}
		MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
		cmd.Parameters.AddWithValue("@start", selectStockDates.DateBegin);
		cmd.Parameters.AddWithValue("@end", selectStockDates.DateEnd);

		using (MySqlDataReader rdr = cmd.ExecuteReader())
		{
			ExpenseListStore.Clear();
			string doc = "Выдача сотруднику";
			while (rdr.Read())
			{
				ExpenseListStore.AppendValues(rdr.GetInt32("id"),
				                             String.Format("{0:d}", rdr.GetDateTime("date")),
				                             doc,
				                             String.Format("{0} {1} {2}", rdr["last_name"].ToString(), rdr["first_name"].ToString(), rdr["patronymic_name"].ToString())
				                             );
			}
		}
		MainClass.StatusMessage("Ok");
		bool isSelect = treeviewExpense.Selection.CountSelectedRows() == 1;
		buttonEdit.Sensitive = isSelect;
		buttonDelete.Sensitive = isSelect;
	}

	void UpdateStockWriteOff()
	{
		MainClass.StatusMessage("Получаем таблицу актов списания...");

		string sql = "SELECT stock_write_off.*, users.name as user FROM stock_write_off " +
			"LEFT JOIN users ON users.id = stock_write_off.user_id ";
		if (!selectStockDates.IsAllTime)
		{
			sql += "WHERE stock_write_off.date BETWEEN @start AND @end";
		}
		MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
		cmd.Parameters.AddWithValue("@start", selectStockDates.DateBegin);
		cmd.Parameters.AddWithValue("@end", selectStockDates.DateEnd);

		using (MySqlDataReader rdr = cmd.ExecuteReader())
		{
			WriteOffListStore.Clear();
			string doc = "Акт списания";
			while (rdr.Read())
			{
				ExpenseListStore.AppendValues(rdr.GetInt32("id"),
				                              String.Format("{0:d}", rdr.GetDateTime("date")),
				                              doc,
				                              rdr.GetString("user")
				                              );
			}
		}
		MainClass.StatusMessage("Ok");
		bool isSelect = treeviewWriteOff.Selection.CountSelectedRows() == 1;
		buttonEdit.Sensitive = isSelect;
		buttonDelete.Sensitive = isSelect;
	}

	private bool FilterTreeIncome (Gtk.TreeModel model, Gtk.TreeIter iter)
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

	protected void OnTreeviewIncomeRowActivated(object o, RowActivatedArgs args)
	{
		buttonEdit.Click();
	}

	protected void OnTreeviewIncomeCursorChanged(object sender, EventArgs e)
	{
		bool isSelect = treeviewIncome.Selection.CountSelectedRows() == 1;
		buttonEdit.Sensitive = isSelect;
		buttonDelete.Sensitive = isSelect;
	}

	protected void OnTreeviewExpenseCursorChanged(object sender, EventArgs e)
	{
		bool isSelect = treeviewExpense.Selection.CountSelectedRows() == 1;
		buttonEdit.Sensitive = isSelect;
		buttonDelete.Sensitive = isSelect;
	}

	protected void OnTreeviewExpenseRowActivated(object o, RowActivatedArgs args)
	{
		buttonEdit.Click();
	}

	protected void OnTreeviewWriteOffCursorChanged(object sender, EventArgs e)
	{
		bool isSelect = treeviewWriteOff.Selection.CountSelectedRows() == 1;
		buttonEdit.Sensitive = isSelect;
		buttonDelete.Sensitive = isSelect;
	}

	protected void OnTreeviewWriteOffRowActivated(object o, RowActivatedArgs args)
	{
		buttonEdit.Click();
	}
}


