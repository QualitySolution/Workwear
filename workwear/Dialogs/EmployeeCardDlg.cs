using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gamma.Utilities;
using Gtk;
using MySql.Data.MySqlClient;
using NHibernate;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using workwear.Domain;
using workwear.Domain.Stock;
using workwear.DTO;
using workwear.Measurements;
using workwear.Repository;

namespace workwear
{
	public partial class EmployeeCardDlg : FakeTDIEntityGtkDialogBase<EmployeeCard>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		List<EmployeeCardItems> listedItems;
		List<EmployeeCardMovements> Movements;
		bool IsShowedMovementsTable = false;
		bool IsShowedItemsTable = false;
		bool IsNomenclaturePickuped = false;

		public EmployeeCardDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<EmployeeCard> ();
			Entity.CreatedbyUser = UserRepository.GetMyUser (UoW);
			ConfigureDlg ();
		}

		private void ConfigureDlg()
		{
			notebook1.GetNthPage (2).Visible = !UoWGeneric.IsNew;
			notebook1.GetNthPage (3).Visible = !UoWGeneric.IsNew;

			comboSex.ItemsEnum = typeof(Sex);
			comboSex.Binding.AddBinding (Entity, e => e.Sex, w => w.SelectedItem).InitializeFromSource ();

			var stdConverter = new SizeStandardCodeConverter ();

			ycomboWearStd.Binding.AddBinding (Entity, e => e.WearSizeStd, w => w.SelectedItemOrNull, stdConverter ).InitializeFromSource ();
			ycomboShoesStd.Binding.AddBinding (Entity, e => e.ShoesSizeStd, w => w.SelectedItemOrNull, stdConverter ).InitializeFromSource ();
			ycomboHeaddressStd.ItemsEnum = typeof(SizeStandartHeaddress);
			ycomboHeaddressStd.Binding.AddBinding (Entity, e => e.HeaddressSizeStd, w => w.SelectedItemOrNull, stdConverter ).InitializeFromSource ();
			ycomboGlovesStd.ItemsEnum = typeof(SizeStandartGloves);
			ycomboGlovesStd.Binding.AddBinding (Entity, e => e.GlovesSizeStd, w => w.SelectedItemOrNull, stdConverter ).InitializeFromSource ();

			ycomboWearGrowth.Binding.AddBinding (Entity, e => e.WearGrowth, w => w.ActiveText).InitializeFromSource ();
			ycomboWearSize.Binding.AddBinding (Entity, e => e.WearSize, w => w.ActiveText).InitializeFromSource ();
			ycomboShoesSize.Binding.AddBinding (Entity, e => e.ShoesSize, w => w.ActiveText).InitializeFromSource ();
			ycomboHeaddressSize.Binding.AddBinding (Entity, e => e.HeaddressSize, w => w.ActiveText).InitializeFromSource ();
			ycomboGlovesSize.Binding.AddBinding (Entity, e => e.GlovesSize, w => w.ActiveText).InitializeFromSource ();

			entryFirstName.Binding.AddBinding (Entity, e => e.FirstName, w => w.Text).InitializeFromSource ();
			entryLastName.Binding.AddBinding (Entity, e => e.LastName, w => w.Text).InitializeFromSource ();
			entryPatronymic.Binding.AddBinding (Entity, e => e.Patronymic, w => w.Text).InitializeFromSource ();

			yentryPersonnelNumber.Binding.AddBinding (Entity, e => e.PersonnelNumber, w => w.Text).InitializeFromSource ();
			dateHire.Binding.AddBinding (Entity, e => e.HireDate, w => w.DateOrNull).InitializeFromSource ();
			dateDismiss.Binding.AddBinding (Entity, e => e.DismissDate, w => w.DateOrNull).InitializeFromSource ();

			yentryPost.SubjectType = typeof(Post);
			yentryPost.Binding.AddBinding (Entity, e => e.Post, w => w.Subject).InitializeFromSource ();
			OnYentryPostChanged (null, null);
			yentryLeader.SubjectType = typeof(Leader);
			yentryLeader.Binding.AddBinding (Entity, e => e.Leader, w => w.Subject).InitializeFromSource ();
			yentryObject.SubjectType = typeof(Facility);
			yentryObject.Binding.AddBinding (Entity, e => e.Facility, w => w.Subject).InitializeFromSource ();

			yimagePhoto.Binding.AddBinding (Entity, e => e.Photo, w => w.ImageFile).InitializeFromSource ();

			//Устанавливаем последовательность заполнения
			table1.FocusChain = new Widget[] {entryLastName, entryFirstName, entryPatronymic, dateHire, dateDismiss, 
				comboSex, yentryPost, yentryLeader, yentryObject
			};

			ytreeNorms.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<Norm> ()
				.AddColumn ("№ ТОН").SetDataProperty (node => node.TONNumber)
				.AddColumn ("№ Приложения").SetDataProperty (node => node.TONAttachment)
				.AddColumn ("№ Пункта").SetDataProperty (node => node.TONParagraph)
				.AddColumn ("Профессии").AddTextRenderer (node => node.ProfessionsText)
				.Finish ();
			ytreeNorms.Selection.Changed += YtreeNorms_Selection_Changed;
			ytreeNorms.ItemsDataSource = Entity.ObservableUsedNorms;

			ytreeWorkwear.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<EmployeeCardItem> ()
				.AddColumn ("ТОН").AddTextRenderer (node => node.ActiveNormItem.Norm.Title)
				.AddColumn ("Наименование").AddTextRenderer (node => node.Item.Name)
				.AddColumn ("По норме").AddTextRenderer (node => node.Item.Units.MakeAmountShortStr (node.ActiveNormItem.Amount))
				.AddColumn ("Срок службы").AddTextRenderer (node => node.ActiveNormItem.LifeText)
				.AddColumn ("Дата получения").AddTextRenderer (node => String.Format ("{0:d}", node.LastIssue))
				.AddColumn ("Получено").AddTextRenderer (node => node.Item.Units.MakeAmountShortStr (node.Amount))
					.AddSetter ((w, node) => w.Foreground = node.AmountColor)
				.AddColumn ("След. получение").AddTextRenderer (node => String.Format ("{0:d}", node.NextIssue))
				.AddColumn ("Просрочка").AddTextRenderer (
					node => node.NextIssue.HasValue && node.NextIssue.Value < DateTime.Today 
					? RusNumber.FormatCase((int) (DateTime.Today - node.NextIssue.Value).TotalDays, "{0} день", "{0} дня", "{0} дней") 
					: String.Empty)
				.AddColumn ("На складе").AddTextRenderer (node => node.Item.Units.MakeAmountShortStr (node.InStock))
				 .AddSetter ((w, node) => w.Foreground = node.InStockState.GetEnumColor ())
				.AddColumn ("Подобранная номенклатура").AddTextRenderer (node => node.MatchedNomenclature != null ? node.MatchedNomenclature.Name : (node.InStockState == StockStateInfo.UnknownNomenclature ? "нет подходящей" : String.Empty))
				.AddSetter ((w, node) => w.Foreground = node.InStockState.GetEnumColor ())
				.Finish ();
			//ytreeWorkwear.Selection.Changed += YtreeNorms_Selection_Changed;

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

		void YtreeNorms_Selection_Changed (object sender, EventArgs e)
		{
			buttonRemoveNorm.Sensitive = ytreeNorms.Selection.CountSelectedRows () > 0;
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
			notebook1.GetNthPage (2).Visible = true;
			notebook1.GetNthPage (3).Visible = true;
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
				string sql = "SELECT movements.*, nomenclature.name as nomenclature_name, measurement_units.name as unit " +
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
					"LEFT JOIN measurement_units ON item_types.units_id = measurement_units.id " +
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
					"item_types.name, measurement_units.name as unit, " +
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
					"LEFT JOIN measurement_units ON item_types.units_id = measurement_units.id " +
					"LEFT JOIN stock_expense ON stock_expense.id = stock_expense_detail.stock_expense_id " +
					"LEFT JOIN stock_income_detail ON stock_income_detail.id = stock_expense_detail.stock_income_detail_id " +
					"WHERE stock_expense.wear_card_id = @id AND " +
					"(stock_expense_detail.auto_writeoff_date IS NULL OR stock_expense_detail.auto_writeoff_date > CURDATE() ) AND " +
					"(spent.count IS NULL OR spent.count < stock_expense_detail.quantity ) " +
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
			ExpenseDocDlg winExpense = new ExpenseDocDlg (Entity);
			winExpense.Show ();
			int result = winExpense.Run ();
			winExpense.Destroy ();
			if(result == (int)ResponseType.Ok)
			{
				RefreshWorkItems ();
				UpdateMovements ();
				UpdateListedItems ();
			}
		}

		protected void OnButtonReturnWearClicked (object sender, EventArgs e)
		{
			IncomeDocDlg winIncome = new IncomeDocDlg (Entity);
			winIncome.Show ();
			int result = winIncome.Run ();
			winIncome.Destroy ();
			if(result == (int)ResponseType.Ok)
			{
				RefreshWorkItems ();
				UpdateMovements ();
				UpdateListedItems ();
			}
		}

		protected void OnButtonWriteOffWearClicked (object sender, EventArgs e)
		{
			WriteOffDocDlg winWriteOff = new WriteOffDocDlg (Entity);
			winWriteOff.Show ();
			int result = winWriteOff.Run ();
			winWriteOff.Destroy ();
			if(result == (int)ResponseType.Ok)
			{
				RefreshWorkItems ();
				UpdateMovements ();
				UpdateListedItems ();
			}
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
			if(notebook1.CurrentPage == 1 && ytreeWorkwear.ItemsDataSource == null)
			{
				Entity.FillWearInStockInfo (UoW);
				Entity.FillWearRecivedInfo (UoW);
				ytreeWorkwear.ItemsDataSource = Entity.ObservableWorkwearItems;
			}

			if(notebook1.CurrentPage == 3 && !IsShowedMovementsTable)
			{
				IsShowedMovementsTable = true;
				UpdateMovements ();
			}

			if(notebook1.CurrentPage == 2 && !IsShowedItemsTable)
			{
				IsShowedItemsTable = true;
				UpdateListedItems ();
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

		protected void OnComboSexChanged (object sender, EventArgs e)
		{
			if(Entity.Sex == Sex.M)
			{
				ycomboWearStd.ItemsEnum = typeof(SizeStandartMenWear);
				ycomboShoesStd.ItemsEnum = typeof(SizeStandartMenShoes);
				SizeHelper.FillSizeCombo (ycomboWearGrowth, SizeHelper.GetSizesList (GrowthStandartWear.Men, SizeUse.СlothesOnly));
			}
			else if(Entity.Sex == Sex.F)
			{
				ycomboWearStd.ItemsEnum = typeof(SizeStandartWomenWear);
				ycomboShoesStd.ItemsEnum = typeof(SizeStandartWomenShoes);
				SizeHelper.FillSizeCombo (ycomboWearGrowth, SizeHelper.GetSizesList (GrowthStandartWear.Women, SizeUse.СlothesOnly));
			}
			else
			{
				ycomboWearStd.ItemsEnum = null;
				ycomboShoesStd.ItemsEnum = null;
				ycomboWearGrowth.Clear ();
			}
		}

		protected void OnYcomboHeaddressChanged (object sender, EventArgs e)
		{
			if (ycomboHeaddressStd.SelectedItemOrNull != null)
				SizeHelper.FillSizeCombo (ycomboHeaddressSize, SizeHelper.GetSizesList (ycomboHeaddressStd.SelectedItem));
			else
				ycomboHeaddressSize.Clear ();
		}

		protected void OnYcomboWearStdChanged (object sender, EventArgs e)
		{
			if (ycomboWearStd.SelectedItemOrNull != null)
				SizeHelper.FillSizeCombo (ycomboWearSize, SizeHelper.GetSizesList (ycomboWearStd.SelectedItem));
			else
				ycomboWearSize.Clear ();
		}

		protected void OnYcomboShoesStdChanged (object sender, EventArgs e)
		{
			if (ycomboShoesStd.SelectedItemOrNull != null)
				SizeHelper.FillSizeCombo (ycomboShoesSize, SizeHelper.GetSizesList (ycomboShoesStd.SelectedItem));
			else
				ycomboShoesSize.Clear ();
		}

		protected void OnYcomboGlovesStdChanged (object sender, EventArgs e)
		{
			if (ycomboGlovesStd.SelectedItemOrNull != null)
				SizeHelper.FillSizeCombo (ycomboGlovesSize, SizeHelper.GetSizesList (ycomboGlovesStd.SelectedItem));
			else
				ycomboGlovesSize.Clear ();
		}

		protected void OnButtonAddNormClicked (object sender, EventArgs e)
		{
			var refWin = new ReferenceRepresentation (new workwear.ViewModel.NormVM ());
			refWin.Mode = OrmReferenceMode.Select;
			refWin.ObjectSelected += RefWin_ObjectSelected;
			var dialog = new OneWidgetDialog (refWin);
			dialog.Show ();
			dialog.Run ();
			dialog.Destroy ();
		}

		void RefWin_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			Entity.AddUsedNorm (UoW.GetById<Norm> (e.ObjectId));
		}

		protected void OnButtonRemoveNormClicked (object sender, EventArgs e)
		{
			Entity.RemoveUsedNorm (ytreeNorms.GetSelectedObject<Norm> ());
		}

		protected void OnYentryPostChanged (object sender, EventArgs e)
		{
			buttonNormFromPost.Sensitive = Entity.Post != null;
		}

		protected void OnButtonNormFromPostClicked (object sender, EventArgs e)
		{
			var norms = Repository.NormRepository.GetNormForPost (UoWGeneric, Entity.Post);
			foreach (var norm in norms)
				Entity.AddUsedNorm (norm);
		}

		protected void OnButtonGiveWearByNormClicked (object sender, EventArgs e)
		{
			if (IsNomenclaturePickuped == false &&
				Entity.WorkwearItems.Any (i => i.Amount < i.ActiveNormItem.Amount 
					&& 
					(i.InStockState == StockStateInfo.NotEnough
				   || i.InStockState == StockStateInfo.OutOfStock
				   || i.MatchedNomenclature == null
					)
				))
			{
				if (MessageDialogWorks.RunQuestionDialog ("Некоторые позиции отсутствуют на складе в достаточном количестве. Попробовать подобрать подходящую номенклатуру?"))
					buttonPickNomenclature.Click ();
			}

			if (!Save ())
				return;

			var addItems = new Dictionary<Nomenclature, int> ();
			foreach(var item in Entity.WorkwearItems)
			{
				if(item.NeededAmount > 0 && (item.InStockState == StockStateInfo.Enough || item.InStockState == StockStateInfo.NotEnough))
				{
					int amount = item.InStockState == StockStateInfo.Enough ? item.NeededAmount : item.InStock;
					addItems.Add (item.MatchedNomenclature, amount);
				}
			}

			ExpenseDocDlg winExpense = new ExpenseDocDlg (Entity, addItems);
			winExpense.Show ();
			int result = winExpense.Run ();
			winExpense.Destroy ();
			if(result == (int)ResponseType.Ok)
			{
				RefreshWorkItems ();
				UpdateMovements ();
				UpdateListedItems ();
			}
		}

		protected void OnButtonPickNomenclatureClicked (object sender, EventArgs e)
		{
			foreach (var item in Entity.WorkwearItems)
				item.FindMatchedNomenclature (UoW);
			IsNomenclaturePickuped = true;
		}

		protected void RefreshWorkItems()
		{
			if (!NHibernateUtil.IsInitialized (Entity.WorkwearItems))
				return;

			foreach (var item in Entity.WorkwearItems)
			{
				UoW.Session.Refresh (item);
			}
			Entity.FillWearInStockInfo (UoW);
			Entity.FillWearRecivedInfo (UoW);
		}
	}
}
