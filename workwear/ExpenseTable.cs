using System;
using System.Collections.Generic;
using Gtk;
using MySql.Data.MySqlClient;
using NLog;
using QSProjectsLib;

namespace workwear
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ExpenseTable : Gtk.Bin
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private int _WorkerId, _ObjectId, _ExpenseDocId;
		private ExpenseDoc.Operations _Operation;
		private bool _CanSave = false;
		private Gtk.ListStore ItemsListStore, StockListStore;
		private string StockSearchText = "";
		TreeModelFilter StockFilter;
		private List<long> DeletedRowId = new List<long>();
		private TreeModel PlacementList;
		private TreeViewColumn PlacementColumn;
		private Gtk.CellRendererCombo CellPlacement;

		public event EventHandler CanSaveStateChanged;

		public int ExpenseDocId {
			get {return _ExpenseDocId;}
			set {_ExpenseDocId = value;
				FillExpenseDetails ();}
		}

		public int WorkerId {
			get {return _WorkerId;}
			set {_WorkerId = value;
				FillStockList ();}
		}

		public int ObjectId {
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

		public ExpenseDoc.Operations Operation {
			get {return _Operation;}
			set {
				buttonAdd.Sensitive = (WorkerId > 0  && value == ExpenseDoc.Operations.Employee) || 
					(ObjectId > 0 && value == ExpenseDoc.Operations.Object);
				PlacementColumn.Visible = value == ExpenseDoc.Operations.Object;

				if (_Operation == value)
					return;
				_Operation = value;
				ItemsListStore.Clear();
			}
		}

		public bool CanSave {
			get {return _CanSave;}
		}

		public ExpenseTable()
		{
			this.Build();

			//Создаем таблицу "материальных ценностей"
			ItemsListStore = new Gtk.ListStore (typeof (long), 	//0 this row id
			                                    typeof (long), 	//1 enter row id
			                                    typeof (int), 	//2 nomenclature id
			                                    typeof (string),//3 nomenclature name
			                                    typeof (int), 	//4 quantity
			                                    typeof (double), //5 life
			                                    typeof (string), //6 units
			                                    typeof (int), 	//7 - Placement id
			                                    typeof (string) //8 - Placement name
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

			PlacementColumn = new Gtk.TreeViewColumn ();
			PlacementColumn.Title = "Расположение";
			CellPlacement = new CellRendererCombo();
			CellPlacement.TextColumn = 0;
			CellPlacement.Editable = true;
			CellPlacement.HasEntry = false;
			CellPlacement.Edited += OnPlacementComboEdited;
			PlacementColumn.PackStart(CellPlacement, true);

			treeviewItems.AppendColumn ("Наименование", new Gtk.CellRendererText (), "text", 3);
			treeviewItems.AppendColumn (QuantityColumn);
			treeviewItems.AppendColumn (LifeColumn);
			treeviewItems.AppendColumn (PlacementColumn);

			PlacementColumn.AddAttribute (CellPlacement, "text", 8);

			QuantityColumn.SetCellDataFunc (CellQuantity, RenderQuantityColumn);
			LifeColumn.SetCellDataFunc (CellLife, RenderLifeColumn);

			treeviewItems.Model = ItemsListStore;
			treeviewItems.ShowAll();

			buttonAdd.Sensitive = false;
			buttonDel.Sensitive = false;
		}

		private bool FillStockList()
		{
			if((Operation == ExpenseDoc.Operations.Employee && _WorkerId <= 0) 
			   || (Operation == ExpenseDoc.Operations.Object && _ObjectId <= 0))
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

			QSMain.CheckConnectionAlive ();
			logger.Info("Запрос спецодежды на складе...");
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
					"LEFT JOIN item_types ON nomenclature.type_id = item_types.id " +
					"LEFT JOIN units ON units.id = item_types.units_id " +
					"WHERE (spent.count IS NULL OR spent.count < stock_income_detail.quantity) ";
				if(Operation == ExpenseDoc.Operations.Employee)
					sql += "AND item_types.category = 'wear' ";
				else
					sql += "AND item_types.category = 'property' ";
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
				logger.Info("Ok");
				buttonAdd.Sensitive = true;
				return true;
			}
			catch (Exception ex)
			{
				logger.Warn(ex, "Ошибка получения информации по складу!");
				throw;
			}
		}

		private void FillExpenseDetails()
		{
			QSMain.CheckConnectionAlive ();
			logger.Info("Запрос деталей документа №" + _ExpenseDocId +"...");
			try
			{
				string sql = "SELECT stock_expense_detail.id, stock_expense_detail.stock_income_detail_id, " +
					"stock_expense_detail.nomenclature_id, nomenclature.name as nomenclature, stock_expense_detail.quantity, " +
					"stock_income_detail.life_percent, units.name as unit, stock_expense_detail.object_place_id, object_places.name as object_place " +
					"FROM stock_expense_detail " +
					"LEFT JOIN nomenclature ON nomenclature.id = stock_expense_detail.nomenclature_id " +
					"LEFT JOIN item_types ON item_types.id = nomenclature.type_id " +
					"LEFT JOIN units ON item_types.units_id = units.id " +
					"LEFT JOIN stock_income_detail ON stock_income_detail.id = stock_expense_detail.stock_income_detail_id " +
					"LEFT JOIN object_places ON object_places.id = stock_expense_detail.object_place_id " +
					"WHERE stock_expense_detail.stock_expense_id = @id";

				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@id", _ExpenseDocId);
				using( MySqlDataReader rdr = cmd.ExecuteReader())
				{
					while (rdr.Read())
					{
						ItemsListStore.AppendValues(rdr.GetInt64("id"),
						                            rdr.GetInt64("stock_income_detail_id"),
						                              rdr.GetInt32 ("nomenclature_id"),
						                              rdr["nomenclature"].ToString(),
						                              rdr.GetInt32("quantity"),
						                              rdr.GetDouble("life_percent") * 100,
						                            rdr["unit"].ToString(),
						                            DBWorks.GetInt(rdr, "object_place_id", -1),
						                            DBWorks.GetString(rdr, "object_place", "нет")
						                              );
					}
				}
				logger.Info("Ok");
				CalculateTotal();
			}
			catch (Exception ex)
			{
				logger.Warn(ex, "Ошибка получения деталей расхода!");
				throw;
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
			TreeIter[] iters;
			SelectStockItem WinSelect = new SelectStockItem(StockFilter);
			WinSelect.SearchTextChanged += OnSelectStockSearch;
			if (WinSelect.GetResult(out iters))
			{
				foreach (var iter in iters)
				{
					ItemsListStore.AppendValues(null,
						StockFilter.GetValue(iter, 0),
						StockFilter.GetValue(iter, 1),
						StockFilter.GetValue(iter, 2),
						StockFilter.GetValue(iter, 8), // from norm
						StockFilter.GetValue(iter, 7),
						StockFilter.GetValue(iter, 6));
				}
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
				Count += (int) row[4];
			}
			labelSum.Text = String.Format ("Количество: {0}", Count);
			bool OldCanSave = _CanSave;
			_CanSave = (Count > 0);
			if(_CanSave != OldCanSave && CanSaveStateChanged != null)
				CanSaveStateChanged(this, EventArgs.Empty);
		} 

		public bool SaveExpenseDetails(int ExpenseDoc_id, MySqlTransaction trans)
		{
			string sql;
			MySqlCommand cmd;
			try
			{
				foreach(object[] row in ItemsListStore)
				{
					if((long)row[0] > 0)
						sql = "UPDATE stock_expense_detail SET stock_expense_id = @stock_expense_id, nomenclature_id = @nomenclature_id, " +
							"quantity = @quantity, stock_income_detail_id = @stock_income_detail_id, object_place_id = @object_place_id WHERE id = @id";
					else
						sql = "INSERT INTO stock_expense_detail(stock_expense_id, nomenclature_id, quantity, stock_income_detail_id, object_place_id)" +
							"VALUES (@stock_expense_id, @nomenclature_id, @quantity, @stock_income_detail_id, @object_place_id)";

					cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
					cmd.Parameters.AddWithValue("@id", row[0]);
					cmd.Parameters.AddWithValue("@stock_expense_id", ExpenseDoc_id);
					cmd.Parameters.AddWithValue("@nomenclature_id", row[2]);
					cmd.Parameters.AddWithValue("@quantity", row[4]);
					cmd.Parameters.AddWithValue("@stock_income_detail_id", DBWorks.ValueOrNull((long)row[1] > 0, row[1]) );
					cmd.Parameters.AddWithValue("@object_place_id", DBWorks.ValueOrNull((int)row[7] > 0, row[7]) );
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
				logger.Warn(ex, "Ошибка записи строк расхода!");
				throw;
			}
		}

		protected void OnSelectStockSearch(object sender, EventArgs e)
		{
			StockSearchText = ((SelectStockItem)sender).SearchText;
			StockFilter.Refilter();
		}

		void OnPlacementComboEdited (object o, EditedArgs args)
		{
			TreeIter iter;
			if (!ItemsListStore.GetIterFromString (out iter, args.Path))
				return;
			if(args.NewText == null)
			{
				Console.WriteLine("newtext is empty");
				return;
			}
			ItemsListStore.SetValue(iter, 8, args.NewText);
			TreeIter PlacementIter;
			ListStoreWorks.SearchListStore((ListStore)PlacementList, args.NewText, out PlacementIter);
			ItemsListStore.SetValue(iter, 7, PlacementList.GetValue(PlacementIter, 1));
		}


	}
}

