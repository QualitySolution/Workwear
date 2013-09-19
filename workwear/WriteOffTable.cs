using System;
using Gtk;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using QSProjectsLib;

namespace workwear
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WriteOffTable : Gtk.Bin
	{
		private int _CurWorkerId, _WriteOffDocId;
		private bool _CanSave = false;
		private Gtk.ListStore ItemsListStore, StockListStore, CardRowsListStore;
		private string StockSearchText = "";
		private int CardRowsWorkerId;
		TreeModelFilter StockFilter, CardRowsFilter;
		private List<long> DeletedRowId = new List<long>();

		public event EventHandler CanSaveStateChanged;

		public int WriteOffDocId {
			get {return _WriteOffDocId;}
			set {_WriteOffDocId = value;
				FillWriteOffDetails ();}
		}

		public int CurWorkerId {
			get {return _CurWorkerId;}
			set {_CurWorkerId = value;
				}
		}

		public bool CanSave {
			get {return _CanSave;}
		}

		public WriteOffTable()
		{
			this.Build();

			//Создаем таблицу "материальных ценностей"
			ItemsListStore = new Gtk.ListStore (typeof (long), //0 this row id
			                                    typeof (long), //1 enter store row id
			                                    typeof (long), //2 enter card row id
			                                    typeof (int), //3 nomenclature id
			                                    typeof (string),//4 nomenclature name
			                                    typeof (string), //5 write off from
			                                    typeof (int), //6 quantity
			                                    typeof (string) //7 units
			                                    );

			Gtk.TreeViewColumn QuantityColumn = new Gtk.TreeViewColumn ();
			QuantityColumn.Title = "Количество";
			Gtk.CellRendererSpin CellQuantity = new CellRendererSpin();
			CellQuantity.Editable = true;
			Adjustment adjQuantity = new Adjustment(0,0,10000,1,10,0);
			CellQuantity.Adjustment = adjQuantity;
			CellQuantity.Edited += OnQuantitySpinEdited;
			QuantityColumn.PackStart (CellQuantity, true);

			treeviewItems.AppendColumn ("Наименование", new Gtk.CellRendererText (), "text", 4);
			treeviewItems.AppendColumn ("Списано из", new Gtk.CellRendererText (), "text", 5);
			treeviewItems.AppendColumn (QuantityColumn);

			QuantityColumn.SetCellDataFunc (CellQuantity, RenderQuantityColumn);

			treeviewItems.Model = ItemsListStore;
			treeviewItems.ShowAll();

			FillStockList();
			FillCardRowsList();

			buttonDel.Sensitive = false;
		}

		private bool FillStockList()
		{
			StockListStore = new ListStore(typeof(long), // 0 - ID income row
			                                typeof(int), //1 - nomenclature id
			                                typeof(string), //2 - nomenclature name
			                                typeof(string), //3 - size
			                                typeof(string), // 4 growth
			                                typeof(int), //5 - quantity
			                                typeof(string), // 6 - units
			                                typeof(double), // 7 - today % of life
			                                typeof(int)); // 8 - norm_quantity
			StockFilter = new TreeModelFilter( StockListStore, null);
			StockFilter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FilterTreeStock);

			MainClass.StatusMessage("Запрос спецодежды на складе...");
			try
			{
				string sql = "SELECT stock_income_detail.id, stock_income_detail.nomenclature_id, stock_income_detail.quantity, " +
					"nomenclature.name as nomenclature, stock_income_detail.life_percent, spent.count, item_types.name as type, " +
					"nomenclature.size, nomenclature.growth, units.name as unit, item_types.norm_quantity " +
					"FROM stock_income_detail " +
					"LEFT JOIN (SELECT id, SUM(count) as count FROM " +
					"(SELECT stock_expense_detail.stock_income_detail_id as id, stock_expense_detail.quantity as count FROM stock_expense_detail WHERE stock_expense_id <> @current_expense " +
					"UNION ALL " +
					"SELECT stock_write_off_detail.stock_income_detail_id as id, stock_write_off_detail.quantity as count FROM stock_write_off_detail WHERE stock_income_detail_id IS NOT NULL) as table1 " +
					"GROUP BY id) as spent ON spent.id = stock_income_detail.id " +
					"LEFT JOIN nomenclature ON nomenclature.id = stock_income_detail.nomenclature_id " +
					"LEFT JOIN units ON units.id = nomenclature.units_id " +
					"LEFT JOIN item_types ON nomenclature.type_id = item_types.id " +
					"WHERE spent.count IS NULL OR spent.count < stock_income_detail.quantity";
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue ("@current_expense", _WriteOffDocId);
				using(MySqlDataReader rdr = cmd.ExecuteReader())
				{
					while (rdr.Read())
					{
						int Quantity, Norm_Quantity;
						if(rdr["count"] == DBNull.Value)
						 	Quantity = rdr.GetInt32("quantity");
						else
							Quantity = rdr.GetInt32("quantity") - rdr.GetInt32("count");
						if(rdr["norm_quantity"] != DBNull.Value)
							Norm_Quantity = rdr.GetInt32("norm_quantity");
						else
							Norm_Quantity = 0;

						StockListStore.AppendValues(rdr.GetInt64("id"),
						                            rdr.GetInt32("nomenclature_id"),
						                            rdr.GetString ("nomenclature"),
						                            rdr["size"].ToString(),
						                            rdr["growth"].ToString(),
						                            Quantity,
						                            rdr["unit"],
						                            rdr.GetDouble("life_percent") * 100.0,
						                            Norm_Quantity);
					}
				}
				MainClass.StatusMessage("Ok");
				buttonAddStore.Sensitive = true;
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка получения информации по складу!");
				throw;
			}
		}

		private void FillWriteOffDetails()
		{
			MainClass.StatusMessage("Запрос деталей документа №" + _WriteOffDocId +"...");
			try
			{
				string sql = "SELECT stock_write_off_detail.id, stock_write_off_detail.stock_income_detail_id, stock_write_off_detail.stock_expense_detail_id," +
						"stock_write_off_detail.nomenclature_id, nomenclature.name as nomenclature, stock_write_off_detail.quantity, " +
						"wear_cards.last_name, wear_cards.first_name, wear_cards.patronymic_name, units.name as unit " +
						"FROM stock_write_off_detail " +
						"LEFT JOIN nomenclature ON nomenclature.id = stock_write_off_detail.nomenclature_id " +
						"LEFT JOIN units ON nomenclature.units_id = units.id " +
						"LEFT JOIN stock_expense_detail ON stock_write_off_detail.stock_expense_detail_id = stock_expense_detail.id " +
						"LEFT JOIN stock_expense ON stock_expense.id = stock_expense_detail.stock_expense_id " +
						"LEFT JOIN wear_cards ON stock_expense.wear_card_id = wear_cards.id " +
						"WHERE stock_write_off_detail.stock_write_off_id = @id";

				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@id", _WriteOffDocId);
				using( MySqlDataReader rdr = cmd.ExecuteReader())
				{
					while (rdr.Read())
					{
						string FromName;
						if(rdr["stock_income_detail_id"] != DBNull.Value)
							FromName = "склад";
						else if (rdr["stock_expense_detail_id"] != DBNull.Value)
							FromName = String.Format("{0} {1}. {2}.", rdr["last_name"].ToString(), rdr["first_name"].ToString()[0], rdr["patronymic_name"].ToString()[0]);
						else 
							FromName = String.Empty;
						ItemsListStore.AppendValues(rdr.GetInt64("id"),
						                            DBWorks.GetLong(rdr, "stock_income_detail_id", -1),
						                            DBWorks.GetLong(rdr, "stock_expense_detail_id", -1),
						                            rdr.GetInt32 ("nomenclature_id"),
						                            rdr["nomenclature"].ToString(),
						                            FromName,
						                            rdr.GetInt32("quantity"),
						                            rdr["unit"].ToString()
						                            );
					}
				}
				MainClass.StatusMessage("Ok");
				CalculateTotal();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка получения деталей списания!");
				throw;
			}
		}

		private bool FillCardRowsList()
		{
			CardRowsListStore = new ListStore(typeof(long), // 0 - ID expense row
			                                  typeof(int), //1 - nomenclature id
			                                  typeof(string), //2 - nomenclature name
			                                  typeof(string), //3 - date
			                                  typeof(int), //4 - quantity
			                                  typeof(string), // 5 - % of life
			                                  typeof(string), // 6 - today % of life
			                                  typeof(double), // 7 - today % of life
			                                  typeof(int), // 8 - worker id
			                                  typeof(string), //9 -worker name
			                                  typeof(string) //10 - units
			                                  );
			CardRowsFilter = new TreeModelFilter( CardRowsListStore, null);
			CardRowsFilter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FilterTreeCardRows);

			MainClass.StatusMessage("Запрос спецодежды по работнику...");
			try
			{
				string sql = "SELECT stock_expense_detail.id, stock_expense_detail.nomenclature_id, stock_expense_detail.quantity, " +
					"nomenclature.name, stock_expense.date, stock_income_detail.life_percent, spent.count, item_types.norm_life, " +
						"wear_cards.last_name, wear_cards.first_name, wear_cards.patronymic_name, units.name as unit, stock_expense.wear_card_id " +
						"FROM stock_expense_detail \n" +
						"LEFT JOIN (\nSELECT id, SUM(count) as count FROM \n" +
						"(SELECT stock_income_detail.stock_expense_detail_id as id, stock_income_detail.quantity as count FROM stock_income_detail WHERE stock_expense_detail_id IS NOT NULL " +
						"UNION ALL\n" +
						"SELECT stock_write_off_detail.stock_expense_detail_id as id, stock_write_off_detail.quantity as count FROM stock_write_off_detail WHERE stock_expense_detail_id IS NOT NULL AND stock_write_off_id <> @current_write_off) as table1 " +
						"GROUP BY id) as spent ON spent.id = stock_expense_detail.id \n" +
						"LEFT JOIN nomenclature ON nomenclature.id = stock_expense_detail.nomenclature_id " +
						"LEFT JOIN item_types ON nomenclature.type_id = item_types.id " +
						"LEFT JOIN stock_expense ON stock_expense.id = stock_expense_detail.stock_expense_id \n" +
						"LEFT JOIN stock_income_detail ON stock_income_detail.id = stock_expense_detail.stock_income_detail_id " +
						"LEFT JOIN wear_cards ON stock_expense.wear_card_id = wear_cards.id " +
						"LEFT JOIN units ON units.id = nomenclature.units_id " +
						"WHERE spent.count IS NULL OR spent.count < stock_expense_detail.quantity ";
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue ("@current_write_off", _WriteOffDocId);
				using( MySqlDataReader rdr = cmd.ExecuteReader())
				{
					while (rdr.Read())
					{
						int Quantity, Life;
						if(rdr["count"] == DBNull.Value)
							Quantity = rdr.GetInt32("quantity");
						else
							Quantity = rdr.GetInt32("quantity") - rdr.GetInt32("count");

						double MonthUsing = ((DateTime.Today - rdr.GetDateTime ("date")).TotalDays / 365) * 12;
						if(rdr["norm_life"] != DBNull.Value)
							Life = (int) (rdr.GetDecimal("life_percent") * 100) - (int) (MonthUsing / rdr.GetInt32("norm_life") * 100);
						else
							Life = (int) (rdr.GetDecimal("life_percent") * 100);
						if(Life < 0) 
							Life = 0;
						CardRowsListStore.AppendValues(rdr.GetInt64("id"),
						                               rdr.GetInt32("nomenclature_id"),
						                               rdr.GetString ("name"),
						                               String.Format ("{0:d}", rdr.GetDateTime ("date")),
						                               Quantity,
						                               String.Format ("{0} %", rdr.GetDecimal("life_percent") * 100),
						                               String.Format ("{0} %", Life),
						                               (double) Life,
						                               rdr.GetInt32("wear_card_id"),
						                               String.Format("{0} {1}. {2}.", rdr["last_name"].ToString(), rdr["first_name"].ToString()[0], rdr["patronymic_name"].ToString()[0]),
						                               rdr["unit"].ToString()
						                               );
					}
				}
				MainClass.StatusMessage("Ok");
				buttonAddWorker.Sensitive = true;
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка получения о выданной одежде!");
				throw;
			}
		}

		private bool FilterTreeCardRows (Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			if(model.GetValue (iter, 0) == null)
				return false;

			if (CardRowsWorkerId > 0)
			{
				int Id = (int) model.GetValue(iter, 8);
				if (CardRowsWorkerId != Id )
					return false;
			}

			long rowid = (long) model.GetValue (iter, 0);

			foreach (object[] row in ItemsListStore)
			{
				if((long)row[1] == rowid)
					return false;
			}

			return true;
		}

		private bool FilterTreeStock (Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			if(model.GetValue (iter, 2) == null)
				return false;

			if (StockSearchText != "")
			{
				string Str = model.GetValue(iter, 2).ToString();
				if (!(Str.IndexOf(StockSearchText, StringComparison.CurrentCultureIgnoreCase) > -1))
					return false;
			}

			long rowid = (long) model.GetValue (iter, 0);

			foreach (object[] row in ItemsListStore)
			{
				if((long)row[1] == rowid)
					return false;
			}

			return true;
		}

		protected void OnButtonAddStoreClicked (object sender, EventArgs e)
		{
			TreeIter iter;
			SelectStockItem WinSelect = new SelectStockItem(StockFilter);
			WinSelect.SearchTextChanged += OnSelectStockSearch;
			if (WinSelect.GetResult(out iter))
			{
				ItemsListStore.AppendValues(-1,
				                            StockFilter.GetValue(iter, 0),
				                            -1,
				                            StockFilter.GetValue(iter, 1),
				                            StockFilter.GetValue(iter, 2),
				                            "склад",
				                            StockFilter.GetValue(iter, 5),
				                            StockFilter.GetValue(iter, 6));
				StockFilter.Refilter();
				CalculateTotal();
			}
		}

		void OnQuantitySpinEdited (object o, EditedArgs args)
		{
			TreeIter iter;
			if (!ItemsListStore.GetIterFromString (out iter, args.Path))
				return;
			int Quantity;
			if (int.TryParse (args.NewText, out Quantity)) 
			{
				ItemsListStore.SetValue (iter, 6, Quantity);
				CalculateTotal ();
			}
		}

		private void RenderQuantityColumn (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			int Quantity = (int) model.GetValue (iter, 6);
			string Unit = (string) model.GetValue (iter, 7);
			(cell as Gtk.CellRendererText).Text = String.Format("{0} {1}", Quantity, Unit);
		}

		protected void OnButtonDelClicked (object sender, EventArgs e)
		{
			TreeIter iter;
			treeviewItems.Selection.GetSelected (out iter);
			if((long)ItemsListStore.GetValue(iter, 0) > 0)
				DeletedRowId.Add ((long)ItemsListStore.GetValue(iter, 0));
			ItemsListStore.Remove(ref iter);
			StockFilter.Refilter ();
			OnTreeviewItemsCursorChanged (null, null);
			CalculateTotal();
		}

		protected void OnTreeviewItemsCursorChanged (object sender, EventArgs e)
		{
			bool isSelect = treeviewItems.Selection.CountSelectedRows() == 1;
			buttonDel.Sensitive = isSelect;
		}

		private void CalculateTotal()
		{
			int Count = 0;
			foreach (object[] row in ItemsListStore)
			{
				Count += (int) row[6];
			}
			labelSum.Text = String.Format ("Количество: {0}", Count);
			bool OldCanSave = _CanSave;
			_CanSave = (Count > 0);
			if(_CanSave != OldCanSave && CanSaveStateChanged != null)
				CanSaveStateChanged(this, EventArgs.Empty);
		} 

		public bool SaveWriteOffDetails(int WriteOffDoc_id, MySqlTransaction trans)
		{
			string sql;
			MySqlCommand cmd;
			try
			{
				foreach(object[] row in ItemsListStore)
				{
					if((long)row[0] > 0)
						sql = "UPDATE stock_write_off_detail SET stock_write_off_id = @stock_write_off_id, nomenclature_id = @nomenclature_id, " +
							"quantity = @quantity, stock_income_detail_id = @stock_income_detail_id, stock_expense_detail_id = @stock_expense_detail_id WHERE id = @id";
					else
						sql = "INSERT INTO stock_write_off_detail(stock_write_off_id, nomenclature_id, quantity, stock_income_detail_id, stock_expense_detail_id)" +
							"VALUES (@stock_write_off_id, @nomenclature_id, @quantity, @stock_income_detail_id, @stock_expense_detail_id)";

					cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
					cmd.Parameters.AddWithValue("@id", row[0]);
					cmd.Parameters.AddWithValue("@stock_write_off_id", WriteOffDoc_id);
					cmd.Parameters.AddWithValue("@nomenclature_id", row[3]);
					cmd.Parameters.AddWithValue("@quantity", row[6]);
					if((long)row[1] > 0)
						cmd.Parameters.AddWithValue("@stock_income_detail_id", row[1]);
					else
						cmd.Parameters.AddWithValue("@stock_income_detail_id", DBNull.Value);
					if((long)row[2] > 0)
						cmd.Parameters.AddWithValue("@stock_expense_detail_id", row[2]);
					else
						cmd.Parameters.AddWithValue("@stock_expense_detail_id", DBNull.Value);

					cmd.ExecuteNonQuery ();
				}

				//Удаляем удаленные строки из базы данных
				sql = "DELETE FROM stock_write_off_detail WHERE id = @id";
				foreach( long id in DeletedRowId)
				{
					cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
					cmd.Parameters.AddWithValue("@id", id);
					cmd.ExecuteNonQuery();
				}
				DeletedRowId.Clear ();
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка записи строк списания!");
				throw;
			}
		}

		protected void OnSelectStockSearch(object sender, EventArgs e)
		{
			StockSearchText = ((SelectStockItem)sender).SearchText;
			StockFilter.Refilter();
		}

		protected void OnSelectCardRowsSearch(object sender, EventArgs e)
		{
			CardRowsWorkerId = ((SelectWearCardRow)sender).WorkerId;
			CardRowsFilter.Refilter();
		}

		protected void OnButtonAddWorkerClicked(object sender, EventArgs e)
		{
			TreeIter iter;

			SelectWearCardRow WinSelect = new SelectWearCardRow(CardRowsFilter);
			WinSelect.WorkerComboActive = true;
			if (CurWorkerId > 0)
				WinSelect.WorkerId = CurWorkerId;
			WinSelect.WorkerIdChanged += OnSelectCardRowsSearch;
			if (WinSelect.GetResult(out iter))
			{
				ItemsListStore.AppendValues(-1,
				                            -1,
				                            CardRowsFilter.GetValue(iter, 0),
				                            CardRowsFilter.GetValue(iter, 1),
				                            CardRowsFilter.GetValue(iter, 2),
				                            CardRowsFilter.GetValue(iter, 9),
				                            CardRowsFilter.GetValue(iter, 4),
				                            CardRowsFilter.GetValue(iter, 10));
				CardRowsFilter.Refilter();
				CalculateTotal();
			}

		}
	}
}

