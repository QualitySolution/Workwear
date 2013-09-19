using System;
using System.IO;
using MySql.Data.MySqlClient;
using QSProjectsLib;
using Gtk;
using Gdk;

namespace workwear
{
	public partial class WearCard : Gtk.Dialog
	{
		public bool NewItem;
		int Itemid, Leader_id, Object_id;
		byte[] PhotoFile;
		bool ImageChanged = false;
		private Gtk.ListStore ItemsListStore;

		enum ReturnType{none, returnwear, writeoff};
		
		public WearCard()
		{
			this.Build();
			
			ComboWorks.ComboFillReference(comboPost, "posts", 2);
			ComboWorks.ComboFillUniqueValue(comboentryWearSize, "wear_cards", "wear_size");
			//Устанавливаем последовательность заполнения
			table1.FocusChain = new Widget[]
			{entryLastName, entryFirstName, entryPatronymic, dateHire, dateDismiss, 
			comboSex, spinGrowth, comboentryWearSize, comboPost, hboxLeader, hboxObject
			};

			//Создаем таблицу "материальных ценностей"
			ItemsListStore = new Gtk.ListStore (typeof (long), //0 in row id
			                                    typeof (int), //1 nomenclature id
			                                    typeof (string),//2 nomenclature name
			                                    typeof (string), //3 type nomenclature
			                                    typeof (string), // 4 nomenclature number
			                                    typeof (int), //5 income quantity
			                                    typeof (string), // 6 life
			                                    typeof (string), //7 units
			                                    typeof (string), // 8 income date
			                                    typeof (decimal), // 9 cost
			                                    typeof (string), // 10 tn number
			                                    // ----- выдача
			                                    typeof (long), // 11 out row id
			                                    typeof(string), // 12 out date
			                                    typeof(int), // 13 out quantity
			                                    typeof(ReturnType), // 14 type write off
			                                    typeof(string) // 15 life
			                                    );

			Gtk.CellRendererText CellQuantityIn = new CellRendererText();
			Gtk.CellRendererText CellQuantityOut = new CellRendererText();
			Gtk.CellRendererText CellCost = new CellRendererText();

			treeviewWear.AppendColumn ("Наименование", new Gtk.CellRendererText (), "text", 2);
			treeviewWear.AppendColumn ("Дата", new Gtk.CellRendererText (), "text", 8);
			treeviewWear.AppendColumn ("Кол-во", CellQuantityIn, RenderQuantityInColumn);
			treeviewWear.AppendColumn ("% годности", new Gtk.CellRendererText (), "text", 6);
			treeviewWear.AppendColumn ("Стоимость", CellCost, RenderCostColumn);
			treeviewWear.AppendColumn ("№ТН", new Gtk.CellRendererText (), "text", 10);

			treeviewWear.AppendColumn ("Дата", new Gtk.CellRendererText (), "text", 12);
			treeviewWear.AppendColumn ("Кол-во", CellQuantityOut, RenderQuantityOutColumn);
			treeviewWear.AppendColumn ("% годности", new Gtk.CellRendererText (), "text", 15);

			treeviewWear.Model = ItemsListStore;
			treeviewWear.ShowAll();
		}
		
