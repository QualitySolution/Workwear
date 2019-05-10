using System;
using Gtk;
using MySql.Data.MySqlClient;
using NLog;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QSProjectsLib;
using workwear.Domain.Organization;

namespace workwear.Dialogs.Organization
{
	public partial class ObjectDlg : EntityDialogBase<Facility>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private Gtk.ListStore ItemsListStore;
		private TreeModel PlacementList;
		CellRendererCombo CellPlacement;

		public ObjectDlg (Facility obj) : this(obj.Id) {}

		public ObjectDlg (int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Facility>(id);
			ConfigureDlg();
			Fill(id);
		}

		public ObjectDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Facility>();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			//FIXME Временно чтобы не реализовывать более сложный механизм.
			HasChanges = true;

			//Создаем таблицу "материальных ценностей"
			ItemsListStore = new Gtk.ListStore (typeof (long), //0 in row id
			                                    typeof (int), //1 nomenclature id
			                                    typeof (string),//2 nomenclature name
			                                    typeof (string), //3 type nomenclature (not used)
			                                    typeof (string), // 4 nomenclature number (not used)
			                                    typeof (int), //5 income quantity
			                                    typeof (string), // 6 life
			                                    typeof (string), //7 units
			                                    typeof (string), // 8 income date
			                                    typeof (decimal), // 9 cost
			                                    typeof (DateTime), // 10 end date of life
			                                    typeof (int), 	// 11 placement id
			                                    typeof (string), // 12 placement name
			                                    typeof (int)	// 13 DB placement id
			                                    );

			Gtk.CellRendererText CellQuantityIn = new CellRendererText();
			Gtk.CellRendererText CellCost = new CellRendererText();

			CellPlacement = new CellRendererCombo();
			CellPlacement.TextColumn = 0;
			CellPlacement.Editable = true;
			CellPlacement.HasEntry = false;
			CellPlacement.Edited += OnPlacementComboEdited;

			treeviewProperty.AppendColumn ("Наименование", new Gtk.CellRendererText (), "text", 2);
			treeviewProperty.AppendColumn ("% годности", new Gtk.CellRendererText (), "text", 6);
			treeviewProperty.AppendColumn ("Кол-во", CellQuantityIn, RenderQuantityInColumn);
			treeviewProperty.AppendColumn ("Стоимость", CellCost, RenderCostColumn);
			treeviewProperty.AppendColumn ("Начало эксп.", new Gtk.CellRendererText (), "text", 8);
			treeviewProperty.AppendColumn ("Окончание эксп.", new Gtk.CellRendererText (), RenderEndOfLifeColumn);
			treeviewProperty.AppendColumn ("Расположение", CellPlacement, "text", 12);

