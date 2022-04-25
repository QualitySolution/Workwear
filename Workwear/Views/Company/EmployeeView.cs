using System;
using System.Reflection;
using Gamma.Binding.Converters;
using Gtk;
using NLog;
using QS.Dialog.GtkUI;
using QS.Views.Dialog;
using QSOrmProject;
using workwear.Domain.Company;
using workwear.Measurements;
using workwear.ViewModels.Company;
using workwear.Views.Company.EmployeeChilds;
using Workwear.Domain.Company;
using Workwear.Measurements;

namespace workwear.Views.Company
{
	public partial class EmployeeView : EntityDialogViewBase<EmployeeViewModel, EmployeeCard>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		private readonly Image eyeIcon = new Image(Assembly.GetExecutingAssembly(), "workwear.icon.buttons.eye.png");
		private readonly Image crossedEyeIcon = new Image(Assembly.GetExecutingAssembly(), "workwear.icon.buttons.eye-crossed.png");

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
			
			panelEmploeePhoto.Panel = new EmployeePhotoView(ViewModel.EmployeePhotoViewModel);
			panelEmploeePhoto.Binding.AddBinding(ViewModel, v => v.VisiblePhoto, w => w.IsHided, new BoolReverseConverter()).InitializeFromSource();

			notebook1.GetNthPage(4).Visible = ViewModel.VisibleListedItem;
			notebook1.GetNthPage(5).Visible = ViewModel.VisibleHistory;

			notebook1.Binding.AddSource(ViewModel).AddBinding(v => v.CurrentTab, w => w.CurrentPage);

			buttonColorsLegend.Binding.AddBinding(ViewModel, v => v.VisibleColorsLegend, w => w.Visible).InitializeFromSource();

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

			FillSizeCombo(ycomboWearGrowth, ViewModel.GetGrowths());
			ycomboWearGrowth.Binding.AddBinding (Entity, e => e.WearGrowth, w => w.ActiveText).InitializeFromSource ();
			ycomboWearSize.Binding.AddBinding (Entity, e => e.WearSize, w => w.ActiveText).InitializeFromSource ();
			ycomboShoesSize.Binding.AddBinding (Entity, e => e.ShoesSize, w => w.ActiveText).InitializeFromSource ();
			ycomboWinterShoesSize.Binding.AddBinding(Entity, e => e.WinterShoesSize, w => w.ActiveText).InitializeFromSource();
			ycomboHeaddressSize.Binding.AddBinding (Entity, e => e.HeaddressSize, w => w.ActiveText).InitializeFromSource ();
			ycomboGlovesSize.Binding.AddBinding (Entity, e => e.GlovesSize, w => w.ActiveText).InitializeFromSource ();
			FillSizeCombo(ycomboMittensSize, ViewModel.GetSizes(SizeStandartMittens.Rus));
			ycomboMittensSize.Binding.AddBinding(Entity, e => e.MittensSize, w => w.ActiveText).InitializeFromSource();

			entryFirstName.Binding.AddBinding (Entity, e => e.FirstName, w => w.Text).InitializeFromSource ();
			entryLastName.Binding.AddBinding (Entity, e => e.LastName, w => w.Text).InitializeFromSource ();
			entryPatronymic.Binding.AddBinding (Entity, e => e.Patronymic, w => w.Text).InitializeFromSource ();

