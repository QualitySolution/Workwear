using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Autofac;
using Gamma.Utilities;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QSReport;
using workwear.Domain.Company;
using workwear.Journal.ViewModels.Company;
using workwear.Measurements;
using workwear.ViewModels.Company.EmployeeChilds;

namespace workwear.ViewModels.Company
{
	public class EmployeeViewModel : EntityDialogViewModelBase<EmployeeCard>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly IUserService userService;

		ILifetimeScope AutofacScope;
		private readonly CommonMessages messages;

		public EmployeeViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IValidator validator,
			IUserService userService,
			ILifetimeScope autofacScope,
			CommonMessages messages) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
			AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.messages = messages ?? throw new ArgumentNullException(nameof(messages));
			var builder = new CommonEEVMBuilderFactory<EmployeeCard>(this, Entity, UoW, NavigationManager, AutofacScope);

			EntryLeaderViewModel = builder.ForProperty(x => x.Leader)
				.UseViewModelJournalAndAutocompleter<LeadersJournalViewModel>()
				.UseViewModelDialog<LeadersViewModel>()
				.Finish();

			Entity.PropertyChanged += CheckSizeChanged;
			Entity.PropertyChanged += Entity_PropertyChanged;

			if(UoW.IsNew) {
				Entity.CreatedbyUser = userService.GetCurrentUser(UoW);
			}
			else {
				AutoCardNumber = String.IsNullOrWhiteSpace(Entity.CardNumber);
			}

			//Создаем вкладки
			var parameter = new TypedParameter(typeof(EmployeeViewModel), this);
			NormsViewModel = AutofacScope.Resolve<EmployeeNormsViewModel>(parameter);
			WearItemsViewModel = AutofacScope.Resolve<EmployeeWearItemsViewModel>(parameter);
			ListedItemsViewModel = AutofacScope.Resolve<EmployeeListedItemsViewModel>(parameter);
			MovementsViewModel = AutofacScope.Resolve<EmployeeMovementsViewModel>(parameter);
			VacationsViewModel = AutofacScope.Resolve<EmployeeVacationsViewModel>(parameter);
		}

		#region Контролы

		public readonly EntityEntryViewModel<Leader> EntryLeaderViewModel;

		#endregion

		#region Visible

		public bool VisibleListedItem => !UoW.IsNew;
		public bool VisibleHistory => !UoW.IsNew;

		#endregion

		#region Sensetive

		public bool SensetiveCardNumber => !AutoCardNumber;
		public bool SensetiveSavePhoto => Entity.Photo != null;

		#endregion

		#region Свойства ViewModel

		private bool autoCardNumber;
		[PropertyChangedAlso(nameof(CardNumber))]
		[PropertyChangedAlso(nameof(SensetiveCardNumber))]
		public bool AutoCardNumber { get => autoCardNumber; set => SetField(ref autoCardNumber, value); }

		public string CardNumber {
			get => AutoCardNumber ? Entity.Id.ToString() : Entity.CardNumber;
			set => Entity.CardNumber = AutoCardNumber ? null : value;
		}

		public string CreatedByUser => Entity.CreatedbyUser?.Name;

		public string SubdivisionAddress => Entity.Subdivision?.Address ?? "--//--";

		#endregion

		#region Обработка событий

		void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			//Так как склад подбора мог поменятся при смене подразделения.
			if(e.PropertyName == nameof(Entity.Subdivision)) {
				Entity.FillWearInStockInfo(UoW, Entity.Subdivision?.Warehouse, DateTime.Now);
				OnPropertyChanged(nameof(SubdivisionAddress));			}
		}

		void CheckSizeChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			СlothesType category;
			if(e.PropertyName == nameof(Entity.GlovesSize))
				category = СlothesType.Gloves;
			else if(e.PropertyName == nameof(Entity.WearSize))
				category = СlothesType.Wear;
			else if(e.PropertyName == nameof(Entity.ShoesSize))
				category = СlothesType.Shoes;
			else if(e.PropertyName == nameof(Entity.HeaddressSize))
				category = СlothesType.Headgear;
			else if(e.PropertyName == nameof(Entity.WinterShoesSize))
				category = СlothesType.WinterShoes;
			else if(e.PropertyName == nameof(Entity.WearGrowth))
				category = СlothesType.Wear;
			else return;

			//Обновляем подобранную номенклатуру
			Entity.FillWearInStockInfo(UoW, Entity?.Subdivision?.Warehouse, DateTime.Now);
		}

		#endregion

		#region Вкладки

		public EmployeeNormsViewModel NormsViewModel;				//0
		public EmployeeWearItemsViewModel WearItemsViewModel; 		//1
		public EmployeeListedItemsViewModel ListedItemsViewModel;  //2
		public EmployeeMovementsViewModel MovementsViewModel;      //3
		public EmployeeVacationsViewModel VacationsViewModel;		//4

		public void SwitchOn(int tab)
		{
			switch(tab) {
				case 0: NormsViewModel.OnShow();
					break;
				case 1: WearItemsViewModel.OnShow();
					break;
				case 2: ListedItemsViewModel.OnShow();
					break;
				case 3: MovementsViewModel.OnShow();
					break;
				case 4: VacationsViewModel.OnShow();
					break;
			}
		}

		#endregion

		public override bool Save()
		{
			var result = base.Save();

			OnPropertyChanged(nameof(VisibleHistory));
			OnPropertyChanged(nameof(VisibleListedItem));

			return result;
		}

		#region Печать

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

		public void Print(PersonalCardPrint doc)
		{
			if(UoWGeneric.HasChanges && messages.SaveBeforePrint(typeof(EmployeeCard), "бумажной версии"))
				Save();

			var reportInfo = new ReportInfo {
				Title = String.Format("Карточка {0} - {1}", Entity.ShortName, doc.GetEnumTitle()),
				Identifier = doc.GetAttribute<ReportIdentifierAttribute>().Identifier,
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.Id }
				}
			};

			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}

		#endregion

		#region Фото

		public string SuggestedPhotoName => Entity.FullName + ".jpg";

		public void SavePhoto(string fileName)
		{
			using(FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write)) {
				fs.Write(UoWGeneric.Root.Photo, 0, UoWGeneric.Root.Photo.Length);
			}
		}

		public void LoadPhoto(string fileName)
		{
			logger.Info("Загрузка фотографии...");

			using(FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
				if(fileName.ToLower().EndsWith(".jpg")) {
					using(MemoryStream ms = new MemoryStream()) {
						fs.CopyTo(ms);
						Entity.Photo = ms.ToArray();
					}
				}
				else {
					logger.Info("Конвертация в jpg ...");
					Gdk.Pixbuf image = new Gdk.Pixbuf(fs);
					Entity.Photo = image.SaveToBuffer("jpeg");
				}
			}
			OnPropertyChanged(nameof(SensetiveSavePhoto));
			logger.Info("Ok");
		}

		public void OpenPhoto()
		{
			string filePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "temp_img.jpg");
			using(FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) {
				fs.Write(UoWGeneric.Root.Photo, 0, UoWGeneric.Root.Photo.Length);
			}
			System.Diagnostics.Process.Start(filePath);
		}

		#endregion
	}
}
