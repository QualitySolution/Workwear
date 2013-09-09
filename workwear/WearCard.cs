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
		}
		
		public void Fill(int id)
		{
			Itemid = id;
			NewItem = false;
			
			MainClass.StatusMessage(String.Format("Запрос карточки №{0}...", id));
			string sql = "SELECT wear_cards.*, leaders.name as leader, objects.name as object, objects.address as address " +
			"FROM wear_cards " +
			"LEFT JOIN leaders ON leaders.id = wear_cards.leader_id " +
			"LEFT JOIN objects ON objects.id = wear_cards.object_id " +
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
					"wear_size, growth, post_id, leader_id, hire_date, dismiss_date, sex, object_id) " +
						"VALUES (@last_name, @first_name, @patronymic_name, @wear_size, @growth, " +
						"@post_id, @leader_id, @hire_date, @dismiss_date, @sex, @object_id)";
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
			try 
			{
				TreeIter iter;
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				
				cmd.Parameters.AddWithValue("@id", Itemid);
				cmd.Parameters.AddWithValue("@last_name", entryLastName.Text);
				cmd.Parameters.AddWithValue("@first_name", entryFirstName.Text);
				cmd.Parameters.AddWithValue("@patronymic_name", entryPatronymic.Text);
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
					cmd = new MySqlCommand(sql, QSMain.connectionDB);

					cmd.Parameters.AddWithValue("@id", Itemid);
					cmd.Parameters.AddWithValue("@photo_size", PhotoFile.LongLength);
					cmd.Parameters.AddWithValue("@photo", PhotoFile);
					cmd.ExecuteNonQuery();
				}

				MainClass.StatusMessage("Ok");
				Respond (Gtk.ResponseType.Ok);
			} 
			catch (Exception ex) 
			{
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка записи карточки!");
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
	}
}

