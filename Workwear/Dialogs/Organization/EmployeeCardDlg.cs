using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Autofac;
using Gamma.Utilities;
using Gtk;
using NLog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QSOrmProject;
using QSProjectsLib;
using QSReport;
using workwear.Dialogs.Regulations;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.JournalViewModels.Company;
using workwear.Measurements;
using workwear.Repository;
using workwear.Tools;
using workwear.ViewModels.Company;

namespace workwear.Dialogs.Organization
{
	public partial class EmployeeCardDlg : EntityDialogBase<EmployeeCard>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		ILifetimeScope AutofacScope;
		bool IsShowedItemsTable = false;

		bool IsPostSetOnLoad;
		bool IsSubdivisionSetOnLoad;

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

			IsPostSetOnLoad = Entity.Post != null;
			IsSubdivisionSetOnLoad = Entity.Subdivision != null;

			comboSex.ItemsEnum = typeof(Sex);
			comboSex.Binding.AddBinding (Entity, e => e.Sex, w => w.SelectedItem).InitializeFromSource ();

			var stdConverter = new SizeStandardCodeConverter ();

			ycomboWearStd.Binding.AddBinding (Entity, e => e.WearSizeStd, w => w.SelectedItemOrNull, stdConverter ).InitializeFromSource ();
			ycomboShoesStd.Binding.AddBinding (Entity, e => e.ShoesSizeStd, w => w.SelectedItemOrNull, stdConverter ).InitializeFromSource ();
			ycomboWinterShoesStd.Binding.AddBinding(Entity, e => e.WinterShoesSizeStd, w => w.SelectedItemOrNull, stdConverter).InitializeFromSource();
			ycomboHeaddressStd.ItemsEnum = typeof(SizeStandartHeaddress);
			ycomboHeaddressStd.Binding.AddBinding (Entity, e => e.HeaddressSizeStd, w => w.SelectedItemOrNull, stdConverter ).InitializeFromSource ();
			ycomboGlovesStd.ItemsEnum = typeof(SizeStandartGloves);
			ycomboGlovesStd.Binding.AddBinding (Entity, e => e.GlovesSizeStd, w => w.SelectedItemOrNull, stdConverter ).InitializeFromSource ();

			ycomboWearGrowth.Binding.AddBinding (Entity, e => e.WearGrowth, w => w.ActiveText).InitializeFromSource ();
			ycomboWearSize.Binding.AddBinding (Entity, e => e.WearSize, w => w.ActiveText).InitializeFromSource ();
			ycomboShoesSize.Binding.AddBinding (Entity, e => e.ShoesSize, w => w.ActiveText).InitializeFromSource ();
			ycomboWinterShoesSize.Binding.AddBinding(Entity, e => e.WinterShoesSize, w => w.ActiveText).InitializeFromSource();
			ycomboHeaddressSize.Binding.AddBinding (Entity, e => e.HeaddressSize, w => w.ActiveText).InitializeFromSource ();
			ycomboGlovesSize.Binding.AddBinding (Entity, e => e.GlovesSize, w => w.ActiveText).InitializeFromSource ();

			entryFirstName.Binding.AddBinding (Entity, e => e.FirstName, w => w.Text).InitializeFromSource ();
			entryLastName.Binding.AddBinding (Entity, e => e.LastName, w => w.Text).InitializeFromSource ();
			entryPatronymic.Binding.AddBinding (Entity, e => e.Patronymic, w => w.Text).InitializeFromSource ();

			yentryPersonnelNumber.Binding.AddBinding (Entity, e => e.PersonnelNumber, w => w.Text).InitializeFromSource ();
			dateHire.Binding.AddBinding (Entity, e => e.HireDate, w => w.DateOrNull).InitializeFromSource ();
			dateChangePosition.Binding.AddBinding(Entity, e => e.ChangeOfPositionDate, w => w.DateOrNull).InitializeFromSource();
			dateDismiss.Binding.AddBinding (Entity, e => e.DismissDate, w => w.DateOrNull).InitializeFromSource ();

			yentryPost.SubjectType = typeof(Post);
			yentryPost.Binding.AddBinding (Entity, e => e.Post, w => w.Subject).InitializeFromSource ();
			OnYentryPostChanged (null, null); //Так как событие не будет вызвано, если в объекте null, а кнопку "По должности" нужно заблокировать. 
			yentryObject.SubjectType = typeof(Subdivision);
			yentryObject.Binding.AddBinding (Entity, e => e.Subdivision, w => w.Subject).InitializeFromSource ();
			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			yimagePhoto.Binding.AddBinding (Entity, e => e.Photo, w => w.ImageFile).InitializeFromSource ();

