using System;
using System.IO;
using Gtk;
using MySql.Data.MySqlClient;
using NLog;
using QSProjectsLib;
using System.Collections.Generic;
using workwear.DTO;
using workwear.Domain;
using QSOrmProject;
using QSValidation;
using workwear.Repository;

namespace workwear
{
	public partial class EmployeeCardDlg : FakeTDIEntityGtkDialogBase<EmployeeCard>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		List<EmployeeCardItems> listedItems;
		List<EmployeeCardMovements> Movements;
		bool IsShowedMovementsTable = false;

		public EmployeeCardDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<EmployeeCard> ();
			Entity.CreatedbyUser = UserRepository.GetMyUser (UoW);
			ConfigureDlg ();
		}

		private void ConfigureDlg()
		{
			ycomboWearStd.ItemsEnum = typeof(SizeStandartWear);
			ycomboShoesStd.ItemsEnum = typeof(SizeStandartShoes);
			ycomboHeaddress.ItemsEnum = typeof(SizeStandartHeaddress);
			ycomboGloves.ItemsEnum = typeof(SizeStandartGloves);

			entryFirstName.Binding.AddBinding (Entity, e => e.FirstName, w => w.Text).InitializeFromSource ();
			entryLastName.Binding.AddBinding (Entity, e => e.LastName, w => w.Text).InitializeFromSource ();
			entryPatronymic.Binding.AddBinding (Entity, e => e.Patronymic, w => w.Text).InitializeFromSource ();

			dateHire.Binding.AddBinding (Entity, e => e.HireDate, w => w.DateOrNull).InitializeFromSource ();
			dateDismiss.Binding.AddBinding (Entity, e => e.DismissDate, w => w.DateOrNull).InitializeFromSource ();

			yentryPost.SubjectType = typeof(Post);
			yentryPost.Binding.AddBinding (Entity, e => e.Post, w => w.Subject).InitializeFromSource ();
			yentryLeader.SubjectType = typeof(Leader);
			yentryLeader.Binding.AddBinding (Entity, e => e.Leader, w => w.Subject).InitializeFromSource ();
			yentryObject.SubjectType = typeof(Facility);
			yentryObject.Binding.AddBinding (Entity, e => e.Facility, w => w.Subject).InitializeFromSource ();

			comboSex.ItemsEnum = typeof(Sex);
			comboSex.Binding.AddBinding (Entity, e => e.Sex, w => w.SelectedItem).InitializeFromSource ();

			yimagePhoto.Binding.AddBinding (Entity, e => e.Photo, w => w.ImageFile).InitializeFromSource ();

			//Устанавливаем последовательность заполнения
			table1.FocusChain = new Widget[] {entryLastName, entryFirstName, entryPatronymic, dateHire, dateDismiss, 
				comboSex, yentryPost, yentryLeader, yentryObject
			};

			ytreeListedItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<EmployeeCardItems> ()
				.AddColumn ("Наименование").AddTextRenderer (e => e.ItemTypeName)
				.AddColumn ("Количество").AddTextRenderer (e => e.AmountText)
				.AddColumn ("Средняя стоимость").AddTextRenderer (e => e.AvgCostText)
				.Finish ();
			ytreeListedItems.ShowAll ();

			treeviewMovements.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<EmployeeCardMovements> ()
				.AddColumn ("Дата").AddTextRenderer (e => e.Date.ToShortDateString ())
				.AddColumn ("Документ").AddTextRenderer (e => e.DocumentName)
				.AddColumn ("Номенклатура").AddTextRenderer (e => e.NomenclatureName)
				.AddColumn ("% годности").AddTextRenderer (e => e.LifeText)
				.AddColumn ("Стоимость").AddTextRenderer (e => e.CostText )
				.AddColumn ("Получено").AddTextRenderer (e => e.AmountReceivedText)
				.AddColumn ("Сдано\\списано").AddTextRenderer (e => e.AmountReturnedText)
				.Finish ();
			treeviewMovements.ShowAll ();
		}

		public EmployeeCardDlg (int id)
		{
			this.Build ();

			logger.Info ("Запрос карточки №{0}...", id);
			try 
			{
				UoWGeneric = UnitOfWorkFactory.CreateForRoot<EmployeeCard> (id);

				entryId.Text = String.IsNullOrWhiteSpace (Entity.CardNumber) ? Entity.Id.ToString () : Entity.CardNumber;
				checkAuto.Active = String.IsNullOrWhiteSpace (Entity.CardNumber);

				labelUser.LabelProp = Entity.CreatedbyUser != null ? Entity.CreatedbyUser.Name : "не указан";

				logger.Info ("Ok");
				UpdateMovements ();
				UpdateListedItems ();
				buttonGiveWear.Sensitive = true;
				buttonReturnWear.Sensitive = true;
				buttonWriteOffWear.Sensitive = true;
				buttonPrint.Sensitive = true;
			} catch (Exception ex) {
				QSMain.ErrorMessageWithLog (this, "Ошибка получения карточки!", logger, ex);
				this.Respond (Gtk.ResponseType.Reject);
			}

			ConfigureDlg ();
			TestCanSave ();
		}

		protected void TestCanSave ()
		{
			//bool Nameok = entryName.Text != "";
			bool Numberok = checkAuto.Active || (!String.IsNullOrWhiteSpace(entryId.Text) && entryId.Text != "авто") ;
			buttonOk.Sensitive = Numberok;
		}

		public override bool Save()
		{
			logger.Info ("Запись карточки...");
			var valid = new QSValidator<EmployeeCard> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			try {
				UoWGeneric.Save ();
			} catch (Exception ex) {
				QSMain.ErrorMessageWithLog (this, "Не удалось записать сотрудника.", logger, ex);
				return false;
			}
			logger.Info ("Ok");
			return true;
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			if(Save ())
				Respond (Gtk.ResponseType.Ok);
		}

		private void UpdateMovements ()
		{
			if (!IsShowedMovementsTable)
				return;
			QSMain.CheckConnectionAlive ();
			logger.Info ("Запрос движений по работнику...");
			try {
				string sql = "SELECT movements.*, nomenclature.name as nomenclature_name, units.name as unit " +
					"FROM (" +
					"SELECT stock_expense.date, stock_expense_detail.stock_expense_id, NULL as stock_income_id, NULL as stock_write_off_id, " +
					"stock_expense_detail.nomenclature_id, stock_expense_detail.quantity, stock_income_detail.life_percent, stock_income_detail.cost " +
					"FROM stock_expense_detail " +
					"LEFT JOIN stock_expense ON stock_expense.id = stock_expense_detail.stock_expense_id " +
					"LEFT JOIN stock_income_detail ON stock_income_detail.id = stock_expense_detail.stock_income_detail_id " +
					"WHERE stock_expense.wear_card_id = @id " +
					"UNION ALL " +
					"SELECT stock_income.date, NULL as stock_expense_id, stock_income_detail.stock_income_id, NULL as stock_write_off_id, " +
					"stock_income_detail.nomenclature_id, stock_income_detail.quantity, stock_income_detail.life_percent, stock_income_detail.cost " +
					"FROM stock_income_detail " +
					"LEFT JOIN stock_income ON stock_income.id = stock_income_detail.stock_income_id " +
					"WHERE stock_income.wear_card_id = @id " +
					"UNION ALL " +
					"SELECT stock_write_off.date, NULL as stock_expense_id, NULL as stock_income_id, stock_write_off_detail.stock_write_off_id, " +
					"stock_write_off_detail.nomenclature_id, stock_write_off_detail.quantity, NULL as life_percent, NULL as cost " +
					"FROM stock_write_off_detail " +
					"LEFT JOIN stock_write_off ON stock_write_off.id = stock_write_off_detail.stock_write_off_id " +
					"LEFT JOIN stock_expense_detail ON stock_write_off_detail.stock_expense_detail_id = stock_expense_detail.id " +
					"LEFT JOIN stock_expense ON stock_expense.id = stock_expense_detail.stock_expense_id " +
					"WHERE stock_expense.wear_card_id = @id " +
					") as movements " +
					"LEFT JOIN nomenclature ON nomenclature.id = movements.nomenclature_id " +
					"LEFT JOIN item_types ON item_types.id = nomenclature.type_id " +
					"LEFT JOIN units ON item_types.units_id = units.id " +
					"ORDER BY movements.date, movements.stock_write_off_id, movements.stock_income_id, movements.stock_expense_id";
				MySqlCommand cmd = new MySqlCommand (sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue ("@id", Entity.Id);

				Movements = new List<EmployeeCardMovements>();

				using( MySqlDataReader rdr = cmd.ExecuteReader ())
				{
					while (rdr.Read ()) {
						var move = new EmployeeCardMovements{
							Date = rdr.GetDateTime ("date"),
							NomenclatureName = rdr.GetString ("nomenclature_name"),
							Cost = DBWorks.GetDecimal (rdr, "cost"),
							Life = DBWorks.GetDecimal (rdr, "life_percent"),
							UnitsName = DBWorks.GetString (rdr, "unit", String.Empty)
						};

						if(rdr["stock_expense_id"] != DBNull.Value)
						{
							move.MovementType = MovementType.Received;
							move.AmountReceived = rdr.GetInt32 ("quantity");
							move.DocumentId = rdr.GetInt32 ("stock_expense_id");
						}

						if(rdr["stock_income_id"] != DBNull.Value)
						{
							move.MovementType = MovementType.Returned;
							move.AmountReturned = rdr.GetInt32 ("quantity");
							move.DocumentId = rdr.GetInt32 ("stock_income_id");
						}

						if(rdr["stock_write_off_id"] != DBNull.Value)
						{
							move.MovementType = MovementType.Writeoff;
							move.AmountReturned = rdr.GetInt32 ("quantity");
							move.DocumentId = rdr.GetInt32 ("stock_write_off_id");
						}

						Movements.Add (move);
					}
				}
				treeviewMovements.ItemsDataSource = Movements;
				logger.Info ("Ok");
			} catch (Exception ex) {
				QSMain.ErrorMessageWithLog(this, "Ошибка получения движений по работнику!", logger, ex);
			}
		}

		private void UpdateListedItems ()
		{
			QSMain.CheckConnectionAlive ();
			logger.Info ("Запрос числящегося за сотрудником...");
			try {
				string sql = "SELECT item_types.id as item_types_id, " +
					"SUM(stock_expense_detail.quantity - ifnull(spent.count, 0)) as quantity, " +
					"item_types.name, units.name as unit, " +
					"SUM(stock_income_detail.cost * (stock_expense_detail.quantity - ifnull(spent.count, 0)))/SUM(stock_expense_detail.quantity - ifnull(spent.count, 0)) as avgcost " +
					"FROM stock_expense_detail " +
					"LEFT JOIN " +
					"(SELECT stock_income_detail.stock_expense_detail_id as idin, stock_income_detail.id as income_id, NULL as write_off_id, stock_income_detail.quantity as count " +
					"FROM stock_income_detail " +
					"LEFT JOIN stock_income ON stock_income.id = stock_income_detail.stock_income_id " +
					"WHERE stock_expense_detail_id IS NOT NULL " +
					"UNION ALL " +
					"SELECT stock_write_off_detail.stock_expense_detail_id as idin, NULL as income_id, stock_write_off_detail.id as write_off_id, stock_write_off_detail.quantity as count FROM stock_write_off_detail " +
					"LEFT JOIN stock_write_off ON stock_write_off_detail.stock_write_off_id = stock_write_off.id " +
					"WHERE stock_expense_detail_id IS NOT NULL" +
					") as spent ON spent.idin = stock_expense_detail.id " +
					"LEFT JOIN nomenclature ON nomenclature.id = stock_expense_detail.nomenclature_id " +
					"LEFT JOIN item_types ON item_types.id = nomenclature.type_id " +
					"LEFT JOIN units ON item_types.units_id = units.id " +
					"LEFT JOIN stock_expense ON stock_expense.id = stock_expense_detail.stock_expense_id " +
					"LEFT JOIN stock_income_detail ON stock_income_detail.id = stock_expense_detail.stock_income_detail_id " +
					"WHERE stock_expense.wear_card_id = @id AND (spent.count IS NULL OR spent.count < stock_expense_detail.quantity ) " +
					"GROUP BY nomenclature.type_id";
				MySqlCommand cmd = new MySqlCommand (sql, QSMain.connectionDB);
				cmd.Parameters.AddWithValue ("@id", Entity.Id);

				listedItems = new List<EmployeeCardItems>();

				using(MySqlDataReader rdr = cmd.ExecuteReader ())
				{
					while (rdr.Read ()) {
						listedItems.Add (new EmployeeCardItems{
							ItemTypeId = rdr.GetInt32 ("item_types_id"),
							ItemTypeName = rdr.GetString ("name"),
							UnitsName = DBWorks.GetString (rdr, "unit", String.Empty),
							Amount = rdr.GetInt32 ("quantity"),
							AvgCost = rdr.GetDecimal ("avgcost")
						});
					}
				}
				ytreeListedItems.ItemsDataSource = listedItems;
				logger.Info ("Ok");
			} catch (Exception ex) {
				QSMain.ErrorMessageWithLog(this, "Ошибка получения числящегося за сотрудником!", logger, ex);
			}

		}

		protected void OnButtonGiveWearClicked (object sender, EventArgs e)
		{
			ExpenseDoc winExpense = new ExpenseDoc ();
			winExpense.NewItem = true;
			winExpense.Operation = ExpenseDoc.Operations.Employee;
			winExpense.SetWorker (Entity.Id, String.Format ("{0} {1} {2}", entryLastName.Text, entryFirstName.Text, entryPatronymic.Text));
			winExpense.Show ();
			winExpense.Run ();
			winExpense.Destroy ();
			UpdateMovements ();
			UpdateListedItems ();
		}

		protected void OnButtonReturnWearClicked (object sender, EventArgs e)
		{
			IncomeDoc winIncome = new IncomeDoc ();
			winIncome.NewItem = true;
			winIncome.Operation = IncomeDoc.Operations.Return;
			winIncome.SetWorker (Entity.Id, String.Format ("{0} {1} {2}", entryLastName.Text, entryFirstName.Text, entryPatronymic.Text));
			winIncome.Show ();
			winIncome.Run ();
			winIncome.Destroy ();
			UpdateMovements ();
			UpdateListedItems ();
		}

		protected void OnButtonWriteOffWearClicked (object sender, EventArgs e)
		{
			WriteOffDoc winWriteOff = new WriteOffDoc ();
			winWriteOff.NewItem = true;
			winWriteOff.CurrentWorkerId = Entity.Id;
			winWriteOff.Show ();
			winWriteOff.Run ();
			winWriteOff.Destroy ();
			UpdateMovements ();
			UpdateListedItems ();
		}

		protected void OnButtonPrintClicked (object sender, EventArgs e)
		{
			string param = String.Format ("id={0}", Entity.Id);
			ViewReportExt.Run ("WearCard", param);
		}

		protected void OnCheckAutoToggled (object sender, EventArgs e)
		{
			entryId.Sensitive = !checkAuto.Active;
			if (checkAuto.Active)
				entryId.Text = Entity.Id.ToString ();
			TestCanSave();
		}

		protected void OnEntryIdChanged(object sender, EventArgs e)
		{
			Entity.CardNumber = checkAuto.Active ? entryId.Text : null;
			TestCanSave();
		}

		protected void OnNotebook1SwitchPage (object o, SwitchPageArgs args)
		{
			if(notebook1.CurrentPage == 1 && !IsShowedMovementsTable)
			{
				IsShowedMovementsTable = true;
				UpdateMovements ();
			}
		}

		protected void OnYentryObjectChanged (object sender, EventArgs e)
		{
			labelObjectAddress.LabelProp = Entity.Facility != null ? Entity.Facility.Address : "--//--";
		}

		protected void OnButtonSavePhotoClicked (object sender, EventArgs e)
		{
			FileChooserDialog fc =
				new FileChooserDialog ("Укажите файл для сохранения фотографии",
					(Gtk.Window)this,
					FileChooserAction.Save,
					"Отмена", ResponseType.Cancel,
					"Сохранить", ResponseType.Accept);
			fc.CurrentName = Entity.FullName + ".jpg";
			fc.Show (); 
			if (fc.Run () == (int)ResponseType.Accept) {
				fc.Hide ();
				FileStream fs = new FileStream (fc.Filename, FileMode.Create, FileAccess.Write);
				fs.Write (UoWGeneric.Root.Photo, 0, UoWGeneric.Root.Photo.Length);
				fs.Close ();
			}
			fc.Destroy ();
		}

		protected void OnButtonLoadPhotoClicked (object sender, EventArgs e)
		{
			FileChooserDialog Chooser = new FileChooserDialog ("Выберите фото для загрузки...", 
				(Gtk.Window)this,
				FileChooserAction.Open,
				"Отмена", ResponseType.Cancel,
				"Загрузить", ResponseType.Accept);

			FileFilter Filter = new FileFilter ();
			Filter.AddPixbufFormats ();
			Filter.Name = "Все изображения";
			Chooser.AddFilter (Filter);

			if ((ResponseType)Chooser.Run () == ResponseType.Accept) {
				Chooser.Hide ();
				logger.Info ("Загрузка фотографии...");

				FileStream fs = new FileStream (Chooser.Filename, FileMode.Open, FileAccess.Read);
				if (Chooser.Filename.ToLower ().EndsWith (".jpg")) {
					using (MemoryStream ms = new MemoryStream ()) {
						fs.CopyTo (ms);
						UoWGeneric.Root.Photo = ms.ToArray ();
					}
				} else {
					logger.Info ("Конвертация в jpg ...");
					Gdk.Pixbuf image = new Gdk.Pixbuf (fs);
					UoWGeneric.Root.Photo = image.SaveToBuffer ("jpeg");
				}
				fs.Close ();
				buttonSavePhoto.Sensitive = true;
				logger.Info ("Ok");
			}
			Chooser.Destroy ();
		}

		protected void OnYimagePhotoButtonPressEvent (object o, ButtonPressEventArgs args)
		{
			if (((Gdk.EventButton)args.Event).Type == Gdk.EventType.TwoButtonPress) {
				string filePath = System.IO.Path.Combine (System.IO.Path.GetTempPath (), "temp_img.jpg");
				FileStream fs = new FileStream (filePath, FileMode.Create, FileAccess.Write);
				fs.Write (UoWGeneric.Root.Photo, 0, UoWGeneric.Root.Photo.Length);
				fs.Close ();
				System.Diagnostics.Process.Start (filePath);
			}
		}
	}
}
