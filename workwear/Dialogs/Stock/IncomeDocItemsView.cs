using System;
using Gtk;
using NLog;
using QSOrmProject;
using workwear.Domain.Stock;

namespace workwear
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class IncomeDocItemsView : WidgetOnDialogBase
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private Income incomeDoc;

		public Income IncomeDoc {
			get {return incomeDoc;}
			set {
				if (incomeDoc == value)
					return;
				incomeDoc = value;
				ytreeItems.ItemsDataSource = incomeDoc.ObservableItems;
				IncomeDoc.PropertyChanged += IncomeDoc_PropertyChanged;
				IncomeDoc_PropertyChanged (null, new System.ComponentModel.PropertyChangedEventArgs(IncomeDoc.GetPropertyName (d => d.Operation)));
			}
		}

		void IncomeDoc_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == IncomeDoc.GetPropertyName (d => d.Operation) 
				|| e.PropertyName == IncomeDoc.GetPropertyName (d => d.EmployeeCard)
				|| e.PropertyName == IncomeDoc.GetPropertyName (d => d.Facility))
			{
				buttonAdd.Sensitive = (IncomeDoc.Operation == IncomeOperations.Return && IncomeDoc.EmployeeCard != null) 
					|| (IncomeDoc.Operation == IncomeOperations.Object && IncomeDoc.Facility != null) 
					|| IncomeDoc.Operation == IncomeOperations.Enter;
			}
		}

		public IncomeDocItemsView()
		{
			this.Build();

			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<IncomeItem> ()
				.AddColumn ("Наименование").AddTextRenderer (e => e.Nomenclature.Name)
				.AddColumn ("Состояние").AddNumericRenderer (e => e.LifePercent, new MultiplierToPercentConverter()).Editing (new Adjustment(0,0,100,1,10,0))
				.AddTextRenderer (e => "%")
				.AddColumn ("Количество").AddNumericRenderer (e => e.Amount).Editing (new Adjustment(0, 0, 100000, 1, 10, 1))
				.AddTextRenderer (e => e.Nomenclature.Type.Units.Name)
				.AddColumn ("Стоимость").AddNumericRenderer (e => e.Cost).Editing (new Adjustment(0,0,100000000,100,1000,0)).Digits (2)
				.Finish ();
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
		}

		void YtreeItems_Selection_Changed (object sender, EventArgs e)
		{
			buttonDel.Sensitive = ytreeItems.Selection.CountSelectedRows () > 0;
		}


/*
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

			QSMain.CheckConnectionAlive ();
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
						"LEFT JOIN units ON item_types.units_id = units.id " +
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
				logger.Warn(ex, "Ошибка получения о выданного имущества!");
				throw;
			}
		}*/

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			if(IncomeDoc.Operation == IncomeOperations.Return)
			{
				var selectFromEmployeeDlg = new ReferenceRepresentation (new ViewModel.EmployeeBalanceVM (IncomeDoc.EmployeeCard));
				selectFromEmployeeDlg.Mode = OrmReferenceMode.MultiSelect;
				selectFromEmployeeDlg.ObjectSelected += SelectFromEmployeeDlg_ObjectSelected;

				var dialog = new OneWidgetDialog (selectFromEmployeeDlg);
				dialog.Show ();
				dialog.Run ();
				dialog.Destroy ();
			}

			if(IncomeDoc.Operation == IncomeOperations.Enter)
			{
				var selectNomenclatureDlg = new OrmReference (typeof(Nomenclature));
				selectNomenclatureDlg.Mode = OrmReferenceMode.Select;
				selectNomenclatureDlg.ObjectSelected += SelectNomenclatureDlg_ObjectSelected;

				var dialog = new OneWidgetDialog (selectNomenclatureDlg);
				dialog.Show ();
				dialog.Run ();
				dialog.Destroy ();
			}

		}

		void SelectFromEmployeeDlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			foreach(var node in e.GetNodes<ViewModel.EmployeeBalanceVMNode> ())
			{
				IncomeDoc.AddItem (MyOrmDialog.UoW.GetById<ExpenseItem> (node.Id), node.Added - node.Removed);
			}
			CalculateTotal();
		}

		void SelectNomenclatureDlg_ObjectSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			IncomeDoc.AddItem (e.Subject as Nomenclature);
			CalculateTotal();
		}

		protected void OnButtonDelClicked (object sender, EventArgs e)
		{
			IncomeDoc.RemoveItem (ytreeItems.GetSelectedObject<IncomeItem> ());
			CalculateTotal();
		}

		private void CalculateTotal()
		{
			labelSum.Text = String.Format ("Количество: {0}", IncomeDoc.Items.Count);
		} 
	}
}