			//Устанавливаем последовательность фокуса по Tab
			//!!!!!!!! НЕ ЗАБЫВАЕМ КОРРЕКТИРОВАТЬ ПОРЯДОК ПРИ ДОБАВЛЕНИИ ВИДЖЕТОВ В ТАБЛИЦУ !!!!!!!!
			//Это порядок только внутри таблицы! А не всего диалога.
			table1.FocusChain = new Widget[] {hbox7, entryLastName, entryFirstName, entryPatronymic, 
				yentryPost, entityLeader, yentryObject, yentryPersonnelNumber, dateHire, dateChangePosition, dateDismiss, 
				GtkScrolledWindowComments, comboSex, ycomboWearGrowth, 
				ycomboWearStd, ycomboWearSize, 
				ycomboShoesStd, ycomboShoesSize,
				ycomboWinterShoesStd, ycomboWinterShoesSize,
				ycomboHeaddressStd, ycomboHeaddressSize,
				ycomboGlovesStd, ycomboGlovesSize,
			};

			ytreeNorms.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<Norm> ()
				.AddColumn ("№ ТОН").AddTextRenderer (node => node.DocumentNumberText)
				.AddColumn ("№ Приложения").AddNumericRenderer (node => node.AnnexNumberText)
				.AddColumn ("№ Пункта").SetDataProperty (node => node.TONParagraph)
				.AddColumn ("Профессии").AddTextRenderer (node => node.ProfessionsText)
				.Finish ();
			ytreeNorms.Selection.Changed += YtreeNorms_Selection_Changed;
			ytreeNorms.ItemsDataSource = Entity.ObservableUsedNorms;

			enumPrint.ItemsEnum = typeof(PersonalCardPrint);

			Entity.PropertyChanged += CheckSizeChanged;
			Entity.PropertyChanged += Entity_PropertyChanged;

			AutofacScope = MainClass.AppDIContainer.BeginLifetimeScope();
			var builder = new LegacyEEVMBuilderFactory<EmployeeCard>(this, Entity, UoW, MainClass.MainWin.NavigationManager, AutofacScope);

