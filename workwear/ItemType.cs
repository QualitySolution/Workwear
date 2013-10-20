using System;
using MySql.Data.MySqlClient;
using QSProjectsLib;

namespace workwear
{
	public partial class ItemType : Gtk.Dialog
	{
		public bool NewItem;
		int Itemid;
		
		public ItemType()
		{
			this.Build();
		}

		public void Fill(int id)
		{
			Itemid = id;
			NewItem = false;
			
			MainClass.StatusMessage(String.Format("Запрос типа номенклатуры №{0}...", id));
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
					spinQuantity.Value = rdr.GetDouble("norm_quantity");
					spinLife.Value = rdr.GetDouble("norm_life");
					switch (DBWorks.GetString(rdr, "category", "")) {
						case "wear":
							comboCategory.Active = 0;
							break;
						case "property":
							comboCategory.Active = 1;
							break;
					}
					MainClass.StatusMessage("Ok");
				}
				this.Title = entryName.Text;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка получения информации о услуге!");
				QSMain.ErrorMessage(this, ex);
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
				sql = "INSERT INTO item_types (name, category, norm_quantity, norm_life) " +
					"VALUES (@name, @category, @norm_quantity, @norm_life)";
			}
			else
			{
				sql = "UPDATE item_types SET name = @name, category = @category, norm_quantity = @norm_quantity, norm_life = @norm_life WHERE id = @id";
			}
			MainClass.StatusMessage("Запись тип номенклатуры...");
			try 
			{
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				
				cmd.Parameters.AddWithValue("@id", Itemid);
				cmd.Parameters.AddWithValue("@name", entryName.Text);
				cmd.Parameters.AddWithValue("@norm_quantity", spinQuantity.Value);
				cmd.Parameters.AddWithValue("@norm_life", spinLife.Value);
				switch (comboCategory.Active) {
					case 0:
						cmd.Parameters.AddWithValue("@category", "wear");
						break;
					case 1:
						cmd.Parameters.AddWithValue("@category", "property");
						break;
				}
				
				cmd.ExecuteNonQuery();
				MainClass.StatusMessage("Ok");
				Respond (Gtk.ResponseType.Ok);
			} 
			catch (Exception ex) 
			{
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка записи типа номенклатуры!");
				QSMain.ErrorMessage(this,ex);
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
	}
}