		public void Fill(int id)
		{
			Itemid = id;
			NewItem = false;
			
			MainClass.StatusMessage(String.Format("Запрос карточки №{0}...", id));
			string sql = "SELECT wear_cards.*, leaders.name as leader, objects.name as object, objects.address as address, users.name as user " +
			"FROM wear_cards " +
			"LEFT JOIN leaders ON leaders.id = wear_cards.leader_id " +
			"LEFT JOIN objects ON objects.id = wear_cards.object_id " +
			"LEFT JOIN users ON wear_cards.user_id = users.id " +
			"WHERE wear_cards.id = @id";
			try
			{
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				
				cmd.Parameters.AddWithValue("@id", id);

				using(MySqlDataReader rdr = cmd.ExecuteReader())
				{		
					TreeIter iter;
					
					rdr.Read();
					
					labelId.LabelProp = rdr["id"].ToString();
					labelUser.LabelProp = DBWorks.GetString(rdr, "user", "не указан");
					entryLastName.Text = rdr["last_name"].ToString();
					entryFirstName.Text = rdr["first_name"].ToString();
					entryPatronymic.Text = rdr["patronymic_name"].ToString();
					comboentryWearSize.Entry.Text = rdr["wear_size"].ToString();
					spinGrowth.Value = rdr.GetDouble("growth");
					if(rdr["post_id"] != DBNull.Value)
					{
						ListStoreWorks.SearchListStore((ListStore)comboPost.Model, rdr.GetInt32("post_id"), out iter);
						comboPost.SetActiveIter(iter);
					}
					if(rdr["leader_id"] != DBNull.Value)
					{
						Leader_id = rdr.GetInt32("leader_id");
						entryLeader.Text = rdr.GetString("leader");
						entryLeader.TooltipText = rdr.GetString("leader");
					}
					else
					{
						Leader_id = -1;
					}
					if(rdr["hire_date"] != DBNull.Value)
					{
						dateHire.Date = rdr.GetDateTime("hire_date");
					}
					if(rdr["dismiss_date"] != DBNull.Value)
					{
						dateDismiss.Date = rdr.GetDateTime("dismiss_date");
					}
					switch (rdr["sex"].ToString()) {
					case "M": comboSex.Active = 0;
						break;
					case "F": comboSex.Active = 1;
						break;
					}
					if(rdr["object_id"] != DBNull.Value)
					{
						Object_id = rdr.GetInt32("object_id");
						labelObjectName.LabelProp = rdr.GetString("object");
						labelObjectAddress.LabelProp = rdr.GetString("address");
					}
					else
					{
						Object_id = -1;
					}
					if(rdr["photo"] != DBNull.Value)
					{
						PhotoFile = new byte[rdr.GetInt64("photo_size")];
						rdr.GetBytes(rdr.GetOrdinal("photo"), 0, PhotoFile, 0, rdr.GetInt32("photo_size"));
						ImageChanged = false;
						ReadImage();
						buttonSavePhoto.Sensitive = true;
					}
				}
				MainClass.StatusMessage("Ok");
				UpdateWear();
				buttonGiveWear.Sensitive = true;
				buttonReturnWear.Sensitive = true;
				buttonWriteOffWear.Sensitive = true;
				this.Title = entryLastName.Text + " " + entryFirstName.Text;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка получения карточки!");
				QSMain.ErrorMessage(this, ex);
				this.Respond(Gtk.ResponseType.Reject);
			}
			TestCanSave();
		}

		protected void TestCanSave ()
		{
			//bool Nameok = entryName.Text != "";
			buttonOk.Sensitive = true;
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			string sql;
			if(NewItem)
			{
				sql = "INSERT INTO wear_cards (last_name, first_name, patronymic_name, " +
					"wear_size, growth, post_id, leader_id, hire_date, dismiss_date, sex, object_id, user_id) " +
						"VALUES (@last_name, @first_name, @patronymic_name, @wear_size, @growth, " +
						"@post_id, @leader_id, @hire_date, @dismiss_date, @sex, @object_id, @user_id)";
			}
			else
			{
				sql = "UPDATE wear_cards SET last_name = @last_name, first_name = @first_name, " +
					"patronymic_name = @patronymic_name, wear_size = @wear_size, growth = @growth, " +
					"post_id = @post_id, leader_id = @leader_id, hire_date = @hire_date, " +
					"dismiss_date = @dismiss_date, sex = @sex, object_id = @object_id " +
					"WHERE id = @id";
			}
			MainClass.StatusMessage("Запись карточки...");
			MySqlTransaction trans = QSMain.connectionDB.BeginTransaction();
			try 
			{
				TreeIter iter;
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);
				
				cmd.Parameters.AddWithValue("@id", Itemid);
				cmd.Parameters.AddWithValue("@last_name", entryLastName.Text);
				cmd.Parameters.AddWithValue("@first_name", entryFirstName.Text);
				cmd.Parameters.AddWithValue("@patronymic_name", entryPatronymic.Text);
				cmd.Parameters.AddWithValue("@user_id", QSMain.User.id);
				if(comboentryWearSize.Entry.Text != "")
					cmd.Parameters.AddWithValue("@wear_size", comboentryWearSize.Entry.Text);
				else 
					cmd.Parameters.AddWithValue("@wear_size", DBNull.Value);
				cmd.Parameters.AddWithValue("@growth", spinGrowth.ValueAsInt);
				if (Leader_id > 0)
					cmd.Parameters.AddWithValue("@leader_id", Leader_id);
				else
					cmd.Parameters.AddWithValue("@leader_id", DBNull.Value);
				if(comboPost.Active > 0 && comboPost.GetActiveIter(out iter))
					cmd.Parameters.AddWithValue("@post_id", comboPost.Model.GetValue(iter,1));
				else
					cmd.Parameters.AddWithValue("@post_id", DBNull.Value);
				if(dateHire.IsEmpty)
					cmd.Parameters.AddWithValue("@hire_date", DBNull.Value);
				else
					cmd.Parameters.AddWithValue("@hire_date", dateHire.Date);
				if(dateDismiss.IsEmpty)
					cmd.Parameters.AddWithValue("@dismiss_date", DBNull.Value);
				else
					cmd.Parameters.AddWithValue("@dismiss_date", dateDismiss.Date);
				switch (comboSex.Active) {
					case 0: cmd.Parameters.AddWithValue("@sex", "M");
						break;
					case 1: cmd.Parameters.AddWithValue("@sex", "F");
						break;
					default: cmd.Parameters.AddWithValue("@sex", DBNull.Value);
						break;
				}
				if(Object_id > 0)
					cmd.Parameters.AddWithValue("@object_id", Object_id);
				else
					cmd.Parameters.AddWithValue("@object_id", DBNull.Value);

				cmd.ExecuteNonQuery();
				if(NewItem)
					Itemid = (int) cmd.LastInsertedId;

				if(PhotoFile != null && ImageChanged)
				{
					MainClass.StatusMessage("Запись фотографии в базу...");
					sql = "UPDATE wear_cards SET photo = @photo, photo_size = @photo_size WHERE id = @id";
					cmd = new MySqlCommand(sql, QSMain.connectionDB, trans);

					cmd.Parameters.AddWithValue("@id", Itemid);
					cmd.Parameters.AddWithValue("@photo_size", PhotoFile.LongLength);
					cmd.Parameters.AddWithValue("@photo", PhotoFile);
					cmd.ExecuteNonQuery();
				}
				trans.Commit();
				MainClass.StatusMessage("Ok");
				Respond (Gtk.ResponseType.Ok);
			} 
			catch (MySqlException ex) 
			{
				trans.Rollback();
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка записи карточки!");
				if(ex.Number == 1153)
				{
					string Text = "Превышен максимальный размер пакета для передачи на сервер базы данных. " +
						"Это значение настраивается на сервере, по умолчанию для MySQL оно равняется 1Мб. " +
						"Максимальный размер фотографии поддерживаемый программой составляет 16Мб, мы рекомендуем " +
						"установить в настройках сервера параметр <b>max_allowed_packet=16M</b>. Подробнее о настройке здесь " +
						"http://dev.mysql.com/doc/refman/5.6/en/packet-too-large.html";
					MessageDialog md = new MessageDialog( this, DialogFlags.Modal,
					                                     MessageType.Error, 
					                                     ButtonsType.Ok, Text);
					md.Run ();
					md.Destroy();
				}
				else
					QSMain.ErrorMessage(this,ex);
			}
		}