			entityLeader.ViewModel = builder.ForProperty(x => x.Leader)
				.UseViewModelJournalAndAutocompleter<LeadersJournalViewModel>()
				.UseViewModelDialog<LeadersViewModel>()
				.Finish();
		}

		public EmployeeCardDlg(EmployeeCard card) : this(card.Id) { }

		public EmployeeCardDlg(int id)
		{
			this.Build();

			logger.Info("Запрос карточки №{0}...", id);
			try
			{
				UoWGeneric = UnitOfWorkFactory.CreateForRoot<EmployeeCard>(id);

				checkAuto.Active = String.IsNullOrWhiteSpace(Entity.CardNumber);
				entryId.Text = String.IsNullOrWhiteSpace(Entity.CardNumber) ? Entity.Id.ToString() : Entity.CardNumber;

				labelUser.LabelProp = Entity.CreatedbyUser != null ? Entity.CreatedbyUser.Name : "не указан";

				logger.Info("Ok");
				buttonGiveWear.Sensitive = true;
				buttonReturnWear.Sensitive = true;
				buttonWriteOffWear.Sensitive = true;
				enumPrint.Sensitive = true;
			}
			catch (Exception ex)
			{
				QSMain.ErrorMessageWithLog("Ошибка получения карточки!", logger, ex);
				FailInitialize = true;
				return;
			}

			ConfigureDlg();
		}

		public override bool Save()
		{
			logger.Info("Запись карточки...");
			var valid = new QSValidator<EmployeeCard>(UoWGeneric.Root);
			if (valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			UoWGeneric.Save();

			notebook1.GetNthPage(2).Visible = true;
			notebook1.GetNthPage(3).Visible = true;
			logger.Info("Ok");
			return true;
		}

		void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			//Так как склад подбора мог поменятся при смене подразделения.
			if(e.PropertyName == nameof(Entity.Subdivision))
				Entity.FillWearInStockInfo(UoW, Entity.Subdivision?.Warehouse, DateTime.Now);
		}

		void CheckSizeChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			СlothesType category;
			if (e.PropertyName == nameof(Entity.GlovesSize))
				category = СlothesType.Gloves;
			else if (e.PropertyName == nameof(Entity.WearSize))
				category = СlothesType.Wear;
			else if (e.PropertyName == nameof(Entity.ShoesSize))
				category = СlothesType.Shoes;
			else if (e.PropertyName == nameof(Entity.HeaddressSize))
				category = СlothesType.Headgear;
			else if (e.PropertyName == nameof(Entity.WinterShoesSize))
				category = СlothesType.WinterShoes;
			else if (e.PropertyName == nameof(Entity.WearGrowth))
				category = СlothesType.Wear;
			else return;

			//Обновляем подобранную номенклатуру
			Entity.FillWearInStockInfo(UoW, Entity?.Subdivision?.Warehouse, DateTime.Now);
		}

		protected void OnButtonGiveWearClicked(object sender, EventArgs e)
		{
			ExpenseDocDlg winExpense = new ExpenseDocDlg(Entity);
			OpenNewTab(winExpense);
		}

		protected void OnButtonReturnWearClicked(object sender, EventArgs e)
		{
			IncomeDocDlg winIncome = new IncomeDocDlg(Entity);
			OpenNewTab(winIncome);
		}

		protected void OnButtonWriteOffWearClicked(object sender, EventArgs e)
		{
			WriteOffDocDlg winWriteOff = new WriteOffDocDlg(Entity);
			OpenNewTab(winWriteOff);
		}

		protected void OnCheckAutoToggled(object sender, EventArgs e)
		{
			entryId.Sensitive = !checkAuto.Active;
			if (checkAuto.Active)
				entryId.Text = Entity.Id.ToString();
		}

		protected void OnEntryIdChanged(object sender, EventArgs e)
		{
			Entity.CardNumber = checkAuto.Active ? null : entryId.Text;
		}

		protected void OnNotebook1SwitchPage(object o, SwitchPageArgs args)
		{
			if (notebook1.CurrentPage == 1 && !employeewearitemsview1.ItemsLoaded)
			{
				employeewearitemsview1.LoadItems();
			}

			if (notebook1.CurrentPage == 3 && !employeemovementsview1.MovementsLoaded)
			{
				employeemovementsview1.LoadMovements();
			}

			if (notebook1.CurrentPage == 2 && !IsShowedItemsTable)
			{
				IsShowedItemsTable = true;
				employeecardlisteditemsview.UpdateList();
			}

			if(notebook1.CurrentPage == 4 && !employeevacationsview1.VacationsLoaded) {
				employeevacationsview1.UpdateList();
			}
		}

		protected void OnYentryObjectChanged(object sender, EventArgs e)
		{
			labelObjectAddress.LabelProp = Entity.Subdivision != null ? Entity.Subdivision.Address : "--//--";
		}

		protected void OnButtonSavePhotoClicked(object sender, EventArgs e)
		{
			FileChooserDialog fc =
				new FileChooserDialog("Укажите файл для сохранения фотографии",
									   (Gtk.Window)this.Toplevel,
					FileChooserAction.Save,
					"Отмена", ResponseType.Cancel,
					"Сохранить", ResponseType.Accept);
			fc.CurrentName = Entity.FullName + ".jpg";
			fc.Show();
			if (fc.Run() == (int)ResponseType.Accept)
			{
				fc.Hide();
				FileStream fs = new FileStream(fc.Filename, FileMode.Create, FileAccess.Write);
				fs.Write(UoWGeneric.Root.Photo, 0, UoWGeneric.Root.Photo.Length);
				fs.Close();
			}
			fc.Destroy();
		}

		protected void OnButtonLoadPhotoClicked(object sender, EventArgs e)
		{
			FileChooserDialog Chooser = new FileChooserDialog("Выберите фото для загрузки...",
															   (Gtk.Window)this.Toplevel,
				FileChooserAction.Open,
				"Отмена", ResponseType.Cancel,
				"Загрузить", ResponseType.Accept);

			FileFilter Filter = new FileFilter();
			Filter.AddPixbufFormats();
			Filter.Name = "Все изображения";
			Chooser.AddFilter(Filter);

			if ((ResponseType)Chooser.Run() == ResponseType.Accept)
			{
				Chooser.Hide();
				logger.Info("Загрузка фотографии...");

				FileStream fs = new FileStream(Chooser.Filename, FileMode.Open, FileAccess.Read);
				if (Chooser.Filename.ToLower().EndsWith(".jpg"))
				{
					using (MemoryStream ms = new MemoryStream())
					{
						fs.CopyTo(ms);
						UoWGeneric.Root.Photo = ms.ToArray();
					}
				}
				else
				{
					logger.Info("Конвертация в jpg ...");
					Gdk.Pixbuf image = new Gdk.Pixbuf(fs);
					UoWGeneric.Root.Photo = image.SaveToBuffer("jpeg");
				}
				fs.Close();
				buttonSavePhoto.Sensitive = true;
				logger.Info("Ok");
			}
			Chooser.Destroy();
		}

		protected void OnYimagePhotoButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			if (((Gdk.EventButton)args.Event).Type == Gdk.EventType.TwoButtonPress)
			{
				string filePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "temp_img.jpg");
				FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
				fs.Write(UoWGeneric.Root.Photo, 0, UoWGeneric.Root.Photo.Length);
				fs.Close();
				System.Diagnostics.Process.Start(filePath);
			}
		}

		protected void OnComboSexChanged(object sender, EventArgs e)
		{
			if (Entity.Sex == Sex.M)
			{
				ycomboWearStd.ItemsEnum = typeof(SizeStandartMenWear);
				ycomboShoesStd.ItemsEnum = typeof(SizeStandartMenShoes);
				ycomboWinterShoesStd.ItemsEnum = typeof(SizeStandartMenShoes);
				SizeHelper.FillSizeCombo(ycomboWearGrowth, SizeHelper.GetSizesList(GrowthStandartWear.Men, SizeHelper.GetExcludedSizeUseForEmployee()));
			}
			else if (Entity.Sex == Sex.F)
			{
				ycomboWearStd.ItemsEnum = typeof(SizeStandartWomenWear);
				ycomboShoesStd.ItemsEnum = typeof(SizeStandartWomenShoes);
				ycomboWinterShoesStd.ItemsEnum = typeof(SizeStandartWomenShoes);
				SizeHelper.FillSizeCombo(ycomboWearGrowth, SizeHelper.GetSizesList(GrowthStandartWear.Women, SizeHelper.GetExcludedSizeUseForEmployee()));
			}
			else
			{
				ycomboWearStd.ItemsEnum = null;
				ycomboShoesStd.ItemsEnum = null;
				ycomboWinterShoesStd.ItemsEnum = null;
				ycomboWearGrowth.Clear();
			}
		}

		protected void OnYcomboHeaddressChanged(object sender, EventArgs e)
		{
			if (ycomboHeaddressStd.SelectedItemOrNull != null)
				SizeHelper.FillSizeCombo(ycomboHeaddressSize, SizeHelper.GetSizesList(ycomboHeaddressStd.SelectedItem, SizeHelper.GetExcludedSizeUseForEmployee()));
			else
				ycomboHeaddressSize.Clear();
		}

		protected void OnYcomboWearStdChanged(object sender, EventArgs e)
		{
			if (ycomboWearStd.SelectedItemOrNull != null)
				SizeHelper.FillSizeCombo(ycomboWearSize, SizeHelper.GetSizesList(ycomboWearStd.SelectedItem, SizeHelper.GetExcludedSizeUseForEmployee()));
			else
				ycomboWearSize.Clear();
		}

		protected void OnYcomboShoesStdChanged(object sender, EventArgs e)
		{
			if (ycomboShoesStd.SelectedItemOrNull != null)
				SizeHelper.FillSizeCombo(ycomboShoesSize, SizeHelper.GetSizesList(ycomboShoesStd.SelectedItem, SizeHelper.GetExcludedSizeUseForEmployee()));
			else
				ycomboShoesSize.Clear();
		}

		protected void OnYcomboWinterShoesStdChanged(object sender, EventArgs e)
		{
			if (ycomboWinterShoesStd.SelectedItemOrNull != null)
				SizeHelper.FillSizeCombo(ycomboWinterShoesSize, SizeHelper.GetSizesList(ycomboWinterShoesStd.SelectedItem, SizeHelper.GetExcludedSizeUseForEmployee()));
			else
				ycomboWinterShoesSize.Clear();
		}

		protected void OnYcomboGlovesStdChanged(object sender, EventArgs e)
		{
			if (ycomboGlovesStd.SelectedItemOrNull != null)
				SizeHelper.FillSizeCombo(ycomboGlovesSize, SizeHelper.GetSizesList(ycomboGlovesStd.SelectedItem, SizeHelper.GetExcludedSizeUseForEmployee()));
			else
				ycomboGlovesSize.Clear();
		}

		#region Подвкладка Используемые нормы

		void YtreeNorms_Selection_Changed(object sender, EventArgs e)
		{
			buttonRemoveNorm.Sensitive = buttonNormOpen.Sensitive = ytreeNorms.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonAddNormClicked(object sender, EventArgs e)
		{
			var refWin = new ReferenceRepresentation(new workwear.ViewModel.NormVM());
			refWin.Mode = OrmReferenceMode.Select;
			refWin.ObjectSelected += RefWin_ObjectSelected;
			OpenSlaveTab(refWin);
		}

		void RefWin_ObjectSelected(object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			Entity.AddUsedNorm(UoW.GetById<Norm>(e.ObjectId));
		}

		protected void OnButtonRemoveNormClicked(object sender, EventArgs e)
		{
			Entity.RemoveUsedNorm(ytreeNorms.GetSelectedObject<Norm>());
		}

		protected void OnYentryPostChanged(object sender, EventArgs e)
		{
			buttonNormFromPost.Sensitive = Entity.Post != null;
		}

		protected void OnButtonNormFromPostClicked(object sender, EventArgs e)
		{
			var norms = Repository.NormRepository.GetNormForPost(UoWGeneric, Entity.Post);
			foreach (var norm in norms)
				Entity.AddUsedNorm(norm);
		}

		protected void OnButtonRefreshWorkwearItemsClicked(object sender, EventArgs e)
		{
			Entity.UpdateWorkwearItems();
		}

		protected void OnButtonNormOpenClicked(object sender, EventArgs e)
		{
			var selectedNorm = ytreeNorms.GetSelectedObject<Norm>();
			OpenTab<NormDlg, Norm>(selectedNorm);
		}

		protected void OnYtreeNormsRowActivated(object o, RowActivatedArgs args)
		{
			buttonNormOpen.Click();
		}

		#endregion

		protected void OnYperiodMaternityLeavePeriodChangedByUser(object sender, EventArgs e)
		{
			Entity.UpdateAllNextIssue();
		}

		protected void OnYentryPostChangedByUser(object sender, EventArgs e)
		{
			if (IsPostSetOnLoad && MessageDialogHelper.RunQuestionDialog("Установить новую дату изменения должности или перевода в другое структурное подразделение для сотрудника?"))
			{
				Entity.ChangeOfPositionDate = DateTime.Today;
			}
		}

		protected void OnYentryObjectChangedByUser(object sender, EventArgs e)
		{
			if (IsSubdivisionSetOnLoad && MessageDialogHelper.RunQuestionDialog("Установить новую дату изменения должности или перевода в другое структурное подразделение для сотрудника?"))
			{
				Entity.ChangeOfPositionDate = DateTime.Today;
			}
		}

		public enum PersonalCardPrint
		{
			[Display(Name = "Лицевая сторона")]
			[ReportIdentifier("Employee.PersonalCardPage1")]
			PersonalCardPage1,
			[Display(Name = "Оборотная сторона")]
			[ReportIdentifier("Employee.PersonalCardPage2")]
			PersonalCardPage2,
			[Display(Name = "Внутренная с фотографией")]
			[ReportIdentifier("WearCard")]
			CardWithPhoto,
		}

		protected void OnEnumPrintEnumItemClicked(object sender, EnumItemClickedEventArgs e)
		{
			var doc = (PersonalCardPrint)e.ItemEnum;

			if (UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint(typeof(EmployeeCard), "бумажной версии"))
				Save();

			var reportInfo = new ReportInfo
			{
				Title = String.Format("Карточка {0} - {1}", Entity.ShortName, doc.GetEnumTitle()),
				Identifier = doc.GetAttribute<ReportIdentifierAttribute>().Identifier,
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.Id }
				}
			};

			TabParent.OpenTab(QSReport.ReportViewDlg.GenerateHashName(reportInfo),
							  () => new QSReport.ReportViewDlg(reportInfo)
							 );
		}

		public override void Destroy()
		{
			base.Destroy();
			AutofacScope.Dispose();
		}
	}
}
