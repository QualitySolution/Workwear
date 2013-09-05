using System;
using MySql.Data.MySqlClient;
using QSProjectsLib;

namespace workwear
{
	public partial class ObjectDlg : Gtk.Dialog
	{
		public bool NewItem;
		int Itemid;

		public ObjectDlg()
		{
			this.Build();
		}

		public void Fill(int id)
		{
			Itemid = id;
			NewItem = false;

			MainClass.StatusMessage(String.Format("Запрос объекта №{0}...", id));
			string sql = "SELECT * FROM objects WHERE id = @id";
			try
			{
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);

				cmd.Parameters.AddWithValue("@id", id);

				using(MySqlDataReader rdr = cmd.ExecuteReader())
				{		
					rdr.Read();

					labelID.Text = rdr["id"].ToString();
					entryName.Text = rdr["name"].ToString();
					entryAddress.Text = rdr.GetString("address");

					MainClass.StatusMessage("Ok");
				}
				this.Title = entryName.Text;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка получения информации о объекте!");
				QSMain.ErrorMessage(this, ex);
				this.Respond(Gtk.ResponseType.Reject);
			}
			TestCanSave();
		}

		protected void TestCanSave ()
		{
			bool Nameok = entryName.Text != "";
			buttonOk.Sensitive = Nameok;
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			string sql;
			if(NewItem)
			{
				sql = "INSERT INTO objects (name, address) " +
					"VALUES (@name, @address)";
			}
			else
			{
				sql = "UPDATE objects SET name = @name, address = @address WHERE id = @id";
			}
			MainClass.StatusMessage("Запись объекта...");
			try 
			{
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);

				cmd.Parameters.AddWithValue("@id", Itemid);
				cmd.Parameters.AddWithValue("@name", entryName.Text);
				cmd.Parameters.AddWithValue("@address", entryAddress.Text);

				cmd.ExecuteNonQuery();
				MainClass.StatusMessage("Ok");
				Respond (Gtk.ResponseType.Ok);
			} 
			catch (Exception ex) 
			{
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка записи объекта!");
				QSMain.ErrorMessage(this,ex);
			}
		}

		protected void OnEntryNameChanged(object sender, EventArgs e)
		{
			TestCanSave();
		}

	}
}

