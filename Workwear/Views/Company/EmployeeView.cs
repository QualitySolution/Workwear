using System;
using Gtk;
using NLog;
using QS.Dialog.GtkUI;
using QS.Views.Dialog;
using QSOrmProject;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Measurements;
using workwear.Tools;
using workwear.ViewModels.Company;

namespace workwear.Views.Company
{
	public partial class EmployeeView : EntityDialogViewBase<EmployeeViewModel, EmployeeCard>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public EmployeeView(EmployeeViewModel viewModel) : base(viewModel)
		{
			this.Build ();
			ConfigureDlg ();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			employeenormsview1.ViewModel = ViewModel.NormsViewModel;
			employeewearitemsview1.ViewModel = ViewModel.WearItemsViewModel;
			employeecardlisteditemsview.ViewModel = ViewModel.ListedItemsViewModel;
			employeemovementsview1.ViewModel = ViewModel.MovementsViewModel;
			employeevacationsview1.ViewModel = ViewModel.VacationsViewModel;

			notebook1.GetNthPage(2).Visible = ViewModel.VisibleListedItem;
			notebook1.GetNthPage (3).Visible = ViewModel.VisibleHistory;

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
			yentryObject.SubjectType = typeof(Subdivision);
			yentryObject.Binding.AddBinding (Entity, e => e.Subdivision, w => w.Subject).InitializeFromSource ();
			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			entryId.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CardNumber, w => w.Text)
				.AddBinding(vm => vm.SensetiveCardNumber, w => w.Sensitive)
				.InitializeFromSource();
			checkAuto.Binding.AddBinding(ViewModel, vm => vm.AutoCardNumber, w => w.Active).InitializeFromSource();
			labelUser.Binding.AddBinding(ViewModel, vm => vm.CreatedByUser, w => w.LabelProp).InitializeFromSource();
			labelObjectAddress.Binding.AddBinding(ViewModel, vm => vm.SubdivisionAddress, w => w.LabelProp).InitializeFromSource();

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

			enumPrint.ItemsEnum = typeof(EmployeeViewModel.PersonalCardPrint);
		}

		#region События контролов

		protected void OnNotebook1SwitchPage(object o, SwitchPageArgs args)
		{
			ViewModel.SwitchOn((int)args.PageNum);
		}

		protected void OnButtonSavePhotoClicked(object sender, EventArgs e)
		{
			FileChooserDialog fc =
				new FileChooserDialog("Укажите файл для сохранения фотографии",
									   (Gtk.Window)this.Toplevel,
					FileChooserAction.Save,
					"Отмена", ResponseType.Cancel,
					"Сохранить", ResponseType.Accept);
			fc.CurrentName = ViewModel.SuggestedPhotoName;
			fc.Show();
			if (fc.Run() == (int)ResponseType.Accept)
			{
				fc.Hide();
				ViewModel.SavePhoto(fc.Filename);
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
				ViewModel.LoadPhoto(Chooser.Filename);
			}
			Chooser.Destroy();
		}

		protected void OnYimagePhotoButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			if (args.Event.Type == Gdk.EventType.TwoButtonPress)
			{
				ViewModel.OpenPhoto();
			}
		}

		protected void OnComboSexChanged(object sender, EventArgs e)
		{
			if (Entity.Sex == Sex.M)
			{
				ycomboWearStd.ItemsEnum = typeof(SizeStandartMenWear);
				ycomboShoesStd.ItemsEnum = typeof(SizeStandartMenShoes);
				ycomboWinterShoesStd.ItemsEnum = typeof(SizeStandartMenShoes);
				SizeHelper.FillSizeCombo(ycomboWearGrowth, SizeHelper.GetSizesList(GrowthStandartWear.Men, GetExcludedSizeUse()));
			}
			else if (Entity.Sex == Sex.F)
			{
				ycomboWearStd.ItemsEnum = typeof(SizeStandartWomenWear);
				ycomboShoesStd.ItemsEnum = typeof(SizeStandartWomenShoes);
				ycomboWinterShoesStd.ItemsEnum = typeof(SizeStandartWomenShoes);
				SizeHelper.FillSizeCombo(ycomboWearGrowth, SizeHelper.GetSizesList(GrowthStandartWear.Women, GetExcludedSizeUse()));
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
				SizeHelper.FillSizeCombo(ycomboHeaddressSize, SizeHelper.GetSizesList(ycomboHeaddressStd.SelectedItem, GetExcludedSizeUse()));
			else
				ycomboHeaddressSize.Clear();
		}

		protected void OnYcomboWearStdChanged(object sender, EventArgs e)
		{
			if (ycomboWearStd.SelectedItemOrNull != null)
				SizeHelper.FillSizeCombo(ycomboWearSize, SizeHelper.GetSizesList(ycomboWearStd.SelectedItem, GetExcludedSizeUse()));
			else
				ycomboWearSize.Clear();
		}

		protected void OnYcomboShoesStdChanged(object sender, EventArgs e)
		{
			if (ycomboShoesStd.SelectedItemOrNull != null)
				SizeHelper.FillSizeCombo(ycomboShoesSize, SizeHelper.GetSizesList(ycomboShoesStd.SelectedItem, GetExcludedSizeUse()));
			else
				ycomboShoesSize.Clear();
		}

		protected void OnYcomboWinterShoesStdChanged(object sender, EventArgs e)
		{
			if (ycomboWinterShoesStd.SelectedItemOrNull != null)
				SizeHelper.FillSizeCombo(ycomboWinterShoesSize, SizeHelper.GetSizesList(ycomboWinterShoesStd.SelectedItem, GetExcludedSizeUse()));
			else
				ycomboWinterShoesSize.Clear();
		}

		protected void OnYcomboGlovesStdChanged(object sender, EventArgs e)
		{
			if (ycomboGlovesStd.SelectedItemOrNull != null)
				SizeHelper.FillSizeCombo(ycomboGlovesSize, SizeHelper.GetSizesList(ycomboGlovesStd.SelectedItem, GetExcludedSizeUse()));
			else
				ycomboGlovesSize.Clear();
		}

		#endregion

		protected void OnYperiodMaternityLeavePeriodChangedByUser(object sender, EventArgs e)
		{
			Entity.UpdateAllNextIssue();
		}

		#region Обработка кнопок

		protected void OnEnumPrintEnumItemClicked(object sender, EnumItemClickedEventArgs e)
		{
			var doc = (EmployeeViewModel.PersonalCardPrint)e.ItemEnum;
			ViewModel.Print(doc);
		}

		#endregion

		private SizeUse[] GetExcludedSizeUse()
		{
			return BaseParameters.EmployeeSizeRanges ? new SizeUse[] { } : new SizeUse[] { SizeUse.СlothesOnly };
		}
	}
}
