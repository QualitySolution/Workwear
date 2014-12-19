using System;
using System.Collections.Generic;
using Gtk;
using MySql.Data.MySqlClient;
using NLog;
using QSProjectsLib;

namespace workwear
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class IncomeTable : Gtk.Bin
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private int _WorkerId, _ObjectId, _IncomeDocId;
		private IncomeDoc.Operations _Operation;
		private bool _CanSave = false;
		private Gtk.ListStore ItemsListStore, CardRowsListStore, PropertyListStore;
		TreeModelFilter CardRowsFilter, PropertyFilter;
		private List<long> DeletedRowId = new List<long>();

		public event EventHandler CanSaveStateChanged;

		public int IncomeDocId {
			get {return _IncomeDocId;}
			set {_IncomeDocId = value;
				FillIncomeDetails ();}
		}

		public int WorkerId {
			get {return _WorkerId;}
			set {_WorkerId = value;
				FillCardRowsList ();}
		}

		public int ObjectId {
			get {return _ObjectId;}
			set {_ObjectId = value;

				FillObjectPropertyList ();
			}
		}

		public IncomeDoc.Operations Operation {
			get {return _Operation;}
			set {
				buttonAdd.Sensitive = (WorkerId > 0  && value == IncomeDoc.Operations.Return) || 
					(ObjectId > 0 && value == IncomeDoc.Operations.Object) || value == IncomeDoc.Operations.Enter;

				if (_Operation == value)
					return;
				_Operation = value;
				ItemsListStore.Clear();
			}
		}

		public bool CanSave {
			get {return _CanSave;}
		}

		public IncomeTable()
		{
			this.Build();

			//Создаем таблицу "материальных ценностей"
			ItemsListStore = new Gtk.ListStore (typeof (long), //0 this row id
			                                    typeof (long), //1 expense row id
			                                    typeof (int), //2 nomenclature id
			                                    typeof (string),//3 nomenclature name
			                                    typeof (int), //4 quantity
			                                    typeof (double), // 5 life
			                                    typeof (double), // 6 cost
			                                    typeof (string) // 7 units
			                                    );

			Gtk.TreeViewColumn QuantityColumn = new Gtk.TreeViewColumn ();
			QuantityColumn.Title = "Количество";
			Gtk.CellRendererSpin CellQuantity = new CellRendererSpin();
			CellQuantity.Editable = true;
			Adjustment adjQuantity = new Adjustment(0,0,10000,1,10,0);
			CellQuantity.Adjustment = adjQuantity;
			CellQuantity.Edited += OnQuantitySpinEdited;
			QuantityColumn.PackStart (CellQuantity, true);

			Gtk.TreeViewColumn CostColumn = new Gtk.TreeViewColumn ();
			CostColumn.Title = "Стоимость";
			Gtk.CellRendererSpin CellCost = new CellRendererSpin();
			CellCost.Editable = true;
			CellCost.Digits = 2;
			Adjustment adjCost = new Adjustment(0,0,100000000,100,1000,0);
			CellCost.Adjustment = adjCost;
			CellCost.Edited += OnCostSpinEdited;
			CostColumn.PackStart (CellCost, true);

			Gtk.TreeViewColumn LifeColumn = new Gtk.TreeViewColumn ();
			LifeColumn.Title = "% годности";
			Gtk.CellRendererSpin CellLife = new CellRendererSpin();
			CellLife.Editable = true;
			Adjustment adjLife = new Adjustment(0,0,100,1,10,0);
			CellLife.Adjustment = adjLife;
			CellLife.Edited += OnLifeSpinEdited;
			LifeColumn.PackStart (CellLife, true);

			treeviewItems.AppendColumn ("Наименование", new Gtk.CellRendererText (), "text", 3);
			treeviewItems.AppendColumn (QuantityColumn);
			treeviewItems.AppendColumn (LifeColumn);
			treeviewItems.AppendColumn (CostColumn);

			QuantityColumn.SetCellDataFunc (CellQuantity, RenderQuantityColumn);
			LifeColumn.SetCellDataFunc (CellLife, RenderLifeColumn);
			CostColumn.SetCellDataFunc (CellCost, RenderCostColumn);

			treeviewItems.Model = ItemsListStore;
			treeviewItems.ShowAll();

			buttonAdd.Sensitive = false;
			buttonDel.Sensitive = false;
		}

		private bool FillCardRowsList()
		{
			if(_WorkerId <= 0)
				return false;
			CardRowsListStore = new ListStore(typeof(long), // 0 - ID expense row
			                                   typeof(int), //1 - nomenclature id
			                                   typeof(string), //2 - nomenclature name
			                                   typeof(string), //3 - date
			                                   typeof(int), //4 - quantity
			                                   typeof(string), // 5 - % of life
			                                   typeof(string), // 6 - today % of life
			                                   typeof(double), // 7 - today % of life
			                                   typeof(int), // 8 - worker id
			                                  typeof(string), // 9 - worker name
			                                  typeof(string) // 10 - units
			);
			CardRowsFilter = new TreeModelFilter( CardRowsListStore, null);
			CardRowsFilter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FilterTreeCardRows);

			logger.Info("Запрос спецодежды по работнику...");
			try
			{
				string sql = "SELECT stock_expense_detail.id, stock_expense_detail.nomenclature_id, stock_expense_detail.quantity,\n" +
					"nomenclature.name, stock_expense.date, stock_income_detail.life_percent, spent.count, item_types.norm_life, units.name as unit " +
					"FROM stock_expense_detail \n" +
					"LEFT JOIN (\nSELECT id, SUM(count) as count FROM \n" +
						"(SELECT stock_income_detail.stock_expense_detail_id as id, stock_income_detail.quantity as count FROM stock_income_detail WHERE stock_expense_detail_id IS NOT NULL AND stock_income_id <> @current_income\n" +
					"UNION ALL\n" +
					"SELECT stock_write_off_detail.stock_expense_detail_id as id, stock_write_off_detail.quantity as count FROM stock_write_off_detail WHERE stock_expense_detail_id IS NOT NULL) as table1\n" +
					"GROUP BY id) as spent ON spent.id = stock_expense_detail.id \n" +
					"LEFT JOIN nomenclature ON nomenclature.id = stock_expense_detail.nomenclature_id " +
					"LEFT JOIN item_types ON nomenclature.type_id = item_types.id " +
					"LEFT JOIN stock_expense ON stock_expense.id = stock_expense_detail.stock_expense_id \n" +
					"LEFT JOIN stock_income_detail ON stock_income_detail.id = stock_expense_detail.stock_income_detail_id " +
					"LEFT JOIN units ON nomenclature.units_id = units.id " +
					"WHERE stock_expense.wear_card_id = @id AND (spent.count IS NULL OR spent.count < stock_expense_detail.quantity )";
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue ("@id", _WorkerId);
				cmd.Parameters.AddWithValue ("@current_income", _IncomeDocId);
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
						                               WorkerId,
						                               String.Empty,
						                               rdr["unit"].ToString()
						                               );
					}
				}
				logger.Info("Ok");
				buttonAdd.Sensitive = true;
				return true;
			}
			catch (Exception ex)
			{
				logger.WarnException("Ошибка получения о выданной одежде!", ex);
				throw;
			}
		}

		private bool FillObjectPropertyList()
		{
			if(_ObjectId <= 0)
				return false;
			PropertyListStore = new ListStore(typeof(long), // 0 - ID expense row
			                                  typeof(int), //1 - nomenclature id
			                                  typeof(string), //2 - nomenclature name
			                                  typeof(string), //3 - date
			                                  typeof(int), //4 - quantity
			                                  typeof(string), // 5 - % of life
			                                  typeof(string), // 6 - today % of life
			                                  typeof(double), // 7 - today % of life
			                                  typeof(int), // 8 - object id
			                                  typeof(string), // 9 - object name
			                                  typeof(string) // 10 - units
			                                  );
			PropertyFilter = new TreeModelFilter( PropertyListStore, null);
			PropertyFilter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FilterTreeProperty);

			logger.Info("Запрос имужества по объекту...");
			try
			{
				string sql = "SELECT stock_expense_detail.id, stock_expense_detail.nomenclature_id, stock_expense_detail.quantity,\n" +
					"nomenclature.name, stock_expense.date, stock_income_detail.life_percent, spent.count, item_types.norm_life, units.name as unit " +
						"FROM stock_expense_detail \n" +
						"LEFT JOIN (\nSELECT id, SUM(count) as count FROM \n" +
						"(SELECT stock_income_detail.stock_expense_detail_id as id, stock_income_detail.quantity as count FROM stock_income_detail WHERE stock_expense_detail_id IS NOT NULL AND stock_income_id <> @current_income\n" +
						"UNION ALL\n" +
						"SELECT stock_write_off_detail.stock_expense_detail_id as id, stock_write_off_detail.quantity as count FROM stock_write_off_detail WHERE stock_expense_detail_id IS NOT NULL) as table1\n" +
						"GROUP BY id) as spent ON spent.id = stock_expense_detail.id \n" +
						"LEFT JOIN nomenclature ON nomenclature.id = stock_expense_detail.nomenclature_id " +
						"LEFT JOIN item_types ON nomenclature.type_id = item_types.id " +
						"LEFT JOIN stock_expense ON stock_expense.id = stock_expense_detail.stock_expense_id \n" +
						"LEFT JOIN stock_income_detail ON stock_income_detail.id = stock_expense_detail.stock_income_detail_id " +
						"LEFT JOIN units ON nomenclature.units_id = units.id " +
						"WHERE stock_expense.object_id = @id AND (spent.count IS NULL OR spent.count < stock_expense_detail.quantity )";
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue ("@id", _ObjectId);
				cmd.Parameters.AddWithValue ("@current_income", _IncomeDocId);
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
						PropertyListStore.AppendValues(rdr.GetInt64("id"),
						                               rdr.GetInt32("nomenclature_id"),
						                               rdr.GetString ("name"),
						                               String.Format ("{0:d}", rdr.GetDateTime ("date")),
						                               Quantity,
						                               String.Format ("{0} %", rdr.GetDecimal("life_percent") * 100),
						                               String.Format ("{0} %", Life),
						                               (double) Life,
						                               ObjectId,
						                               String.Empty,
						                               rdr["unit"].ToString()
						                               );
					}
				}
				logger.Info("Ok");
				buttonAdd.Sensitive = true;
				return true;
			}
			catch (Exception ex)
			{
				logger.WarnException("Ошибка получения о выданного имущества!", ex);
				throw;
			}
		}

		private void FillIncomeDetails()
		{
			logger.Info("Запрос деталей документа №" + _IncomeDocId +"...");
			try
			{
				string sql = "SELECT stock_income_detail.id, stock_income_detail.stock_expense_detail_id, " +
					"stock_income_detail.nomenclature_id, nomenclature.name, stock_income_detail.quantity, " +
					"stock_income_detail.life_percent, stock_income_detail.cost, units.name as unit " +
					"FROM stock_income_detail " +
					"LEFT JOIN nomenclature ON nomenclature.id = stock_income_detail.nomenclature_id " +
					"LEFT JOIN units ON units.id = nomenclature.units_id " +
					"WHERE stock_income_detail.stock_income_id = @id";

				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@id", _IncomeDocId);
				using( MySqlDataReader rdr = cmd.ExecuteReader())
				{
					while (rdr.Read())
					{
						long ExpenseRow;
						if(rdr["stock_expense_detail_id"] != DBNull.Value)
							ExpenseRow = rdr.GetInt64("stock_expense_detail_id");
						else
							ExpenseRow = -1;
						ItemsListStore.AppendValues(rdr.GetInt64("id"),
						                            ExpenseRow,
						                              rdr.GetInt32 ("nomenclature_id"),
						                              rdr["name"].ToString(),
						                              rdr.GetInt32("quantity"),
						                              rdr.GetDouble("life_percent") * 100,
						                            rdr.GetDouble("cost"),
						                            rdr["unit"].ToString()
						                            );
					}
				}

				logger.Info("Ok");
				CalculateTotal();
			}
			catch (Exception ex)
			{
				logger.WarnException("Ошибка получения деталей прихода!", ex);
				throw;
			}
		}

		private bool FilterTreeCardRows (Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			if(model.GetValue (iter, 0) == null)
				return false;
			long rowid = (long) model.GetValue (iter, 0);

			foreach (object[] row in ItemsListStore)
			{
				if((long)row[1] == rowid)
					return false;
			}

			return true;
		}

		private bool FilterTreeProperty (Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			if(model.GetValue (iter, 0) == null)
				return false;
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
			if (_Operation == IncomeDoc.Operations.Return)
			{
				SelectWearCardRow WinSelect = new SelectWearCardRow(CardRowsFilter);
				WinSelect.WorkerComboActive = false;
				if (WinSelect.GetResult(out iter))
				{
					ItemsListStore.AppendValues(null,
					                            CardRowsFilter.GetValue(iter, 0),
					                            CardRowsFilter.GetValue(iter, 1),
					                            CardRowsFilter.GetValue(iter, 2),
					                            CardRowsFilter.GetValue(iter, 4),
					                            CardRowsFilter.GetValue(iter, 7),
					                            0.0,
					                            CardRowsFilter.GetValue(iter, 10)
					                            );
					CardRowsFilter.Refilter();
					CalculateTotal();
				}
			}
			else if (_Operation == IncomeDoc.Operations.Object)
			{
				SelectObjectProperty WinSelect = new SelectObjectProperty(PropertyFilter);
				WinSelect.ObjectComboActive = false;
				if (WinSelect.GetResult(out iter))
				{
					ItemsListStore.AppendValues(null,
					                            PropertyFilter.GetValue(iter, 0),
					                            PropertyFilter.GetValue(iter, 1),
					                            PropertyFilter.GetValue(iter, 2),
					                            PropertyFilter.GetValue(iter, 4),
					                            PropertyFilter.GetValue(iter, 7),
					                            0.0,
					                            PropertyFilter.GetValue(iter, 10)
					                            );
					PropertyFilter.Refilter();
					CalculateTotal();
				}
			}
			else
			{
				Reference NomenclatureSelect = new Reference();
				NomenclatureSelect.SetMode(false, true, true, true, false);
				//NomenclatureSelect.SqlSelect = "SELECT id, last_name, first_name, patronymic_name FROM @tablename ";
				//NomenclatureSelect.Columns[1].DisplayFormat = "{1} {2} {3}";
				NomenclatureSelect.FillList("nomenclature","Номенклатура", "Номенклатура");
				NomenclatureSelect.Show();
				int result = NomenclatureSelect.Run();
				if((ResponseType)result == ResponseType.Ok)
				{
					ItemsListStore.AppendValues(-1,
					                            -1,
					                            NomenclatureSelect.SelectedID,
					                            NomenclatureSelect.SelectedName,
					                            1,
					                            100.0,
					                            0.0,
					                            "");
					CalculateTotal();
				}
				NomenclatureSelect.Destroy();
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

		void OnCostSpinEdited (object o, EditedArgs args)
		{
			TreeIter iter;
			if (!ItemsListStore.GetIterFromString (out iter, args.Path))
				return;
			double Cost;
			if (double.TryParse (args.NewText, out Cost)) 
			{
				ItemsListStore.SetValue (iter, 6, Cost);
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
			string units = (string) model.GetValue (iter, 7);
			(cell as Gtk.CellRendererSpin).Text = String.Format("{0} {1}", Quantity, units);
		}

		private void RenderCostColumn (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			double Cost = (double) model.GetValue (iter, 6);
			(cell as Gtk.CellRendererSpin).Text = String.Format("{0:0.00}", Cost); //Установить отображение денежных знаков не получилось значение портится при повторном редактировании.
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
			if(Operation == IncomeDoc.Operations.Return)
				CardRowsFilter.Refilter ();
			if(Operation == IncomeDoc.Operations.Object)
				CardRowsFilter.Refilter ();
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

		public bool SaveIncomeDetails(int IncomeDoc_id, MySqlTransaction trans)
		{
			string sql;
			MySqlCommand cmd;
			try
			{
				foreach(object[] row in ItemsListStore)
				{
					if((long)row[0] > 0)
						sql = "UPDATE stock_income_detail SET stock_income_id = @stock_income_id, nomenclature_id = @nomenclature_id, " +
							"quantity = @quantity, life_percent = @life_percent, cost = @cost, stock_expense_detail_id = @stock_expense_detail_id WHERE id = @id";
					else
						sql = "INSERT INTO stock_income_detail(stock_income_id, nomenclature_id, quantity, life_percent, cost, stock_expense_detail_id)" +
							"VALUES (@stock_income_id, @nomenclature_id, @quantity, @life_percent, @cost, @stock_expense_detail_id)";

					cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
					cmd.Parameters.AddWithValue("@id", row[0]);
					cmd.Parameters.AddWithValue("@stock_income_id", IncomeDoc_id);
					cmd.Parameters.AddWithValue("@nomenclature_id", row[2]);
					cmd.Parameters.AddWithValue("@quantity", row[4]);
					cmd.Parameters.AddWithValue("@life_percent", (double)row[5]/100.0);
					cmd.Parameters.AddWithValue("@cost", row[6]);
					if((long)row[1] > 0)
						cmd.Parameters.AddWithValue("@stock_expense_detail_id", row[1]);
					else
						cmd.Parameters.AddWithValue("@stock_expense_detail_id", DBNull.Value);

					cmd.ExecuteNonQuery ();
				}

				//Удаляем удаленные строки из базы данных
				sql = "DELETE FROM stock_income_detail WHERE id = @id";
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
				logger.WarnException("Ошибка записи строк прихода!", ex);
				throw;
			}
		}
	}
}