		protected void OnButtonLeaderClearClicked(object sender, EventArgs e)
		{
			Leader_id = -1;
			entryLeader.Text = "";
			entryLeader.TooltipText = "";
		}

		protected void OnButtonObjectClearClicked(object sender, EventArgs e)
		{
			Object_id = -1;
			labelObjectName.LabelProp = "---";
			labelObjectAddress.LabelProp = "---";
		}

		protected void OnButtonLeaderClicked(object sender, EventArgs e)
		{
			Reference LeaderSelect = new Reference();
			LeaderSelect.SetMode(true,true,true,true,false);
			LeaderSelect.FillList("leaders","Руководитель", "Наши руководители");
			LeaderSelect.Show();
			int result = LeaderSelect.Run();
			if((ResponseType)result == ResponseType.Ok)
			{
				Leader_id = LeaderSelect.SelectedID;
				entryLeader.Text = LeaderSelect.SelectedName;
				entryLeader.TooltipText = LeaderSelect.SelectedName;
			}
			LeaderSelect.Destroy();
		}

		protected void OnButtonObjectClicked(object sender, EventArgs e)
		{
			Reference ObjectSelect = new Reference();
			ObjectSelect.SetMode(false,true,true,true,false);
			ObjectSelect.FillList("objects","Объект", "Объекты");
			ObjectSelect.Show();
			int result = ObjectSelect.Run();
			if((ResponseType)result == ResponseType.Ok)
			{
				Object_id = ObjectSelect.SelectedID;
				labelObjectName.LabelProp = ObjectSelect.SelectedName;
				string sql = "SELECT address FROM objects WHERE id = @id";
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue("@id", Object_id);
				labelObjectAddress.LabelProp = cmd.ExecuteScalar().ToString();
			}
			ObjectSelect.Destroy();
		}

