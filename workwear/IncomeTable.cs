using System;
using Gtk;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using QSProjectsLib;

namespace workwear
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class IncomeTable : Gtk.Bin
	{
		private int _WorkerId, _IncomeDocId;
		private Operations _Operation;
		private bool _CanSave = false;
		private Gtk.ListStore ItemsListStore, CardRowsListStore;
		TreeModelFilter CardRowsFilter;
		private List<long> DeletedRowId = new List<long>();

		public event EventHandler CanSaveStateChanged;

		public enum Operations {Enter, Return};

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

		public Operations Operation {
			get {return _Operation;}
			set {if (value == Operations.Enter)
					buttonAdd.Sensitive = true;

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
			                                    typeof (double) // 5 life
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
			CellLife.Editable = true;
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

		private bool FillCardRowsList()
		{
			if(_WorkerId <= 0)
				return false;
			CardRowsListStore = new ListStore (typeof (long), // 0 - ID expense row
			                                      typeof (int), //1 - nomenclature id
			                                      typeof (string), //2 - nomenclature name
			                                      typeof (string), //3 - date
			                                      typeof (int), //4 - quantity
			                                      typeof (string), // 5 - % of life
			                                   typeof (string), // 6 - today % of life
			                                   typeof(double)); // 7 - today % of life
			CardRowsFilter = new TreeModelFilter( CardRowsListStore, null);
			CardRowsFilter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FilterTreeAccrualRows);

			MainClass.StatusMessage("Запрос спецодежды по работнику...");
			try
			{
				string sql = "SELECT stock_expense_detail.id, stock_expense_detail.nomenclature_id, stock_expense_detail.quantity,\n" +
					"nomenclature.name, stock_expense.date, stock_income_detail.life_percent, spent.count, item_types.norm_life " +
					"FROM stock_expense_detail \n" +
					"LEFT JOIN (\nSELECT id, SUM(count) as count FROM \n" +
						"(SELECT stock_income_detail.stock_expense_detail_id as id, stock_income_detail.quantity as count FROM stock_income_detail WHERE stock_expense_detail_id IS NOT NULL AND stock_income_id <> @current_income\n" +
					"UNION ALL\n" +
					"SELECT stock_write_off_detail.stock_expense_detail_id as id, stock_write_off_detail.quantity as count FROM stock_write_off_detail WHERE stock_expense_detail_id IS NOT NULL) as table1\n" +
					"GROUP BY id) as spent ON spent.id = stock_expense_detail.id \n" +
					"LEFT JOIN nomenclature ON nomenclature.id = stock_expense_detail.nomenclature_id " +
					"LEFT JOIN item_types ON nomenclature.type_id = item_types.id " +
					"LEFT JOIN stock_expense ON stock_expense.id = stock_expense_detail.stock_expense_id \n" +
					"LEFT JOIN stock_income_detail ON stock_income_detail.id = stock_expense_detail.stock_income_detail_id \n" +
					"WHERE stock_expense.wear_card_id = @id AND (spent.count IS NULL OR spent.count < stock_expense_detail.quantity )";
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue ("@id", _WorkerId);
				cmd.Parameters.AddWithValue ("@current_income", _IncomeDocId);
				MySqlDataReader rdr = cmd.ExecuteReader();

				while (rdr.Read())
				{
					int Quantity;
					if(rdr["count"] == DBNull.Value)
					 	Quantity = rdr.GetInt32("quantity");
					else
						Quantity = rdr.GetInt32("quantity") - rdr.GetInt32("count");
					 ;
					double MonthUsing = ((DateTime.Today - rdr.GetDateTime ("date")).TotalDays / 365) * 12;
					int Life = (int) (rdr.GetDecimal("life_percent") * 100) - (int) (MonthUsing / rdr.GetInt32("norm_life") * 100);
					if(Life < 0) 
						Life = 0;
					CardRowsListStore.AppendValues(rdr.GetInt64("id"),
					                                  rdr.GetInt32("nomenclature_id"),
					                                  rdr.GetString ("name"),
					                                  String.Format ("{0:d}", rdr.GetDateTime ("date")),
					                               	Quantity,
					                                  String.Format ("{0} %", rdr.GetDecimal("life_percent") * 100),
					                               String.Format ("{0} %", Life),
					                               (double) Life);
				}
				rdr.Close();
				MainClass.StatusMessage("Ok");
				buttonAdd.Sensitive = true;
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка получения начисления!");
				return false;
			}
		}

		private void FillIncomeDetails()
		{
			MainClass.StatusMessage("Запрос деталей документа №" + _IncomeDocId +"...");
			try
			{
				string sql = "SELECT stock_income_detail.id, stock_income_detail.stock_expense_detail_id, " +
					"stock_income_detail.nomenclature_id, nomenclature.name, stock_income_detail.quantity, " +
					"stock_income_detail.life_percent " +
					"FROM stock_income_detail " +
					"LEFT JOIN nomenclature ON nomenclature.id = stock_income_detail.nomenclature_id " +
					"WHERE stock_income_detail.stock_income_id = @id";

				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@id", _IncomeDocId);
				MySqlDataReader rdr = cmd.ExecuteReader();

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
					                              rdr.GetDouble("life_percent") * 100
					                              );
				}
				rdr.Close();

				MainClass.StatusMessage("Ok");
				CalculateTotal();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка получения деталей прихода!");
			}
		}

		private bool FilterTreeAccrualRows (Gtk.TreeModel model, Gtk.TreeIter iter)
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
			if (_Operation == Operations.Return)
			{
				SelectWearCardRow WinSelect = new SelectWearCardRow(CardRowsFilter);
				if (WinSelect.GetResult(out iter))
				{
					ItemsListStore.AppendValues(null,
					                            CardRowsFilter.GetValue(iter, 0),
					                            CardRowsFilter.GetValue(iter, 1),
					                            CardRowsFilter.GetValue(iter, 2),
					                            CardRowsFilter.GetValue(iter, 4),
					                            CardRowsFilter.GetValue(iter, 7));
					CardRowsFilter.Refilter();
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
					                            100.0);
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
			(cell as Gtk.CellRendererText).Text = String.Format("{0}", Quantity);
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
			CardRowsFilter.Refilter ();
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
							"quantity = @quantity, life_percent = @life_percent, stock_expense_detail_id = @stock_expense_detail_id WHERE id = @id";
					else
						sql = "INSERT INTO stock_income_detail(stock_income_id, nomenclature_id, quantity, life_percent, stock_expense_detail_id)" +
							"VALUES (@stock_income_id, @nomenclature_id, @quantity, @life_percent, @stock_expense_detail_id)";

					cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
					cmd.Parameters.AddWithValue("@id", row[0]);
					cmd.Parameters.AddWithValue("@stock_income_id", IncomeDoc_id);
					cmd.Parameters.AddWithValue("@nomenclature_id", row[2]);
					cmd.Parameters.AddWithValue("@quantity", row[4]);
					cmd.Parameters.AddWithValue("@life_percent", (double)row[5]/100.0);
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
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка записи строк прихода!");
				return false;
			}
		}
	}
}

