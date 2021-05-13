using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
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
using workwear.Repository.Company;
using workwear.Tools.Features;
using workwear.ViewModels.Company.EmployeeChilds;
using workwear.ViewModels.IdentityCards;

namespace workwear.ViewModels.Company
{
	public class EmployeeViewModel : EntityDialogViewModelBase<EmployeeCard>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly IUserService userService;

		public ILifetimeScope AutofacScope;
		private readonly SizeService sizeService;
		private readonly IInteractiveQuestion interactive;
		private readonly FeaturesService featuresService;
		private readonly EmployeeRepository employeeRepository;
		private readonly CommonMessages messages;

		public EmployeeViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IValidator validator,
			IUserService userService,
			ILifetimeScope autofacScope,
			SizeService sizeService,
			IInteractiveQuestion interactive,
			FeaturesService featuresService,
			EmployeeRepository employeeRepository,
			CommonMessages messages) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
			AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			this.messages = messages ?? throw new ArgumentNullException(nameof(messages));
			var builder = new CommonEEVMBuilderFactory<EmployeeCard>(this, Entity, UoW, NavigationManager, AutofacScope);

			EntryLeaderViewModel = builder.ForProperty(x => x.Leader)
				.UseViewModelJournalAndAutocompleter<LeadersJournalViewModel>()
				.UseViewModelDialog<LeadersViewModel>()
				.Finish();

			EntrySubdivisionViewModel = builder.ForProperty(x => x.Subdivision)
				.UseViewModelJournalAndAutocompleter<SubdivisionJournalViewModel>()
				.UseViewModelDialog<SubdivisionViewModel>()
				.Finish();

			EntryDepartmentViewModel = builder.ForProperty(x => x.Department)
				.UseViewModelJournalAndAutocompleter<DepartmentJournalViewModel>()
				.UseViewModelDialog<DepartmentViewModel>()
				.Finish();

			EntryPostViewModel = builder.ForProperty(x => x.Post)
				.UseViewModelJournalAndAutocompleter<PostJournalViewModel>()
				.UseViewModelDialog<PostViewModel>()
				.Finish();

			Entity.PropertyChanged += CheckSizeChanged;
			Entity.PropertyChanged += Entity_PropertyChanged;
			Entity.PropertyChanged += PostChangedCheck;

			if(UoW.IsNew) {
				Entity.CreatedbyUser = userService.GetCurrentUser(UoW);
			}
			else {
				AutoCardNumber = String.IsNullOrWhiteSpace(Entity.CardNumber);
			}

			lastSubdivision = Entity.Subdivision;
			lastPost = Entity.Post;

			//Создаем вкладки
			var parameter = new TypedParameter(typeof(EmployeeViewModel), this);
			NormsViewModel = AutofacScope.Resolve<EmployeeNormsViewModel>(parameter);
			WearItemsViewModel = AutofacScope.Resolve<EmployeeWearItemsViewModel>(parameter);
			ListedItemsViewModel = AutofacScope.Resolve<EmployeeListedItemsViewModel>(parameter);
			MovementsViewModel = AutofacScope.Resolve<EmployeeMovementsViewModel>(parameter);
			VacationsViewModel = AutofacScope.Resolve<EmployeeVacationsViewModel>(parameter);
			//Панели
			EmployeePhotoViewModel = AutofacScope.Resolve<EmployeePhotoViewModel>(parameter);

			VisiblePhoto = Entity.Photo != null;
		}

		#region Контролы

		public readonly EntityEntryViewModel<Leader> EntryLeaderViewModel;
		public readonly EntityEntryViewModel<Subdivision> EntrySubdivisionViewModel;
		public readonly EntityEntryViewModel<Department> EntryDepartmentViewModel;
		public readonly EntityEntryViewModel<Post> EntryPostViewModel;

		public EmployeePhotoViewModel EmployeePhotoViewModel;

		#endregion

		#region Visible

		public bool VisibleListedItem => !UoW.IsNew;
		public bool VisibleHistory => !UoW.IsNew;
		public bool VisibleCardUid => featuresService.Available(WorkwearFeature.IdentityCards);
		public bool VisibleLkPassword => featuresService.Available(WorkwearFeature.EmployeeLk);

		private bool visiblePhoto;
		public virtual bool VisiblePhoto {
			get => visiblePhoto;
			set => SetField(ref visiblePhoto, value);
		}

		#endregion

		#region Sensetive

		public bool SensetiveCardNumber => !AutoCardNumber;

		#endregion

		#region Свойства ViewModel

		private bool autoCardNumber = true;
		[PropertyChangedAlso(nameof(CardNumber))]
		[PropertyChangedAlso(nameof(SensetiveCardNumber))]
		public bool AutoCardNumber { get => autoCardNumber; set => SetField(ref autoCardNumber, value); }

		public string CardNumber {
			get => AutoCardNumber ? (Entity.Id != 0 ? Entity.Id.ToString() : "авто" ) : Entity.CardNumber;
			set => Entity.CardNumber = (AutoCardNumber || value == "авто") ? null : value;
		}

		public string CreatedByUser => Entity.CreatedbyUser?.Name;

		public string SubdivisionAddress => Entity.Subdivision?.Address ?? "--//--";

		#region CardUid
		public virtual string CardUid {
			get => Entity.CardKey;
			set {
				Entity.CardKey = value;
				OnPropertyChanged(nameof(CardUid));
				OnPropertyChanged(nameof(CardUidEntryColor));
			}
		}

		public string CardUidEntryColor => (String.IsNullOrEmpty(CardUid) || System.Text.RegularExpressions.Regex.IsMatch(CardUid, @"\A\b[0-9a-fA-F]+\b\Z")) ? "black" : "red";

		#endregion

		#region Личный кабинет

		private bool showPassword;
		public virtual bool ShowPassword {
			get => showPassword;
			set => SetField(ref showPassword, value);
		}

		public void CreateLkPassword()
		{
			var length = 6;
			var random = new Random();
			var charStr = "abcdefghijklmnpqrstuvwxyz0123456789";
			var password = new StringBuilder(length);
			for(int i = 0; i < length; i++) {
				password.Append(charStr[random.Next(0, charStr.Length)]);
			}

			Entity.LkPassword = password.ToString();
		}

		#endregion

		#endregion


		#region Size

		public string[] GetSizes(string code) => sizeService.GetSizesForEmployee(code);
		public string[] GetSizes(Enum std) => sizeService.GetSizesForEmployee(std);

		public string[] GetGrowths(Sex sex) => sizeService.GetSizesForEmployee(sex == Sex.F ? GrowthStandartWear.Women : GrowthStandartWear.Men);

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
			else if(e.PropertyName == nameof(Entity.MittensSize))
				category = СlothesType.Mittens;
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
		public EmployeeVacationsViewModel VacationsViewModel;       //4

		private int lastTab;
		private int currentTab;
		public virtual int CurrentTab {
			get => currentTab;
			set => SetField(ref currentTab, value);
		}

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
				case 4:
					if(UoW.IsNew) {
						if(interactive.Question("Перед открытием отпусков необходимо сохранить сотрудника. Сохранить?", "Сохранить сотрудника?")
								&& Save()) {
							VacationsViewModel.OnShow();
						}
						else {
							CurrentTab = lastTab;
						}
					}
					else
						VacationsViewModel.OnShow();
					break;
			}
			lastTab = CurrentTab;
		}

		#endregion

		protected override bool Validate()
		{
			if(!base.Validate())
				return false;
			if(!String.IsNullOrWhiteSpace(Entity.CardKey)) {
				var employeeSameUid = employeeRepository.GetEmployeeByCardkey(UoW, Entity.CardKey);
				if(employeeSameUid != null && !employeeSameUid.IsSame(Entity)) {
					if(interactive.Question($"UID карты уже привязан к сотруднику {employeeSameUid.ShortName}. Перепривязать карту к {Entity.ShortName}?")) {
						//Здесь сохраняем удаляем UID через отдельный uow чтобы избежать ошибки базы по уникальному значению поля.
						using(var uow2 = UnitOfWorkFactory.CreateForRoot<EmployeeCard>(employeeSameUid.Id)) {
							uow2.Root.CardKey = null;
							uow2.Save();
						}
					}
					else
						return false;
				}
			}
			return true;
		}

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

		#region Дата изменения должности

		Subdivision lastSubdivision;
		Post lastPost;

		void PostChangedCheck(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Post) && lastPost != null && interactive.Question("Установить новую дату изменения должности или перевода в другое структурное подразделение для сотрудника?")) {
				Entity.ChangeOfPositionDate = DateTime.Today;
			}
			if(e.PropertyName == nameof(Entity.Subdivision) && lastSubdivision != null && interactive.Question("Установить новую дату изменения должности или перевода в другое структурное подразделение для сотрудника?")) {
				Entity.ChangeOfPositionDate = DateTime.Today;
			}
		}
		#endregion

		#region Uid

		public void ReadUid()
		{
			var page = NavigationManager.OpenViewModel<ReadCardViewModel>(this);
			page.PageClosed += delegate (object sender, PageClosedEventArgs e) {
				if(e.CloseSource != CloseSource.Save)
					return;
				CardUid = page.ViewModel.CardUid.Replace("-", "");
			};
		}

		#endregion
	}
}
