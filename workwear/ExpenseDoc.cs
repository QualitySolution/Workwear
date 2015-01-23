using System;
using Gtk;
using MySql.Data.MySqlClient;
using NLog;
using QSProjectsLib;

namespace workwear
{
	public partial class ExpenseDoc : Gtk.Dialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public bool NewItem;
		private int Itemid, Worker_id, Object_id;

		public enum Operations {Employee, Object};

		public ExpenseDoc()
		{
			this.Build();
			dateDoc.Date = DateTime.Today;
			labelUser.LabelProp = QSMain.User.Name;
			comboOperation.Active = 0;
		}

		public Operations Operation
		{
			get {return (Operations)comboOperation.Active;}
			set	{comboOperation.Active = (int) value;}
		}

		public void Fill(int id)
		{
			Itemid = id;
			NewItem = false;

			QSMain.CheckConnectionAlive ();
			logger.Info("Запрос расходного документа №{0}...", id);
			string sql = "SELECT stock_expense.*, wear_cards.last_name, wear_cards.first_name, wear_cards.patronymic_name, objects.name as object, objects.address, users.name as user " +
				"FROM stock_expense " +
				"LEFT JOIN wear_cards ON wear_cards.id = stock_expense.wear_card_id " +
				"LEFT JOIN users ON stock_expense.user_id = users.id " +
				"LEFT JOIN objects ON objects.id = stock_expense.object_id " +
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
					switch (rdr.GetString("operation")) 
					{
						case "employee":
							comboOperation.Active = 0;
							break;
						case "object":
							comboOperation.Active = 1;
							break;
					}
					comboOperation.Sensitive = false;
					if(rdr["wear_card_id"] != DBNull.Value)
					{
						Worker_id = rdr.GetInt32("wear_card_id");
						if(rdr["first_name"].ToString() == "" || rdr["patronymic_name"].ToString() == "")
							entryWorker.Text = String.Format("{0} {1} {2}", rdr["last_name"].ToString(), rdr["first_name"].ToString(), rdr["patronymic_name"].ToString());
						else
							entryWorker.Text = String.Format("{0} {1}. {2}.", rdr["last_name"].ToString(), rdr["first_name"].ToString()[0], rdr["patronymic_name"].ToString()[0]);
						entryWorker.TooltipText = String.Format("{0} {1} {2}", rdr["last_name"].ToString(), rdr["first_name"].ToString(), rdr["patronymic_name"].ToString());
					}
					else
					{
						Worker_id = -1;
					}
					Object_id = DBWorks.GetInt(rdr, "object_id", -1);
					entryObject.Text = DBWorks.GetString(rdr, "object", "");
					entryObject.TooltipText = String.Format("{0}\n{1}", DBWorks.GetString(rdr, "object", ""), DBWorks.GetString(rdr, "address", ""));
					if(rdr["date"] != DBNull.Value)
					{
						dateDoc.Date = rdr.GetDateTime("date");
					}
				}
				ItemsTable.ExpenseDocId = Itemid;
				if(Worker_id > 0)
					ItemsTable.WorkerId = Worker_id;
				if(Object_id > 0)
					ItemsTable.ObjectId = Object_id;
				logger.Info("Ok");