		void ReadImage()
		{
			int MaxWidth = imagePhoto.Allocation.Size.Width;
			int MaxHeight = imagePhoto.Allocation.Size.Height;
			Console.WriteLine("W: {0} H: {1}", MaxWidth, MaxHeight);

			Pixbuf pix = new Pixbuf(PhotoFile);
			double vratio = (double) MaxHeight / pix.Height;
			double hratio = (double) MaxWidth / pix.Width;
			int Heigth, Width;
			if(vratio < hratio)
			{
				Heigth = MaxHeight;
				Width = Convert.ToInt32(pix.Width * vratio);
			}
			else 
			{
				Heigth = Convert.ToInt32(pix.Height * hratio);
				Width = MaxWidth;
			}
			imagePhoto.Pixbuf = pix.ScaleSimple (Width,
			                                   Heigth,
			                                   InterpType.Bilinear);
		}

		protected void OnButtonLoadPhotoClicked(object sender, EventArgs e)
		{
			FileChooserDialog Chooser = new FileChooserDialog("Выберите фото для загрузки...", 
			                                                  this,
			                                                  FileChooserAction.Open,
			                                                  "Отмена", ResponseType.Cancel,
			                                                  "Загрузить", ResponseType.Accept );

			FileFilter Filter = new FileFilter();
			Filter.AddPixbufFormats ();
			Filter.Name = "Все изображения";
			Chooser.AddFilter(Filter);

			if((ResponseType) Chooser.Run () == ResponseType.Accept)
			{
				Chooser.Hide();
				MainClass.StatusMessage("Загрузка фотографии...");

				FileStream fs = new FileStream(Chooser.Filename, FileMode.Open, FileAccess.Read);
				if(Chooser.Filename.ToLower().EndsWith (".jpg"))
				{
					using (MemoryStream ms = new MemoryStream())
					{
						fs.CopyTo(ms);
						PhotoFile = ms.ToArray();
					}
				}
				else 
				{
					MainClass.StatusMessage("Конвертация в jpg ...");
					Pixbuf image = new Pixbuf(fs);
					PhotoFile = image.SaveToBuffer("jpeg");
				}
				fs.Close();
				ImageChanged = true;
				ReadImage();
				buttonSavePhoto.Sensitive = true;
				MainClass.StatusMessage("Ok");
			}
			Chooser.Destroy ();
		}

