using System;
using Gtk;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using QSProjectsLib;

namespace workwear
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ExpenseTable : Gtk.Bin
	{
		private int _WorkerId, _ExpenseDocId;
		private bool _CanSave = false;
		private Gtk.ListStore ItemsListStore, StockListStore;
		private string StockSearchText = "";
		TreeModelFilter StockFilter;
		private List<long> DeletedRowId = new List<long>();

		public event EventHandler CanSaveStateChanged;

		public int ExpenseDocId {
			get {return _ExpenseDocId;}
			set {_ExpenseDocId = value;
				FillIncomeDetails ();}
		}

		public int WorkerId {
			get {return _WorkerId;}
			set {_WorkerId = value;
				FillStockList ();}
		}

		public bool CanSave {
			get {return _CanSave;}
		}

		public ExpenseTable()
		{
			this.Build();

			//Создаем таблицу "материальных ценностей"
			ItemsListStore = new Gtk.ListStore (typeof (long), //0 this row id
			                                    typeof (long), //1 enter row id
			                                    typeof (int), //2 nomenclature id
			                                    typeof (string),//3 nomenclature name
			                                    typeof (int), //4 quantity
			                                    typeof (double), // 5 life
			                                    typeof (string) //6 units
			                                    );

			Gtk.TreeViewColumn QuantityColumn = new Gtk.TreeViewColumn ();
			QuantityColumn.Title = "Количество";
			Gtk.CellRendererSpin CellQuantity = new CellRendererSpin();
			CellQuantity.Editable = true;
			Adjustment adjQuantity = new Adjustment(0,0,10000,1,10,0);
			CellQuantity.Adjustment = adjQuantity;
			CellQuantity.Edited += OnQuantitySpinEdited;
			QuantityColumn.PackStart (CellQuantity, true);

			Gtk.TreeViewColumn LifeColumn = new Gtk.TreeViewColumn ();
			LifeColumn.Title = "% годности";
			Gtk.CellRendererSpin CellLife = new CellRendererSpin();
			CellLife.Editable = false;
			Adjustment adjLife = new Adjustment(0,0,100,1,10,0);
			CellLife.Adjustment = adjLife;
			CellLife.Edited += OnLifeSpinEdited;
			LifeColumn.PackStart (CellLife, true);

			treeviewItems.AppendColumn ("Наименование", new Gtk.CellRendererText (), "text", 3);
			treeviewItems.AppendColumn (QuantityColumn);
			QuantityColumn.AddAttribute (CellQuantity, "text" , 4);
			treeviewItems.AppendColumn (LifeColumn);
			LifeColumn.AddAttribute (CellLife, "text" , 5);

			LifeColumn.SetCellDataFunc (CellQuantity, RenderQuantityColumn);
			LifeColumn.SetCellDataFunc (CellLife, RenderLifeColumn);

			treeviewItems.Model = ItemsListStore;
			treeviewItems.ShowAll();

			buttonAdd.Sensitive = false;
			buttonDel.Sensitive = false;
		}

		private bool FillStockList()
		{
			if(_WorkerId <= 0)
				return false;
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
				cmd.Parameters.AddWithValue ("@current_expense", _ExpenseDocId);
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
				buttonAdd.Sensitive = true;
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка получения информации по складу!");
				return false;
			}
		}

		private void FillIncomeDetails()
		{
			MainClass.StatusMessage("Запрос деталей документа №" + _ExpenseDocId +"...");
			try
			{
				string sql = "SELECT stock_expense_detail.id, stock_expense_detail.stock_income_detail_id, " +
					"stock_expense_detail.nomenclature_id, nomenclature.name as nomenclature, stock_expense_detail.quantity, " +
					"stock_income_detail.life_percent, units.name as unit " +
						"FROM stock_expense_detail " +
					"LEFT JOIN nomenclature ON nomenclature.id = stock_expense_detail.nomenclature_id " +
					"LEFT JOIN units ON nomenclature.units_id = units.id " +
						"LEFT JOIN stock_income_detail ON stock_income_detail.id = stock_expense_detail.stock_income_detail_id " +
					"WHERE stock_expense_detail.stock_expense_id = @id";

				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@id", _ExpenseDocId);
				MySqlDataReader rdr = cmd.ExecuteReader();

				while (rdr.Read())
				{
					ItemsListStore.AppendValues(rdr.GetInt64("id"),
					                            rdr.GetInt64("stock_income_detail_id"),
					                              rdr.GetInt32 ("nomenclature_id"),
					                              rdr["nomenclature"].ToString(),
					                              rdr.GetInt32("quantity"),
					                              rdr.GetDouble("life_percent") * 100,
					                            rdr["unit"].ToString()
					                              );
				}
				rdr.Close();

				MainClass.StatusMessage("Ok");
				CalculateTotal();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка получения деталей расхода!");
			}
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

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			TreeIter iter;
			SelectStockItem WinSelect = new SelectStockItem(StockFilter);
			WinSelect.SearchTextChanged += OnSelectStockSearch;
			if (WinSelect.GetResult(out iter))
			{
				ItemsListStore.AppendValues(null,
				                            StockFilter.GetValue(iter, 0),
				                            StockFilter.GetValue(iter, 1),
				                            StockFilter.GetValue(iter, 2),
				                            StockFilter.GetValue(iter, 8), // from norm
				                            StockFilter.GetValue(iter, 7),
				                            StockFilter.GetValue(iter, 6));
				StockFilter.Refilter();
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
				ItemsListStore.SetValue (iter, 4, Quantity);
				CalculateTotal ();
			}
		}

		void OnLifeSpinEdited (object o, EditedArgs args)
		{
			TreeIter iter;
			if (!ItemsListStore.GetIterFromString (out iter, args.Path))
				return;
			double Life;
			if (double.TryParse (args.NewText, out Life)) 
			{
				ItemsListStore.SetValue (iter, 5, Life);
			}
		}

		private void RenderQuantityColumn (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			int Quantity = (int) model.GetValue (iter, 4);
			string Unit = (string) model.GetValue (iter, 6);
			(cell as Gtk.CellRendererText).Text = String.Format("{0} {1}", Quantity, Unit);
		}

		private void RenderLifeColumn (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			double Life = (double) model.GetValue (iter, 5);
			(cell as Gtk.CellRendererSpin).Text = String.Format("{0} %", Life);
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
				Count += (int) row[4];
			}
			labelSum.Text = String.Format ("Количество: {0}", Count);
			bool OldCanSave = _CanSave;
			_CanSave = (Count <= 0);
			if(_CanSave != OldCanSave && CanSaveStateChanged != null)
				CanSaveStateChanged(this, EventArgs.Empty);
		} 

		public bool SaveIncomeDetails(int ExpenseDoc_id, MySqlTransaction trans)
		{
			string sql;
			MySqlCommand cmd;
			try
			{
				foreach(object[] row in ItemsListStore)
				{
					if((long)row[0] > 0)
						sql = "UPDATE stock_expense_detail SET stock_expense_id = @stock_expense_id, nomenclature_id = @nomenclature_id, " +
							"quantity = @quantity, stock_income_detail_id = @stock_income_detail_id WHERE id = @id";
					else
						sql = "INSERT INTO stock_expense_detail(stock_expense_id, nomenclature_id, quantity, stock_income_detail_id)" +
							"VALUES (@stock_expense_id, @nomenclature_id, @quantity, @stock_income_detail_id)";

					cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
					cmd.Parameters.AddWithValue("@id", row[0]);
					cmd.Parameters.AddWithValue("@stock_expense_id", ExpenseDoc_id);
					cmd.Parameters.AddWithValue("@nomenclature_id", row[2]);
					cmd.Parameters.AddWithValue("@quantity", row[4]);
					if((long)row[1] > 0)
						cmd.Parameters.AddWithValue("@stock_income_detail_id", row[1]);
					else
						cmd.Parameters.AddWithValue("@stock_income_detail_id", DBNull.Value);

					cmd.ExecuteNonQuery ();
				}

				//Удаляем удаленные строки из базы данных
				sql = "DELETE FROM stock_expense_detail WHERE id = @id";
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
				MainClass.StatusMessage("Ошибка записи строк расхода!");
				return false;
			}
		}

		protected void OnSelectStockSearch(object sender, EventArgs e)
		{
			StockSearchText = ((SelectStockItem)sender).SearchText;
			StockFilter.Refilter();
		}

	}
}

