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
			buttonOk.Sensitive = Nameok;
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			string sql;
			if(NewItem)
			{
				sql = "INSERT INTO item_types (name, norm_quantity, norm_life) " +
					"VALUES (@name, @norm_quantity, @norm_life)";
			}
			else
			{
				sql = "UPDATE item_types SET name = @name, norm_quantity = @norm_quantity, norm_life = @norm_life WHERE id = @id";
			}
			MainClass.StatusMessage("Запись тип номенклатуры...");
			try 
			{
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				
				cmd.Parameters.AddWithValue("@id", Itemid);
				cmd.Parameters.AddWithValue("@name", entryName.Text);
				cmd.Parameters.AddWithValue("@norm_quantity", spinQuantity.Value);
				cmd.Parameters.AddWithValue("@norm_life", spinLife.Value);
				
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
	}
}