			yentryPersonnelNumber.Binding.AddBinding (Entity, e => e.PersonnelNumber, w => w.Text).InitializeFromSource ();
			dateHire.Binding.AddBinding (Entity, e => e.HireDate, w => w.DateOrNull).InitializeFromSource ();
			dateChangePosition.Binding.AddBinding(Entity, e => e.ChangeOfPositionDate, w => w.DateOrNull).InitializeFromSource();
			dateDismiss.Binding.AddBinding (Entity, e => e.DismissDate, w => w.DateOrNull).InitializeFromSource ();

			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			entryId.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CardNumber, w => w.Text)
				.AddBinding(vm => vm.SensetiveCardNumber, w => w.Sensitive)
				.InitializeFromSource();
			checkAuto.Binding.AddBinding(ViewModel, vm => vm.AutoCardNumber, w => w.Active).InitializeFromSource();
			labelUser.Binding.AddBinding(ViewModel, vm => vm.CreatedByUser, w => w.LabelProp).InitializeFromSource();

			entitySubdivision.ViewModel = ViewModel.EntrySubdivisionViewModel;
			entityDepartment.ViewModel = ViewModel.EntryDepartmentViewModel;
			entityLeader.ViewModel = ViewModel.EntryLeaderViewModel;
			entityPost.ViewModel = ViewModel.EntryPostViewModel;

			hboxCardUid.Binding.AddBinding(ViewModel, v => v.VisibleCardUid, w => w.Visible).InitializeFromSource();
			ylabelCardUid.Binding.AddBinding(ViewModel, v => v.VisibleCardUid, w => w.Visible).InitializeFromSource();
			entryCardUid.Binding.AddSource(ViewModel)
				.AddBinding(v => v.CardUid, w => w.Text)
				.AddBinding(v => v.CardUidEntryColor, w => w.TextColor)
				.InitializeFromSource();

			entryPhone.PhoneFormat = QS.Utilities.Numeric.PhoneFormat.RussiaOnlyHyphenated;
			entryPhone.Binding.AddBinding(Entity, e => e.PhoneNumber, w => w.Text).InitializeFromSource();

			labelLkPassword.Binding.AddBinding(ViewModel, v => v.VisibleLkRegistration, w => w.Visible).InitializeFromSource();
			hboxLkPassword.Binding.AddBinding(ViewModel, v => v.VisibleLkRegistration, w => w.Visible).InitializeFromSource();
			buttonShowPassword.Binding.AddFuncBinding(ViewModel, v => v.ShowLkPassword ? crossedEyeIcon : eyeIcon, w => w.Image).InitializeFromSource();

			entryLkPassword.Binding
				.AddBinding(ViewModel, v => v.LkPassword, w => w.Text)
				.AddBinding(ViewModel, v => v.ShowLkPassword, w => w.Visibility)
				.InitializeFromSource();

			//Устанавливаем последовательность фокуса по Tab
			//!!!!!!!! НЕ ЗАБЫВАЕМ КОРРЕКТИРОВАТЬ ПОРЯДОК ПРИ ДОБАВЛЕНИИ ВИДЖЕТОВ В ТАБЛИЦУ !!!!!!!!
			//Это порядок только внутри таблицы! А не всего диалога.
			table1.FocusChain = new Widget[] {hbox7, entryLastName, entryFirstName, entryPatronymic,
				yentryPersonnelNumber, entryPhone, hboxLkPassword, hboxCardUid,
				dateHire, dateChangePosition, dateDismiss,
				entitySubdivision, entityDepartment, entityPost, entityLeader,   
				GtkScrolledWindowComments,
			};
			table2.FocusChain = new Widget[] {comboSex, ycomboWearGrowth,
				ycomboWearStd, ycomboWearSize,
				ycomboShoesStd, ycomboShoesSize,
				ycomboWinterShoesStd, ycomboWinterShoesSize,
				ycomboHeaddressStd, ycomboHeaddressSize,
				ycomboGlovesStd, ycomboGlovesSize, ycomboMittensSize
			};

			enumPrint.ItemsEnum = typeof(EmployeeViewModel.PersonalCardPrint);
			ViewModel.PropertyChanged += ViewModel_PropertyChanged;
		}

		#region События контролов

		protected void OnNotebook1SwitchPage(object o, SwitchPageArgs args)
		{
			ViewModel.SwitchOn((int)args.PageNum);
		}

		protected void OnComboSexChanged(object sender, EventArgs e)
		{
			if(Entity.Sex == Sex.M) {
				ycomboWearStd.ItemsEnum = typeof(SizeStandartMenWear);
				ycomboShoesStd.ItemsEnum = typeof(SizeStandartMenShoes);
				ycomboWinterShoesStd.ItemsEnum = typeof(SizeStandartMenShoes);
			}
			else if(Entity.Sex == Sex.F) {
				ycomboWearStd.ItemsEnum = typeof(SizeStandartWomenWear);
				ycomboShoesStd.ItemsEnum = typeof(SizeStandartWomenShoes);
				ycomboWinterShoesStd.ItemsEnum = typeof(SizeStandartWomenShoes);
			}
			else {
				ycomboWearStd.ItemsEnum = null;
				ycomboShoesStd.ItemsEnum = null;
				ycomboWinterShoesStd.ItemsEnum = null;
			}
		}

		protected void OnYcomboHeaddressChanged(object sender, EventArgs e)
		{
			if(ycomboHeaddressStd.SelectedItemOrNull != null)
				FillSizeCombo(ycomboHeaddressSize, ViewModel.GetSizes((Enum)ycomboHeaddressStd.SelectedItem));
			else
				ycomboHeaddressSize.Clear();
		}

		protected void OnYcomboWearStdChanged(object sender, EventArgs e)
		{
			if(ycomboWearStd.SelectedItemOrNull != null)
				FillSizeCombo(ycomboWearSize, ViewModel.GetSizes((Enum)ycomboWearStd.SelectedItem));
			else
				ycomboWearSize.Clear();
		}

		protected void OnYcomboShoesStdChanged(object sender, EventArgs e)
		{
			if(ycomboShoesStd.SelectedItemOrNull != null)
				FillSizeCombo(ycomboShoesSize, ViewModel.GetSizes((Enum)ycomboShoesStd.SelectedItem));
			else
				ycomboShoesSize.Clear();
		}

		protected void OnYcomboWinterShoesStdChanged(object sender, EventArgs e)
		{
			if(ycomboWinterShoesStd.SelectedItemOrNull != null)
				FillSizeCombo(ycomboWinterShoesSize, ViewModel.GetSizes((Enum)ycomboWinterShoesStd.SelectedItem));
			else
				ycomboWinterShoesSize.Clear();
		}

		protected void OnYcomboGlovesStdChanged(object sender, EventArgs e)
		{
			if(ycomboGlovesStd.SelectedItemOrNull != null)
				FillSizeCombo(ycomboGlovesSize, ViewModel.GetSizes((Enum)ycomboGlovesStd.SelectedItem));
			else
				ycomboGlovesSize.Clear();
		}

		#endregion

		#region Обработка кнопок

		protected void OnEnumPrintEnumItemClicked(object sender, EnumItemClickedEventArgs e)
		{
			var doc = (EmployeeViewModel.PersonalCardPrint)e.ItemEnum;
			ViewModel.Print(doc);
		}

		protected void OnButtonColorsLegendClicked(object sender, EventArgs e)
		{
			MessageDialogHelper.RunInfoDialog(
				"<b>Колонка «Получено»:</b>\n" +
				"<span color='darkgreen'>●</span> — потребность закрыта полностью\n" +
				"<span color='blue'>●</span> — получено больше необходимого\n" +
				"<span color='orange'>●</span> — получено меньше необходимого\n" +
				"<span color='red'>●</span> — получения не было\n" +
				"\n<b>Колонка «След. получение»:</b>\n" +
				"<span color='red'>●</span> — получение просрочено\n" +
				"<span color='darkgreen'>●</span> — возможна выдача раньше срока\n" +
				"\n<b>Колонка «На складе»:</b>\n" +
				"<span color='orange'>●</span> — подходящей номенклатуры не найдено\n" +
				"<span color='red'>●</span> — номенклатура на складе отсутствует\n" +
				"<span color='blue'>●</span> — на складе не достаточное количество\n" +
				"<span color='green'>●</span> — на складе достаточное количество"
			);
		}
		#endregion

		#region События View
		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.VisibleListedItem))
				notebook1.GetNthPage(4).Visible = ViewModel.VisibleListedItem;
			if(e.PropertyName == nameof(ViewModel.VisibleHistory))
				notebook1.GetNthPage(5).Visible = ViewModel.VisibleHistory;
		}
		#endregion

		void FillSizeCombo(ComboBox combo, string[] sizes)
		{
			combo.Clear();
			var list = new ListStore(typeof(string));
			list.AppendValues(String.Empty);
			foreach(var size in sizes)
				list.AppendValues(size);
			combo.Model = list;
			CellRendererText text = new CellRendererText();
			combo.PackStart(text, true);
			combo.AddAttribute(text, "text", 0);
		}

		protected void OnButtonReadUidClicked(object sender, EventArgs e)
		{
			ViewModel.ReadUid();
		}

		protected void OnButtonShowPasswordClicked(object sender, EventArgs e)
		{
			ViewModel.ToggleShowPassword();
		}

		protected void OnButtonGeneratePasswordClicked(object sender, EventArgs e)
		{
			ViewModel.CreateLkPassword();
		}
	}
}
