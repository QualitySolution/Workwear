using System;
using Gtk;
using MySql.Data.MySqlClient;
using QSProjectsLib;

namespace workwear
{
	public partial class IncomeDoc : Gtk.Dialog
	{
		public bool NewItem;
		int Itemid, Worker_id;
		string DocName;

		public IncomeDoc()
		{
			this.Build();
			dateDoc.Date = DateTime.Today;
			labelUser.LabelProp = QSMain.User.Name;
			comboOperation.Active = 0;
		}

		protected void OnComboOperationChanged(object sender, EventArgs e)
		{
			switch (comboOperation.Active)
			{
				case 0:
					labelTTN.Visible = true;
					labelWorker.Visible = false;
					entryTTN.Visible = true;
					hboxWorker.Visible = false;
					DocName = "Приходная накладная № ";
					this.Title = "Новая приходная накладная";
					ItemsTable.Operation = IncomeTable.Operations.Enter;
					break;
				case 1:
					labelTTN.Visible = false;
					labelWorker.Visible = true;
					entryTTN.Visible = false;
					hboxWorker.Visible = true;
					DocName = "Возврат от работника № ";
					this.Title = "Новый возврат от работника";
					ItemsTable.Operation = IncomeTable.Operations.Return;
					break;
			}
			TestCanSave();
		}

		public void Fill(int id)
		{
			Itemid = id;
			NewItem = false;
			comboOperation.Sensitive = false;

			MainClass.StatusMessage(String.Format("Запрос приходного документа №{0}...", id));
			string sql = "SELECT stock_income.*, wear_cards.last_name, wear_cards.first_name, wear_cards.patronymic_name, users.name as user " +
				"FROM stock_income " +
				"LEFT JOIN wear_cards ON wear_cards.id = stock_income.wear_card_id " +
				"LEFT JOIN users ON stock_income.user_id = users.id " +
				"WHERE stock_income.id = @id";
			try
			{
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);

				cmd.Parameters.AddWithValue("@id", id);

				using(MySqlDataReader rdr = cmd.ExecuteReader())
				{		
					rdr.Read();

					labelId.LabelProp = rdr["id"].ToString();
					entryTTN.Text = rdr["number"].ToString();
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
					switch (rdr.GetString("operation")) 
					{
						case "enter":
							comboOperation.Active = 0;
							break;
						case "return":
							comboOperation.Active = 1;
							break;
					}
				}
				ItemsTable.IncomeDocId = Itemid;
				if(Worker_id > 0)
					ItemsTable.WorkerId = Worker_id;
				MainClass.StatusMessage("Ok");

				this.Title =  DocName + labelId.LabelProp;
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
			bool OperationOk;
			bool NumberOk = entryTTN.Text != "";
			bool WorkerOk = Worker_id > 0;
			switch (comboOperation.Active)
			{
				case 0:
					OperationOk = NumberOk;
					break;
				case 1:
					OperationOk = WorkerOk;
					break;
				default:
					OperationOk = false;
					break;
			}
			buttonOk.Sensitive = Dateok && OperationOk ;
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			string sql;
			if(NewItem)
			{
				sql = "INSERT INTO stock_income (operation, number, date, " +
					"wear_card_id, user_id) " +
						"VALUES (@operation, @number, @date, @wear_card_id, @user_id)";
			}
			else
			{
				sql = "UPDATE stock_income SET operation = @operation, number = @number, " +
					"date = @date, wear_card_id = @wear_card_id " +
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
				if (Worker_id > 0 && comboOperation.Active == 1)
					cmd.Parameters.AddWithValue("@wear_card_id", Worker_id);
				else
					cmd.Parameters.AddWithValue("@wear_card_id", DBNull.Value);
				if(entryTTN.Text != "" && comboOperation.Active == 0)
					cmd.Parameters.AddWithValue("@number", entryTTN.Text);
				else
					cmd.Parameters.AddWithValue("@number", DBNull.Value);
				switch (comboOperation.Active) {
					case 0: cmd.Parameters.AddWithValue("@operation", "enter");
						break;
						case 1: cmd.Parameters.AddWithValue("@operation", "return");
						break;
						default: cmd.Parameters.AddWithValue("@operation", DBNull.Value);
						break;
				}


				cmd.ExecuteNonQuery();
				if(NewItem)
					Itemid = (int) cmd.LastInsertedId;

				if(ItemsTable.SaveIncomeDetails(Itemid, trans))
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

		protected void OnEntryTTNChanged(object sender, EventArgs e)
		{
			TestCanSave();
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
				Worker_id = WorkerSelect.SelectedID;
				string[] Parts = WorkerSelect.SelectedName.Split(new char[] {' '});
				entryWorker.Text = String.Format("{0} {1}. {2}.", Parts[0], Parts[1][0], Parts[2][0]);
				entryWorker.TooltipText = WorkerSelect.SelectedName;
				ItemsTable.WorkerId = Worker_id;
			}
			WorkerSelect.Destroy();
			TestCanSave();
		}
	}
}

