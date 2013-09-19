using System;
using Gtk;
using MySql.Data.MySqlClient;
using QSProjectsLib;

namespace workwear
{
	public partial class ExpenseDoc : Gtk.Dialog
	{
		public bool NewItem;
		int Itemid, Worker_id;

		public ExpenseDoc()
		{
			this.Build();
			dateDoc.Date = DateTime.Today;
			labelUser.LabelProp = QSMain.User.Name;
		}

		public void Fill(int id)
		{
			Itemid = id;
			NewItem = false;

			MainClass.StatusMessage(String.Format("Запрос расходного документа №{0}...", id));
			string sql = "SELECT stock_expense.*, wear_cards.last_name, wear_cards.first_name, wear_cards.patronymic_name, users.name as user " +
				"FROM stock_expense " +
				"LEFT JOIN wear_cards ON wear_cards.id = stock_expense.wear_card_id " +
				"LEFT JOIN users ON stock_expense.user_id = users.id " +
				"WHERE stock_expense.id = @id";
			try
			{
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);

				cmd.Parameters.AddWithValue("@id", id);

				using(MySqlDataReader rdr = cmd.ExecuteReader())
				{		
					rdr.Read();

					labelId.LabelProp = rdr["id"].ToString();
					labelUser.LabelProp = rdr["user"].ToString();
					if(rdr["wear_card_id"] != DBNull.Value)
					{
						Worker_id = rdr.GetInt32("wear_card_id");
						entryWorker.Text = String.Format("{0} {1}. {2}.", rdr["last_name"].ToString(), rdr["first_name"].ToString()[0], rdr["patronymic_name"].ToString()[0]);
						entryWorker.TooltipText = String.Format("{0} {1} {2}", rdr["last_name"].ToString(), rdr["first_name"].ToString(), rdr["patronymic_name"].ToString());
					}
					else
					{
						Worker_id = -1;
					}
					if(rdr["date"] != DBNull.Value)
					{
						dateDoc.Date = rdr.GetDateTime("date");
					}
				}
				ItemsTable.ExpenseDocId = Itemid;
				if(Worker_id > 0)
					ItemsTable.WorkerId = Worker_id;
				MainClass.StatusMessage("Ok");

				this.Title =  "Выдача №" + labelId.LabelProp;
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
			bool WorkerOk = Worker_id > 0;
			bool DetailOk = ItemsTable.CanSave;
			buttonOk.Sensitive = Dateok && WorkerOk && DetailOk;
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			string sql;
			if(NewItem)
			{
				sql = "INSERT INTO stock_expense (date, wear_card_id, user_id) " +
						"VALUES (@date, @wear_card_id, @user_id)";
			}
			else
			{
				sql = "UPDATE stock_expense SET date = @date, wear_card_id = @wear_card_id " +
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
				if (Worker_id > 0)
					cmd.Parameters.AddWithValue("@wear_card_id", Worker_id);
				else
					cmd.Parameters.AddWithValue("@wear_card_id", DBNull.Value);

				cmd.ExecuteNonQuery();
				if(NewItem)
					Itemid = (int) cmd.LastInsertedId;

				if(ItemsTable.SaveExpenseDetails(Itemid, trans))
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

		protected void OnButtonWorkerEditClicked(object sender, EventArgs e)
		{
			Reference WorkerSelect = new Reference();
			WorkerSelect.SetMode(false, true, true, true, false);
			WorkerSelect.SqlSelect = "SELECT id, last_name, first_name, patronymic_name FROM @tablename ";
			WorkerSelect.Columns[1].DisplayFormat = "{1} {2} {3}";
			WorkerSelect.FillList("wear_cards","Работника", "Карточки работников");
			WorkerSelect.Show();
			int result = WorkerSelect.Run();
			if((ResponseType)result == ResponseType.Ok)
			{
				SetWorker(WorkerSelect.SelectedID, WorkerSelect.SelectedName);
			}
			WorkerSelect.Destroy();
			TestCanSave();
		}

		public void SetWorker(int id, string name)
		{
			Worker_id = id;
			string[] Parts = name.Split(new char[] {' '});
			entryWorker.Text = String.Format("{0} {1}. {2}.", Parts[0], Parts[1][0], Parts[2][0]);
			entryWorker.TooltipText = name;
			ItemsTable.WorkerId = Worker_id;
		}

		protected void OnItemsTableCanSaveStateChanged(object sender, EventArgs e)
		{
			TestCanSave();
		}
	}
}

