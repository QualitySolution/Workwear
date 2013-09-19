using System;
using Gtk;
using MySql.Data.MySqlClient;
using QSProjectsLib;

namespace workwear
{
	public partial class WriteOffDoc : Gtk.Dialog
	{
		public bool NewItem;
		private int _CurrentWorkerId = -1;
		int Itemid;

		public WriteOffDoc()
		{
			this.Build();
			dateDoc.Date = DateTime.Today;
			labelUser.LabelProp = QSMain.User.Name;
		}

		public int CurrentWorkerId
		{
			get {return _CurrentWorkerId;} 
			set {if(_CurrentWorkerId != value)
				{
					_CurrentWorkerId = value;
					ItemsTable.CurWorkerId = value;
				}
			}
		}

		public void Fill(int id)
		{
			Itemid = id;
			NewItem = false;

			MainClass.StatusMessage(String.Format("Запрос акта списания №{0}...", id));
			string sql = "SELECT stock_write_off.*, users.name as user " +
				"FROM stock_write_off " +
				"LEFT JOIN users ON stock_write_off.user_id = users.id " +
				"WHERE stock_write_off.id = @id";
			try
			{
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);

				cmd.Parameters.AddWithValue("@id", id);

				using(MySqlDataReader rdr = cmd.ExecuteReader())
				{		
					rdr.Read();

					labelId.LabelProp = rdr["id"].ToString();
					labelUser.LabelProp = rdr["user"].ToString();
					if(rdr["date"] != DBNull.Value)
					{
						dateDoc.Date = rdr.GetDateTime("date");
					}
				}
				ItemsTable.WriteOffDocId = Itemid;
				MainClass.StatusMessage("Ok");

				this.Title =  "Списание №" + labelId.LabelProp;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка получения документа!");
				QSMain.ErrorMessage(this, ex);
				this.Respond(Gtk.ResponseType.Reject);
			}
			TestCanSave();
		}

		protected void TestCanSave ()
		{
			bool Dateok = !dateDoc.IsEmpty;
			bool DetailOk = ItemsTable.CanSave;
			buttonOk.Sensitive = Dateok && DetailOk;
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			string sql;
			if(NewItem)
			{
				sql = "INSERT INTO stock_write_off (date, user_id) " +
						"VALUES (@date, @user_id)";
			}
			else
			{
				sql = "UPDATE stock_write_off SET date = @date, " +
					"WHERE id = @id";
			}
			MainClass.StatusMessage("Запись документа...");
			MySqlTransaction trans = QSMain.connectionDB.BeginTransaction();
			try 
			{
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);

				cmd.Parameters.AddWithValue("@id", Itemid);
				cmd.Parameters.AddWithValue("@date", dateDoc.Date);
				cmd.Parameters.AddWithValue("@user_id", QSMain.User.id);

				cmd.ExecuteNonQuery();
				if(NewItem)
					Itemid = (int) cmd.LastInsertedId;

				if(ItemsTable.SaveWriteOffDetails(Itemid, trans))
					trans.Commit();
				else
					trans.Rollback();
				MainClass.StatusMessage("Ok");
				Respond (Gtk.ResponseType.Ok);
			} 
			catch (Exception ex) 
			{
				trans.Rollback();
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка записи документа!");
				QSMain.ErrorMessage(this,ex);
			}
		}

		protected void OnDateDocDateChanged(object sender, EventArgs e)
		{
			TestCanSave();
		}

		protected void OnItemsTableCanSaveStateChanged(object sender, EventArgs e)
		{
			TestCanSave();
		}
	}
}