		protected void OnButtonSavePhotoClicked(object sender, EventArgs e)
		{
			FileChooserDialog fc=
				new FileChooserDialog("Укажите файл для сохранения фотографии",
				                      this,
				                      FileChooserAction.Save,
				                      "Отмена",ResponseType.Cancel,
				                      "Сохранить",ResponseType.Accept);
			fc.CurrentName = entryLastName.Text + " " + entryFirstName.Text + " " + entryPatronymic.Text + ".jpg";
			fc.Show(); 
			if(fc.Run() == (int) ResponseType.Accept)
			{
				fc.Hide();
				FileStream fs = new FileStream(fc.Filename, FileMode.Create, FileAccess.Write);
				fs.Write(PhotoFile, 0, PhotoFile.Length);
				fs.Close();
			}
			fc.Destroy();
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

		private void UpdateWear()
		{
			MainClass.StatusMessage("Запрос спецодежды по работнику...");
			try
			{
				string sql = "SELECT stock_expense_detail.id as idin, stock_expense_detail.nomenclature_id, stock_expense_detail.quantity as quantityin, stock_income_detail.cost, " +
					"nomenclature.name, units.name as unit, stock_expense.date as datein, stock_income_detail.life_percent as lifein, stock_income.number as tnnumber, " +
					"spent.*  FROM stock_expense_detail " +
					"LEFT JOIN " +
					"(SELECT stock_income_detail.stock_expense_detail_id as idin, stock_income_detail.id as income_id, NULL as write_off_id, " +
					"stock_income_detail.quantity as count, stock_income.date as dateout, stock_income_detail.life_percent as lifeout  FROM stock_income_detail " +
					"LEFT JOIN stock_income ON stock_income.id = stock_income_detail.stock_income_id WHERE stock_expense_detail_id IS NOT NULL " +
					"UNION ALL " +
					"SELECT stock_write_off_detail.stock_expense_detail_id as idin, NULL as income_id, stock_write_off_detail.id as write_off_id, " +
					"stock_write_off_detail.quantity as count, stock_write_off.date as dateout, NULL as lifeout FROM stock_write_off_detail " +
					"LEFT JOIN stock_write_off ON stock_write_off_detail.stock_write_off_id = stock_write_off.id " +
					"WHERE stock_expense_detail_id IS NOT NULL" +
					") as spent ON spent.idin = stock_expense_detail.id " +
					"LEFT JOIN nomenclature ON nomenclature.id = stock_expense_detail.nomenclature_id " +
					"LEFT JOIN units ON nomenclature.units_id = units.id " +
					"LEFT JOIN stock_expense ON stock_expense.id = stock_expense_detail.stock_expense_id " +
					"LEFT JOIN stock_income_detail ON stock_income_detail.id = stock_expense_detail.stock_income_detail_id " +
					"LEFT JOIN stock_income ON stock_income.id = stock_income_detail.stock_income_id " +
					"WHERE stock_expense.wear_card_id = @id ";
				if(!checkShowHistory.Active)
					sql += " AND (spent.count IS NULL OR spent.count < stock_expense_detail.quantity )";
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue ("@id", Itemid);
				MySqlDataReader rdr = cmd.ExecuteReader();

				ItemsListStore.Clear();
				long LastId = -1;
				while (rdr.Read())
				{
					long OutRowId;
					string LifeOut, DateOut;
					ReturnType Mode;

					if(rdr["write_off_id"] != DBNull.Value)
					{
						OutRowId = rdr.GetInt64("write_off_id");
						LifeOut = "списано";
						Mode = ReturnType.writeoff;
						DateOut = String.Format ("{0:d}", rdr.GetDateTime ("dateout"));
					}
					else if(rdr["income_id"] != DBNull.Value)
					{
						OutRowId = rdr.GetInt64("income_id");
						LifeOut = String.Format ("{0:P0}", rdr.GetDecimal("lifeout"));
						Mode = ReturnType.returnwear;
						DateOut = String.Format ("{0:d}", rdr.GetDateTime ("dateout"));
					}
					else
					{
						LifeOut = "";
						Mode = ReturnType.none;
						OutRowId = -1;
						DateOut = "";
					}
					if(LastId == rdr.GetInt64("idin"))
					{
						ItemsListStore.AppendValues(rdr.GetInt64("idin"),
						                            rdr.GetInt32("nomenclature_id"),
						                            string.Empty,
						                            string.Empty,
						                            string.Empty,
						                            -1,
						                            string.Empty,
						                            rdr["unit"].ToString(),
						                            string.Empty,
						                            -1m,
						                            string.Empty,
						                            OutRowId,
						                            DateOut,
						                            DBWorks.GetInt(rdr, "count", 0),
						                            Mode,
						                            LifeOut);
					}
					else
					{
					ItemsListStore.AppendValues(rdr.GetInt64("idin"),
					                            rdr.GetInt32("nomenclature_id"),
					                            rdr.GetString ("name"),
						                        string.Empty,
						                        string.Empty,
					                            rdr.GetInt32("quantityin"),
						                        String.Format ("{0:P0}", rdr.GetDecimal("lifein")),
					                            rdr["unit"].ToString(),
						                        String.Format ("{0:d}", rdr.GetDateTime ("datein")),
					                            DBWorks.GetDecimal(rdr, "cost", -1),
					                            rdr["tnnumber"].ToString(),
					                            OutRowId,
					                            DateOut,
					                            DBWorks.GetInt(rdr, "count", 0),
					                            Mode,
					                            LifeOut);
						LastId = rdr.GetInt64("idin");
					}
				}
				rdr.Close();
				MainClass.StatusMessage("Ok");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка получения спецодежды по работнику!");
			}

		}

		protected void OnCheckShowHistoryClicked(object sender, EventArgs e)
		{
			UpdateWear();
		}

		protected void OnButtonGiveWearClicked(object sender, EventArgs e)
		{
			ExpenseDoc winExpense = new ExpenseDoc();
			winExpense.NewItem = true;
			winExpense.SetWorker(Itemid, String.Format("{0} {1} {2}", entryLastName.Text, entryFirstName.Text, entryPatronymic.Text));
			winExpense.Show();
			winExpense.Run();
			winExpense.Destroy();
			UpdateWear();
		}

		protected void OnButtonReturnWearClicked(object sender, EventArgs e)
		{
			IncomeDoc winIncome = new IncomeDoc();
			winIncome.NewItem = true;
			winIncome.Operation = IncomeDoc.Operations.Return;
			winIncome.SetWorker(Itemid, String.Format("{0} {1} {2}", entryLastName.Text, entryFirstName.Text, entryPatronymic.Text));
			winIncome.Show();
			winIncome.Run();
			winIncome.Destroy();
			UpdateWear();
		}

		protected void OnButtonWriteOffWearClicked(object sender, EventArgs e)
		{
			WriteOffDoc winWriteOff = new WriteOffDoc();
			winWriteOff.NewItem = true;
			winWriteOff.CurrentWorkerId = Itemid;
			winWriteOff.Show();
			winWriteOff.Run();
			winWriteOff.Destroy();
			UpdateWear();
		}

	}
}
