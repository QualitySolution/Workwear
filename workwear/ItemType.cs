using System;
using MySql.Data.MySqlClient;
using NLog;
using QSProjectsLib;

namespace workwear
{
	public partial class ItemType : Gtk.Dialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public bool NewItem;
		int Itemid;
		
		public ItemType()
		{
			this.Build();

			ComboWorks.ComboFillReference(comboUnits, "units", ComboWorks.ListMode.WithNo);
		}

		public void Fill(int id)
		{
			Itemid = id;
			NewItem = false;

			QSMain.CheckConnectionAlive ();
			logger.Info("Запрос типа номенклатуры №{0}...", id);
			string sql = "SELECT * FROM item_types WHERE item_types.id = @id";
			try
			{
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				
				cmd.Parameters.AddWithValue("@id", id);
				
				using(MySqlDataReader rdr = cmd.ExecuteReader())
				{		
					rdr.Read();

					labelId.Text = rdr["id"].ToString();
					entryName.Text = rdr["name"].ToString();
					spinQuantity.Value = DBWorks.GetDouble(rdr, "norm_quantity", 0);
					spinLife.Value = DBWorks.GetDouble(rdr, "norm_life", 0);
					checkOnNorms.Active = DBWorks.GetBoolean (rdr, "on_norms", false);
					ComboWorks.SetActiveItem (comboUnits, DBWorks.GetInt (rdr, "units_id", -1));
					switch (DBWorks.GetString(rdr, "category", "")) {
						case "wear":
							comboCategory.Active = 0;
							break;
						case "property":
							comboCategory.Active = 1;
							break;
					}
					logger.Info("Ok");
				}
				this.Title = entryName.Text;
			}
			catch (Exception ex)
			{
				QSMain.ErrorMessageWithLog(this, "Ошибка получения информации о услуге!", logger, ex);
				this.Respond(Gtk.ResponseType.Reject);
			}
			TestCanSave();
		}

		protected	void TestCanSave ()
		{
			bool Nameok = entryName.Text != "";
			bool CategoryOk = comboCategory.Active >= 0;
			buttonOk.Sensitive = Nameok && CategoryOk;
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			string sql;
			if(NewItem)
			{
				sql = "INSERT INTO item_types (name, category, units_id, on_norms, norm_quantity, norm_life) " +
					"VALUES (@name, @category, @units_id, @on_norms, @norm_quantity, @norm_life)";
			}
			else
			{
				sql = "UPDATE item_types SET name = @name, category = @category, units_id = @units_id, on_norms = @on_norms, " +
					"norm_quantity = @norm_quantity, norm_life = @norm_life WHERE id = @id";
			}
			QSMain.CheckConnectionAlive ();
			logger.Info("Запись типа номенклатуры...");
			try 
			{
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				
				cmd.Parameters.AddWithValue("@id", Itemid);
				cmd.Parameters.AddWithValue("@name", entryName.Text);
				cmd.Parameters.AddWithValue("@units_id", ComboWorks.GetActiveIdOrNull (comboUnits));
				cmd.Parameters.AddWithValue("@on_norms", checkOnNorms.Active);
				cmd.Parameters.AddWithValue("@norm_quantity", DBWorks.ValueOrNull(checkOnNorms.Active, spinQuantity.Value));
				cmd.Parameters.AddWithValue("@norm_life", DBWorks.ValueOrNull(checkOnNorms.Active, spinLife.Value));
				switch (comboCategory.Active) {
					case 0:
						cmd.Parameters.AddWithValue("@category", "wear");
						break;
					case 1:
						cmd.Parameters.AddWithValue("@category", "property");
						break;
				}
				
				cmd.ExecuteNonQuery();
				logger.Info("Ok");
				Respond (Gtk.ResponseType.Ok);
			} 
			catch (Exception ex) 
			{
				QSMain.ErrorMessageWithLog(this, "Ошибка записи типа номенклатуры!", logger, ex);
			}
		}

		protected void OnEntryNameChanged(object sender, EventArgs e)
		{
			TestCanSave();
		}

		protected void OnComboCategoryChanged(object sender, EventArgs e)
		{
			TestCanSave();
		}

		protected void OnCheckOnNormsToggled (object sender, EventArgs e)
		{
			spinLife.Sensitive = spinQuantity.Sensitive = labelLife.Sensitive = labelQuality.Sensitive 
				= checkOnNorms.Active;
		}
	}
}