			treeviewProperty.Model = ItemsListStore;
			treeviewProperty.ShowAll();
		}

		private void Fill(int id)
		{
			labelID.Text = Entity.Id.ToString();
			entryName.Text = Entity.Name;
			textviewAddress.Buffer.Text = Entity.Address;

			UpdatePlacementCombo();
			UpdateProperty();
			buttonPlacement.Sensitive = true;
			buttonGive.Sensitive = true;
			buttonReturn.Sensitive = true;
			buttonWriteOff.Sensitive = true;
			TabName = entryName.Text;

			TestCanSave();
		}

		protected void TestCanSave ()
		{
			bool Nameok = entryName.Text != "";
			buttonSave.Sensitive = Nameok;
		}

		public override bool Save ()
		{
			logger.Info("Запись объекта...");

			Entity.Name = entryName.Text;
			Entity.Address = textviewAddress.Buffer.Text;
			UoW.Save();

			SaveProperty();

			logger.Info("Ok");
			return true;
		}

		void SaveProperty()
		{
			QSMain.CheckConnectionAlive ();
			string sql = "UPDATE stock_expense_detail SET object_place_id = @object_place_id WHERE id = @id ";
			foreach(object[] row in ItemsListStore)
			{
				if((int) row[11] != (int) row[13])
				{
					MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
					cmd.Parameters.AddWithValue("@id", (long) row[0]);
					cmd.Parameters.AddWithValue("@object_place_id", DBWorks.ValueOrNull((int) row[11] > 0, (int) row[11]));
					cmd.ExecuteNonQuery();
				}
			}
		}

		protected void OnEntryNameChanged(object sender, EventArgs e)
		{
			TestCanSave();
		}

		protected void OnButtonPlacementClicked(object sender, EventArgs e)
		{
			Reference WinPlacement = new Reference(false);
			WinPlacement.ParentFieldName = "object_id";
			WinPlacement.ParentId = Entity.Id;
			WinPlacement.SqlSelect = "SELECT id, name FROM @tablename WHERE object_id = " + Entity.Id.ToString();
			WinPlacement.SetMode(true, false, true, true, true);
			WinPlacement.FillList("object_places", "размещение", "Размещения объекта");
			WinPlacement.Show();
			WinPlacement.Run();
			if(WinPlacement.ReferenceIsChanged)
				UpdatePlacementCombo();
			WinPlacement.Destroy();
		}

		private void RenderQuantityInColumn (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			int Quantity = (int) model.GetValue (iter, 5);
			string unit = (string) model.GetValue (iter, 7);
			if(Quantity > 0)
				(cell as Gtk.CellRendererText).Text = String.Format("{0} {1}", Quantity, unit);
			else
				(cell as Gtk.CellRendererText).Text = "";
		}

		private void RenderQuantityOutColumn (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			int Quantity = (int) model.GetValue (iter, 13);
			string unit = (string) model.GetValue (iter, 7);
			if(Quantity > 0)
				(cell as Gtk.CellRendererText).Text = String.Format("{0} {1}", Quantity, unit);
			else
				(cell as Gtk.CellRendererText).Text = "";
		}

		private void RenderCostColumn (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			if (model.GetValue(iter, 9) == null)
				return;
			decimal Cost = (decimal) model.GetValue (iter, 9);
			if(Cost >= 0)
				(cell as Gtk.CellRendererText).Text = String.Format("{0:C}", Cost);
			else
				(cell as Gtk.CellRendererText).Text = String.Empty;
		}

		private void RenderEndOfLifeColumn (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			DateTime EndDate = (DateTime) model.GetValue (iter, 10);
			if (EndDate < DateTime.Today) 
			{
				(cell as Gtk.CellRendererText).Foreground = "red";
			} 
			else 
			{
				(cell as Gtk.CellRendererText).Foreground = "darkgreen";
			}
			if(EndDate.Year == 1)
				(cell as Gtk.CellRendererText).Text = "";
			else
				(cell as Gtk.CellRendererText).Text = String.Format("{0:d}", EndDate);
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
			ItemsListStore.SetValue(iter, 12, args.NewText);
			TreeIter PlacementIter;
			ListStoreWorks.SearchListStore((ListStore)PlacementList, args.NewText, out PlacementIter);
			ItemsListStore.SetValue(iter, 11, PlacementList.GetValue(PlacementIter, 1));
		}

		private void UpdateProperty()
		{
			QSMain.CheckConnectionAlive ();
			logger.Info("Запрос выданного имущества...");
			try
			{
				string sql = "SELECT stock_expense_detail.id, stock_expense_detail.nomenclature_id, stock_expense_detail.quantity, stock_expense_detail.object_place_id, " +
					"nomenclature.name, stock_expense.date, stock_income_detail.life_percent, stock_income_detail.cost, spent.count, item_types.norm_life, measurement_units.name as unit, " +
						"object_places.name as placement, item_types.norm_life " +
						"FROM stock_expense_detail \n" +
						"LEFT JOIN (\nSELECT id, SUM(count) as count FROM \n" +
						"(SELECT stock_income_detail.stock_expense_detail_id as id, stock_income_detail.quantity as count FROM stock_income_detail WHERE stock_expense_detail_id IS NOT NULL " +
						"UNION ALL\n" +
						"SELECT stock_write_off_detail.stock_expense_detail_id as id, stock_write_off_detail.quantity as count FROM stock_write_off_detail WHERE stock_expense_detail_id IS NOT NULL) as table1\n" +
						"GROUP BY id) as spent ON spent.id = stock_expense_detail.id \n" +
						"LEFT JOIN nomenclature ON nomenclature.id = stock_expense_detail.nomenclature_id " +
						"LEFT JOIN item_types ON nomenclature.type_id = item_types.id " +
						"LEFT JOIN stock_expense ON stock_expense.id = stock_expense_detail.stock_expense_id \n" +
						"LEFT JOIN stock_income_detail ON stock_income_detail.id = stock_expense_detail.stock_income_detail_id " +
					"LEFT JOIN measurement_units ON item_types.units_id = measurement_units.id " +
						"LEFT JOIN object_places ON object_places.id = stock_expense_detail.object_place_id " +
						"WHERE stock_expense.object_id = @id AND (spent.count IS NULL OR spent.count < stock_expense_detail.quantity )";
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue ("@id", Entity.Id);
				MySqlDataReader rdr = cmd.ExecuteReader();

				ItemsListStore.Clear();
				while (rdr.Read())
				{
					int Quantity;
					if(rdr["count"] == DBNull.Value)
						Quantity = rdr.GetInt32("quantity");
					else
						Quantity = rdr.GetInt32("quantity") - rdr.GetInt32("count");
					DateTime EndOfLife;
					if(rdr["norm_life"] != DBNull.Value)
						EndOfLife = rdr.GetDateTime("date").AddMonths(rdr.GetInt32("norm_life"));
					else
						EndOfLife = new DateTime();
					ItemsListStore.AppendValues(rdr.GetInt64("id"),
						                        rdr.GetInt32("nomenclature_id"),
						                        rdr.GetString ("name"),
						                        string.Empty,
						                        string.Empty,
						                        Quantity,
					                            String.Format ("{0:P0}", rdr.GetDecimal("life_percent")),
						                        rdr["unit"].ToString(),
						                        String.Format ("{0:d}", rdr.GetDateTime ("date")),
						                        DBWorks.GetDecimal(rdr, "cost", -1),
					                            EndOfLife,
						                        DBWorks.GetInt(rdr, "object_place_id", 0),
						                        DBWorks.GetString(rdr, "placement", "нет"),
					                            DBWorks.GetInt(rdr, "object_place_id", 0)
						                        );
				}
				rdr.Close();
				logger.Info("Ok");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				logger.Warn("Ошибка получения имущества на объекте!");
			}

		}

		void UpdatePlacementCombo()
		{
			string sql = "SELECT name, id FROM object_places WHERE object_id = @id";
			MySqlParameter[] param = new MySqlParameter[]{ new MySqlParameter("@id", Entity.Id) };
			ComboBox PlacementCombo = new ComboBox();
			ComboWorks.ComboFillUniversal(PlacementCombo, sql, "{0}", param, 1, ComboWorks.ListMode.WithNo);
			CellPlacement.Model = PlacementList = PlacementCombo.Model;
			PlacementCombo.Destroy ();
		}

		protected void OnButtonGiveClicked(object sender, EventArgs e)
		{
			SaveIfPropertyChanged();
			Facility obj = UoW.GetById<Facility>(Entity.Id);
			ExpenseDocDlg winExpense = new ExpenseDocDlg(obj);
			winExpense.EntitySaved += (s, ea) => UpdateProperty();
			OpenTab(winExpense);
		}

		protected void OnButtonReturnClicked(object sender, EventArgs e)
		{
			SaveIfPropertyChanged();
			Facility obj = UoW.GetById<Facility>(Entity.Id);
			IncomeDocDlg winIncome = new IncomeDocDlg(obj);
			winIncome.EntitySaved += (s, ea) => UpdateProperty();
			OpenTab(winIncome);
		}

		protected void OnButtonWriteOffClicked(object sender, EventArgs e)
		{
			SaveIfPropertyChanged();
			Facility obj = UoW.GetById<Facility>(Entity.Id);
			WriteOffDocDlg winWriteOff = new WriteOffDocDlg(obj);
			winWriteOff.EntitySaved += (s, ea) => UpdateProperty();
			OpenTab(winWriteOff);
		}

		private void SaveIfPropertyChanged()
		{
			bool Changed = false;
			foreach(object[] row in ItemsListStore)
			{
				if((int) row[11] != (int) row[13])
				{
					Changed = true;
					break;
				}
			}

			if(Changed)
			{
				MessageDialog md = new MessageDialog ( (Window)this.Toplevel, DialogFlags.DestroyWithParent,
				                                      MessageType.Question, 
				                                      ButtonsType.YesNo, 
				                                      "В размещении имущества были сделаны изменения. При добавлении нового документа все незаписанные изменения пропадут. Записать сделанные изменения?");
				int result = (int) md.Run ();
				md.Destroy();

				if(result == (int) ResponseType.Yes)
				{
					SaveProperty();
				}
			}
		}

	}
}