				this.Title =  "Выдача №" + labelId.LabelProp;
			}
			catch (Exception ex)
			{
				QSMain.ErrorMessageWithLog(this, "Ошибка получения документа!", logger, ex);
				this.Respond(Gtk.ResponseType.Reject);
			}
			TestCanSave();
		}

		protected void TestCanSave ()
		{
			bool Dateok = !dateDoc.IsEmpty;
			bool WorkerOk = Worker_id > 0;
			bool ObjectOk = Object_id > 0;
			bool DetailOk = ItemsTable.CanSave;
			buttonOk.Sensitive = Dateok && DetailOk && ((WorkerOk && comboOperation.Active == 0) || (ObjectOk && comboOperation.Active == 1) );
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			string sql;
			if(NewItem)
			{
				sql = "INSERT INTO stock_expense (operation, date, wear_card_id, object_id, user_id) " +
					"VALUES (@operation, @date, @wear_card_id, @object_id, @user_id)";
			}
			else
			{
				sql = "UPDATE stock_expense SET operation = @operation, date = @date, wear_card_id = @wear_card_id, object_id = @object_id " +
					"WHERE id = @id";
			}
			QSMain.CheckConnectionAlive ();
			logger.Info("Запись документа...");
			MySqlTransaction trans = QSMain.connectionDB.BeginTransaction();
			try 
			{
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);

				cmd.Parameters.AddWithValue("@id", Itemid);
				switch (comboOperation.Active) {
					case 0: cmd.Parameters.AddWithValue("@operation", "employee");
						break;
					case 1: cmd.Parameters.AddWithValue("@operation", "object");
						break;
				}
				cmd.Parameters.AddWithValue("@date", dateDoc.Date);
				cmd.Parameters.AddWithValue("@user_id", QSMain.User.id);
				cmd.Parameters.AddWithValue("@wear_card_id", DBWorks.ValueOrNull(Worker_id > 0, Worker_id));
				cmd.Parameters.AddWithValue("@object_id", DBWorks.ValueOrNull(Object_id > 0, Object_id));
				cmd.ExecuteNonQuery();
				if(NewItem)
					Itemid = (int) cmd.LastInsertedId;

				if(ItemsTable.SaveExpenseDetails(Itemid, trans))
					trans.Commit();
				else
					trans.Rollback();
				logger.Info("Ok");
				Respond (Gtk.ResponseType.Ok);
			} 
			catch (Exception ex) 
			{
				trans.Rollback();
				QSMain.ErrorMessageWithLog(this, "Ошибка записи документа!", logger, ex);
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
			WorkerSelect.SqlSelect = "SELECT id, last_name, first_name, patronymic_name FROM @tablename WHERE dismiss_date IS NULL";
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
			name = name.Trim();
			string[] Parts = name.Split(new char[] {' '});
			if(Parts.Length == 3)
				entryWorker.Text = String.Format("{0} {1}. {2}.", Parts[0], Parts[1][0], Parts[2][0]);
			else
				entryWorker.Text = String.Format("{0}", name);
			entryWorker.TooltipText = name;
			ItemsTable.WorkerId = Worker_id;
		}

		protected void OnItemsTableCanSaveStateChanged(object sender, EventArgs e)
		{
			TestCanSave();
		}

		protected void OnComboOperationChanged(object sender, EventArgs e)
		{
			labelWorker.Visible = comboOperation.Active == 0;
			hboxWorker.Visible = comboOperation.Active == 0;
			labelObject.Visible = comboOperation.Active == 1;
			hboxObject.Visible = comboOperation.Active == 1;
			switch (comboOperation.Active)
			{
				case 0:
					ItemsTable.Operation = Operations.Employee;
					break;
				case 1:
					ItemsTable.Operation = Operations.Object;
					break;
			}
			TestCanSave();
		}

		protected void OnButtonEditObjectClicked(object sender, EventArgs e)
		{
			Reference ObjectSelect = new Reference();
			ObjectSelect.SetMode(false, true, true, true, false);
			ObjectSelect.FillList("objects","объект", "Объекты");
			ObjectSelect.Show();
			int result = ObjectSelect.Run();
			if((ResponseType)result == ResponseType.Ok)
			{
				SetObject(ObjectSelect.SelectedID);
			}
			ObjectSelect.Destroy();
			TestCanSave();
		}

		public void SetObject(int id)
		{
			string sql = "SELECT name, address FROM objects WHERE id = @id";
			QSMain.CheckConnectionAlive ();
			try
			{
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@id", id);
				using( MySqlDataReader rdr = cmd.ExecuteReader())
				{
					rdr.Read();
					entryObject.Text = rdr.GetString("name");
					entryObject.TooltipText = String.Format("{0}\n{1}", DBWorks.GetString(rdr, "name", ""), DBWorks.GetString(rdr, "address", ""));
					Object_id = id;
				}
				ItemsTable.ObjectId = Object_id;
			}
			catch (Exception ex) 
			{
				QSMain.ErrorMessageWithLog(this, "Ошибка чтения объекта!", logger, ex);
			}
		}
	}
}

