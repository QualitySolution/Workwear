using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Autofac;
using Gamma.Utilities;
using Grpc.Core;
using NHibernate;
using NLog;
using QS.Cloud.WearLk.Client;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Domain.Sizes;
using workwear.Journal.ViewModels.Communications;
using workwear.Journal.ViewModels.Company;
using Workwear.Models.Company;
using Workwear.Repository.Company;
using Workwear.Repository.Regulations;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Communications;
using Workwear.ViewModels.Company.EmployeeChildren;
using Workwear.ViewModels.IdentityCards;

namespace Workwear.ViewModels.Company
{
	public class EmployeeViewModel : EntityDialogViewModelBase<EmployeeCard>, IValidatableObject, IDialogDocumentation
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public ILifetimeScope AutofacScope;
		public NormRepository NormRepository { get; }
		
		private readonly PersonNames personNames;
		private readonly IInteractiveService interactive;
		private readonly FeaturesService featuresService;
		private readonly EmployeeRepository employeeRepository;
		private readonly LkUserManagerService lkUserManagerService;
		private readonly CommonMessages messages;
		private readonly SpecCoinManagerService specCoinManagerService;
		private readonly BaseParameters baseParameters;
		
		public SizeService SizeService { get; }

		private int remainingEmployees;

		public EmployeeViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			UnitOfWorkProvider unitOfWorkProvider,
			INavigationManager navigation,
			IValidator validator,
			IUserService userService,
			ILifetimeScope autofacScope,
			PersonNames personNames,
			IInteractiveService interactive,
			IProgressBarDisplayable globalProgress,
			FeaturesService featuresService,
			EmployeeRepository employeeRepository,
			NormRepository normRepository,
			LkUserManagerService lkUserManagerService,
			SizeService sizeService,
			CommonMessages messages,
			BaseParameters baseParameters,
			SpecCoinManagerService specCoinManagerService) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider)
		{
			AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.personNames = personNames ?? throw new ArgumentNullException(nameof(personNames));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			employeeRepository.RepoUow = UoW;
			NormRepository = normRepository ?? throw new ArgumentNullException(nameof(normRepository));
			this.lkUserManagerService = lkUserManagerService ?? throw new ArgumentNullException(nameof(lkUserManagerService));
			this.messages = messages ?? throw new ArgumentNullException(nameof(messages));
			this.specCoinManagerService = specCoinManagerService ?? throw new ArgumentNullException(nameof(specCoinManagerService));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			SizeService = sizeService;
			Performance = new ProgressPerformanceHelper(globalProgress, 12, "Загрузка размеров", logger);
			remainingEmployees = featuresService.Employees - employeeRepository.GetNumberOfEmployees();
			if(remainingEmployees < 0) {
				remainingEmployees = 0;
			}
			if(Entity.Id == 0) {
				if(featuresService.Employees != 0) {
					if(featuresService.Employees <= employeeRepository.GetNumberOfEmployees()) {
						throw new AbortCreatingPageException($"Невозможно добавить нового сотрудника: количество сотрудников в базе " +
						                                     $"превышает лимит Вашей лицензии.\nЛимит Вашей лицензии: {featuresService.Employees}",
							"Ошибка добавления сотрудника");

					}
					if(remainingEmployees <= 3) {
						interactive.ShowMessage(ImportanceLevel.Warning, $"Лимит сотрудников по Вашей лицензии: {featuresService.Employees}.\n" +
							                                                 $"Осталось по Вашей лицензии: {remainingEmployees}",
								"Предупреждение");
					}
					
				}
			}
			
			bool isCoinsAvailable = IsSpecCoinsAvailable();
			VisibleSpecCoinsViews = isCoinsAvailable;
			if (isCoinsAvailable)
			{
				SpecCoinsBalance =
					specCoinManagerService.GetCoinsBalance(Entity.PhoneNumber);
				VisibleSpecCoinsViews = SpecCoinsBalance != -1;
			}
			
			SizeService.RefreshSizes(UoW);
			Performance.CheckPoint("Загрузка основной информации о сотруднике");
			//Подгружаем данные для ускорения открытия диалога
			UoW.Session.QueryOver<EmployeeCard>()
				.Where(x => x.Id == Entity.Id)
				.Fetch(SelectMode.FetchLazyProperties, x => x)//Нужна чтобы не пытаться запрашивать фотку отдельным запросом.
				.Fetch(SelectMode.Fetch, x => x.CreatedbyUser)
				.Fetch(SelectMode.Fetch, x => x.Subdivision)
				.Fetch(SelectMode.Fetch, x => x.Department)
				.Fetch(SelectMode.Fetch, x => x.Post)
				.Fetch(SelectMode.Fetch, x => x.Sizes)
				.SingleOrDefault();
			Performance.CheckPoint("Создание диалога");
			
			var builderVm = new CommonEEVMBuilderFactory<EmployeeViewModel>(this, this, UoW, NavigationManager, AutofacScope);

			EntryLeaderViewModel = builderVm.ForProperty(x => x.Leader)
				.UseViewModelJournalAndAutocompleter<LeadersJournalViewModel>()
				.UseViewModelDialog<LeadersViewModel>()
				.Finish();

			subdivision = Entity.Subdivision;
			EntrySubdivisionViewModel = builderVm.ForProperty(x => x.Subdivision)
				.UseViewModelJournalAndAutocompleter<SubdivisionJournalViewModel>()
				.UseViewModelDialog<SubdivisionViewModel>()
				.Finish();

			department = Entity.Department;
			EntryDepartmentViewModel = builderVm.ForProperty(x => x.Department)
				.UseViewModelDialog<DepartmentViewModel>()
				.Finish();

			EntryDepartmentViewModel.EntitySelector = new DepartmentJournalViewModelSelector(
				this, NavigationManager, EntrySubdivisionViewModel);
			
			post = Entity.Post; 			
			EntryPostViewModel = builderVm.ForProperty(x => x.Post)
				.UseViewModelJournalAndAutocompleter<PostJournalViewModel>()
				.UseViewModelDialog<PostViewModel>()
				.Finish();
			
			Entity.PropertyChanged += Entity_PropertyChanged;

			if(Entity.Id == 0) {
				Entity.CreatedbyUser = userService.GetCurrentUser();
				logger.Info("Создание карточки для нового сотрудника");
			}
			else {
				AutoCardNumber = String.IsNullOrWhiteSpace(Entity.CardNumber);
			}

			//Создаем вкладки
			var parameter = new TypedParameter(typeof(EmployeeViewModel), this);
			NormsViewModel = AutofacScope.Resolve<EmployeeNormsViewModel>(parameter);
			WearItemsViewModel = AutofacScope.Resolve<EmployeeWearItemsViewModel>(parameter);
			DutyNormsViewModel = AutofacScope.Resolve<EmployeeDutyNormsViewModel>(parameter);
			CostCenterViewModel = AutofacScope.Resolve<EmployeeCostCentersViewModel>(parameter);
			ListedItemsViewModel = AutofacScope.Resolve<EmployeeListedItemsViewModel>(parameter);
			MovementsViewModel = AutofacScope.Resolve<EmployeeMovementsViewModel>(parameter);
			InGroupsViewModel = AutofacScope.Resolve<EmployeeInGroupsViewModel>(parameter);
			VacationsViewModel = AutofacScope.Resolve<EmployeeVacationsViewModel>(parameter);
			//Панели
			EmployeePhotoViewModel = AutofacScope.Resolve<EmployeePhotoViewModel>(parameter);

			VisiblePhoto = Entity.Photo != null;
			lkLastPhone = Entity.PhoneNumber;
			LkPassword = Entity.LkRegistered ? unknownPassword : String.Empty;
			
			Performance.CheckPoint("Создание View");
			Validations.Clear();
			Validations.Add(new ValidationRequest(this));
			Validations.Add(new ValidationRequest(Entity, new ValidationContext(Entity, new Dictionary<object, object>{{nameof(FeaturesService), featuresService}})));
		}

		public readonly ProgressPerformanceHelper Performance;

		#region Контролы

		public readonly EntityEntryViewModel<Leader> EntryLeaderViewModel;
		public readonly EntityEntryViewModel<Subdivision> EntrySubdivisionViewModel;
		public readonly EntityEntryViewModel<Department> EntryDepartmentViewModel;
		public readonly EntityEntryViewModel<Post> EntryPostViewModel;

		public EmployeePhotoViewModel EmployeePhotoViewModel;

		#endregion

		#region Visible

		public bool VisibleListedItem => Entity.Id != 0;
		public bool VisibleHistory => Entity.Id != 0;
		public bool VisibleCardUid => featuresService.Available(WorkwearFeature.IdentityCards);
		public bool VisibleLkRegistration => featuresService.Available(WorkwearFeature.EmployeeLk);
		public bool VisibleCostCenters => featuresService.Available(WorkwearFeature.CostCenter);
		public bool VisibleEmployeeGroups => featuresService.Available(WorkwearFeature.EmployeeGroups);
		public bool VisibleDutyNorm => featuresService.Available(WorkwearFeature.DutyNorms) && Entity.RelatedDutyNorms.Count > 0;
		public bool VisibleVacations => featuresService.Available(WorkwearFeature.Vacation);
		public bool VisibleColorsLegend => CurrentTab == 3;

		private bool visiblePhoto;
		public virtual bool VisiblePhoto {
			get => visiblePhoto;
			set => SetField(ref visiblePhoto, value);
		}

		private bool visibleSpecCoinsViews;
		public bool VisibleSpecCoinsViews 
		{
			get => visibleSpecCoinsViews;
			set => SetField(ref visibleSpecCoinsViews, value);
		}

		#endregion

		#region Sensetive
		public bool SensitiveCardNumber => !AutoCardNumber;
		public bool SensitiveDeductSpecCoins => SpecCoinsBalance > 0;
		#endregion

		#region Свойства ViewModel и пробросы из Model
		
		private bool skipChangeOfPositionDate = false;
		private bool autoCardNumber = true;
		[PropertyChangedAlso(nameof(CardNumber))]
		[PropertyChangedAlso(nameof(SensitiveCardNumber))]
		public bool AutoCardNumber { get => autoCardNumber; set => SetField(ref autoCardNumber, value); }

		public string CardNumber {
			get => AutoCardNumber ? (Entity.Id != 0 ? Entity.Id.ToString() : "авто" ) : Entity.CardNumber;
			set => Entity.CardNumber = (AutoCardNumber || value == "авто") ? null : value;
		}

		public Leader Leader {
			get => Entity.Leader;
			set => Entity.Leader = value;
		}
			
		private Subdivision subdivision;
		public Subdivision Subdivision {
			get => subdivision;
			set { if(subdivision != value) {
					if(!skipChangeOfPositionDate && Entity.ChangeOfPositionDate != DateTime.Today && interactive.Question(
						   "Установить для сотрудника новую дату последнего перевода в другое структурное подразделение?")) {
						Entity.ChangeOfPositionDate = DateTime.Today;
					}
					subdivision = value;
					Entity.Subdivision = value;
				}
			}
		}

		private Department department;
		public Department Department {
			get => department;
			set { if(department != value) {
					if(!skipChangeOfPositionDate && Entity.ChangeOfPositionDate != DateTime.Today && interactive.Question(
						   "Установить для сотрудника новую дату последнего перевода в другое структурное подразделение?")) {
						Entity.ChangeOfPositionDate = DateTime.Today;
					}
					department = value;
					Entity.Department = value;
				}
			}
		}

		private Post post;
		public Post Post {
			get => post;
			set {
				if(post != value) {
					skipChangeOfPositionDate = true;
					if(value != null) {
						if(value.Subdivision != null && value.Subdivision != Entity.Subdivision &&
						   value.Department != null && value.Department != Entity.Department) {
							if(interactive.Question(
								   "Подразделение и отдел в должности отличается от указанных в сотруднике. Установить их в сотрудника из должности?")) {
								Entity.Subdivision = value.Subdivision;
								Entity.Department = value.Department;
							}
						}
						else if(value.Subdivision != null && value.Subdivision != Entity.Subdivision) {
							if(interactive.Question(
								   "Подразделение в должности отличается от указанных в сотруднике. Установить его в сотрудника из должности?")) {
								
								Entity.Subdivision = value.Subdivision;
							}
						}
						else if(value.Department != null && value.Department != Entity.Department) {
							if(interactive.Question(
								   "Отдел в должности отличается от указанных в сотрудника. Установить его в сотрудника из должности?")) {
								Entity.Department = value.Department;
							}
						}

						if(Entity.UsedNorms.Any(n => (n.Posts.Any(p => p == post) && !n.Posts.Any(p => p == value)))
						   && interactive.Question("Заменить нормы от старой должности нормами от новой должности?")) {
							Entity.NormFromPost(UoW, NormRepository, value);
						}

						var postsNorms = NormRepository.GetNormsForPost(UoW, value);
						if(postsNorms.Any(n => !Entity.UsedNorms.Contains(n)) &&
						   interactive.Question((Entity.UsedNorms.Count == 0 ? "Установить" : "Дополнить") +" нормы по должности?")) {
							if(Entity.Id == 0 && !Save()) {
								//Здесь если не сохраним нового сотрудника при установки нормы скорей всего упадем.
								interactive.ShowMessage(ImportanceLevel.Error,
									"Норма не установлена, так как не все данные сотрудника заполнены корректно.");
								return;
							}
							Entity.NormFromPost(UoW, NormRepository, value);
						}
					}
					post = value;
					Entity.Post = value;
					
					skipChangeOfPositionDate = false;
					if(Entity.ChangeOfPositionDate != DateTime.Today && interactive.Question(
						   "Установить для сотрудника новую дату изменения должности или последнего перевода в другое структурное подразделение?")) {
						Entity.ChangeOfPositionDate = DateTime.Today;
					}
				}
			} 
		}
		
		public string CreatedByUser => Entity.CreatedbyUser?.Name;

		public bool IsDocNumberInIssueSign => baseParameters.IsDocNumberInIssueSign;
		public bool IsDocNumberInReturnSign => baseParameters.IsDocNumberInReturnSign;
		public DateTime? StartDateOfOperations => baseParameters.StartDateOfOperations;

		#region CardUid
		public virtual string CardUid {
			get => Entity.CardKey;
			set {
				Entity.CardKey = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(CardUidEntryColor));
			}
		}

		public string CardUidEntryColor => 
			String.IsNullOrEmpty(CardUid) || System.Text.RegularExpressions.Regex.IsMatch(CardUid, @"\A\b[0-9a-fA-F]+\b\Z") ? "black" : "red";

		#endregion
		
		private int specCoinsBalance;
		[PropertyChangedAlso(nameof(SensitiveDeductSpecCoins))]
		public int SpecCoinsBalance 
		{
			get => specCoinsBalance;
			set => SetField(ref specCoinsBalance, value);
		}

		private bool IsSpecCoinsAvailable() 
		{
			return featuresService.Available(WorkwearFeature.SpecCoinsLk) && 
				Entity.LkRegistered && !string.IsNullOrWhiteSpace(Entity.PhoneNumber);
		}
		
		#endregion

		#region Личный кабинет

		const int lkPasswordLength = 6;
		const string unknownPassword = "Unknown";

		private string lkLastPhone;
		private string lkLastPassword;

		private bool showLkPassword;
		public virtual bool ShowLkPassword {
			get => showLkPassword;
			set => SetField(ref showLkPassword, value);
		}

		private string lkPassword;
		public virtual string LkPassword {
			get => lkPassword;
			set => SetField(ref lkPassword, value);
		}

		public void ToggleShowPassword()
		{
			if(LkPassword == unknownPassword)
				LkPassword = lkLastPassword = lkUserManagerService.GetPassword(lkLastPhone);
			ShowLkPassword = !ShowLkPassword;
		}

		public void CreateLkPassword()
		{
			var random = new Random();
			var charStr = "abcdefghijklmnpqrstuvwxyz0123456789";
			var password = new StringBuilder(lkPasswordLength);
			for(int i = 0; i < lkPasswordLength; i++) {
				password.Append(charStr[random.Next(0, charStr.Length)]);
			}

			LkPassword = password.ToString();
		}

		public bool SyncLkPassword()
		{
			if(!lkUserManagerService.CanConnect)
				return true;
		
			try {
				if(String.IsNullOrWhiteSpace(LkPassword) || String.IsNullOrWhiteSpace(Entity.PhoneNumber)) {
					if(Entity.LkRegistered) {
						lkUserManagerService.RemovePhone(lkLastPhone);
						Entity.LkRegistered = false;
					}
					return true;
				}
				if(lkLastPhone != Entity.PhoneNumber && Entity.LkRegistered && !String.IsNullOrWhiteSpace(lkLastPhone))
					lkUserManagerService.ReplacePhone(lkLastPhone, Entity.PhoneNumber);

				if(LkPasswordNotChanged)
					return true;

				lkUserManagerService.SetPassword(Entity.PhoneNumber, LkPassword);
				Entity.LkRegistered = true;
				return true;
			} catch (RpcException e) when(e.Status.StatusCode == StatusCode.InvalidArgument || e.Status.StatusCode == StatusCode.AlreadyExists || e.Status.StatusCode == StatusCode.ResourceExhausted)
			{
				interactive.ShowMessage(ImportanceLevel.Error, e.Status.Detail);
				return false;
			}
		}

		//Пароль не менялся, проверяем не полное вхождение на случай случайного удаления части символов.
		public bool LkPasswordNotChanged => unknownPassword.StartsWith(LkPassword, StringComparison.InvariantCulture)
			|| LkPassword == lkLastPassword;
		#endregion

		#region Обработка событий

		void Entity_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.FirstName)) {
				var sex = personNames.GetSexByName(Entity.FirstName);
				if(sex != Sex.None)
					Entity.Sex = sex;
			}
		}

		#endregion
		#region MyRegion

		public void OpenCoinsOperations() {
			NavigationManager.OpenViewModel<SpecCoinsOperationsJournalViewModel, EmployeeCard>(this, Entity);
		}
		public void OpenDeductCoinsView() 
		{
			NavigationManager.OpenViewModel<DeductSpecCoinsViewModel, string, int, Action>
			(
				this,
				Entity.PhoneNumber,
				specCoinsBalance,
				UpdateSpecCoinsBalance
			);
		}

		private void UpdateSpecCoinsBalance() 
		{
			SpecCoinsBalance = specCoinManagerService.GetCoinsBalance(Entity.PhoneNumber);
		}
		#endregion

		#region Вкладки
																	// 0 - Информация
																	// 1 - Размеры
		public EmployeeNormsViewModel NormsViewModel;				//2
		public EmployeeWearItemsViewModel WearItemsViewModel;		//3
		public EmployeeDutyNormsViewModel DutyNormsViewModel;		//4
		public EmployeeCostCentersViewModel CostCenterViewModel;	//5
		public EmployeeInGroupsViewModel InGroupsViewModel;			//6
		public EmployeeListedItemsViewModel ListedItemsViewModel;	//7
		public EmployeeMovementsViewModel MovementsViewModel;       //8
		public EmployeeVacationsViewModel VacationsViewModel;       //9


		private int lastTab;
		private int currentTab = 0;
		[PropertyChangedAlso(nameof(VisibleColorsLegend))]
		public virtual int CurrentTab {
			get => currentTab;
			set => SetField(ref currentTab, value);
		}

		public void SwitchOn(int tab)
		{
			switch(tab) {
				case 2: NormsViewModel.OnShow();
					break;
				case 3:
					if (Entity.Id == 0)
						if (interactive.Question("Перед работой с имуществом сотрудника необходимо сохранить карточку. Сохранить?",
							    "Сохранить сотрудника?") && Save())
						{
							WearItemsViewModel.OnShow();
						}
						else
							CurrentTab = lastTab;
					else
						WearItemsViewModel.OnShow();;
					break;
				case 4: DutyNormsViewModel.OnShow();
					break;
				case 5: CostCenterViewModel.OnShow();
					break;
				case 6: InGroupsViewModel.OnShow();
					break;
				case 7:
					if(Entity.Id != 0)
						ListedItemsViewModel.OnShow();
					break;
				case 8: 
					if(Entity.Id != 0)
						MovementsViewModel.OnShow();
					break;
				case 9:
					if( Entity.Id == 0) {
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

		#region Сохранение и Валидация

		protected override bool Validate()
		{
			if(!base.Validate())
				return false;
			//Проверка номера карты на уникальность для базы
			if(!String.IsNullOrWhiteSpace(Entity.CardKey)) {
				var employeeSameUid = employeeRepository.GetEmployeeByCardkey(UoW, Entity.CardKey);
				if(employeeSameUid != null && !employeeSameUid.IsSame(Entity)) {
					if(interactive.Question($"UID карты уже привязан к сотруднику {employeeSameUid.ShortName}, " +
					                        $"удалить у него UID карты? Чтобы сохранить {Entity.ShortName}.")) {
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
			//Проверка номера телефона на уникальность для базы
			if(!String.IsNullOrWhiteSpace(Entity.PhoneNumber)) {
				var employeeSamePhone = employeeRepository.GetEmployeeByPhone(Entity.PhoneNumber);
				if(employeeSamePhone != null && !employeeSamePhone.IsSame(Entity)) {
					if(interactive.Question($"Телефон {Entity.PhoneNumber} уже привязан к сотруднику {employeeSamePhone.ShortName}. " +
					                        $"Удалить у него телефон? Чтобы сохранить {Entity.ShortName}?")) {
						//Здесь сохраняем удаляем телефон через отдельный uow чтобы избежать ошибки базы по уникальному значению поля.
						using(var uow2 = UnitOfWorkFactory.CreateForRoot<EmployeeCard>(employeeSamePhone.Id)) {
							if(uow2.Root.LkRegistered && lkUserManagerService.CanConnect)
								lkUserManagerService.RemovePhone(uow2.Root.PhoneNumber);
							uow2.Root.PhoneNumber = null;
							uow2.Root.LkRegistered = false;
							uow2.Save();
						}
					}
					else
						return false;
				}
			}
			return SyncLkPassword();
		}

		public override bool Save() {
			logger.Info("Сохранение карточки сотрудника");
			var result = base.Save();
			OnPropertyChanged(nameof(VisibleHistory));
			OnPropertyChanged(nameof(VisibleListedItem));
			return result;
		}
		IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
		{
			if (String.IsNullOrEmpty(LkPassword)) yield break;
			if(String.IsNullOrEmpty(Entity.PhoneNumber))
				yield return new ValidationResult(
					"Для установки пароля от личного кабинета сотрудника необходимо так же указать его телефон.",
					new[] { nameof(LkPassword) });

			if(LkPassword.Length < 3)
				yield return new ValidationResult(
					"Длинна пароля от личного кабинета должна быть не менее 3-х символов.", 
					new[] { nameof(LkPassword) });

			if(LkPassword.Length > 32)
				yield return new ValidationResult(
					"Длинна пароля от личного кабинета должна быть не более 32-х символов.", 
					new[] { nameof(LkPassword) });
		}
		#endregion
		#region Печать

		public enum PersonalCardPrint
		{
			[Display(Name = "Лицевая сторона (Приказ №28н от 27.01.2010г.)")]
			[ReportIdentifier("Employee.PersonalCardPage1")]
			PersonalCardPage1,
			[Display(Name = "Оборотная сторона (Приказ №28н от 27.01.2010г.)")]
			[ReportIdentifier("Employee.PersonalCardPage2")]
			PersonalCardPage2,
			[Display(Name = "Лицевая сторона (Приказ №766н от 29.10.2021г.)")]
			[ReportIdentifier("Employee.PersonalCardPageNew1")]
			PersonalCardPageNew1,
			[Display(Name = "Оборотная сторона (Приказ №766н от 29.10.2021г.)")]
			[ReportIdentifier("Employee.PersonalCardPageNew2")]
			PersonalCardPageNew2,
			[Display(Name = "СИЗ к получению")]
			[ReportIdentifier("Employee.IssuedSheet")]
			IssuedSheet
		}

		public void Print(PersonalCardPrint doc)
		{
			if(UoWGeneric.HasChanges && messages.SaveBeforePrint(typeof(EmployeeCard), "бумажной версии"))
				Save();

			var reportInfo = new ReportInfo {
				Title = String.Format("Карточка {0} - {1}", Entity.ShortName, doc.GetEnumTitle()),
				Identifier = doc.GetAttribute<ReportIdentifierAttribute>().Identifier,
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.Id },
					{"isDocNumberInIssueSign", IsDocNumberInIssueSign},
					{"isDocNumberInReturnSign", IsDocNumberInReturnSign},
					{"printPromo",featuresService.Available(WorkwearFeature.PrintPromo)},
					{"startDateOfOperations", StartDateOfOperations}
				}
			};

			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
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

		#region Размеры
		public void SetSizes(Size size, SizeType sizeType) {
			var employeeSize = Entity.Sizes.FirstOrDefault(x => x.SizeType.IsSame(sizeType));
			if (size is null) {
				if(employeeSize != null)
					Entity.Sizes.Remove(employeeSize);
			}
			else {
				if (employeeSize is null) {
					var newEmployeeSize = 
						new EmployeeSize {Size = size, SizeType = sizeType, Employee = Entity};
					Entity.Sizes.Add(newEmployeeSize);
				}
				else {
					if (employeeSize.Size != size)
						employeeSize.Size = size;
				}
			}

			if(WearItemsViewModel.IsConfigured) {
				//Если вкладка со списком спецодежды уже открыта, то обновляем предложения номенклатуры.
				WearItemsViewModel.SizeChanged();
			}
		}
		#endregion

		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("employees.html#employee-card");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion
	}
	public class DepartmentJournalViewModelSelector : IEntitySelector {
		private INavigationManager NavigationManager { get; }
		private DialogViewModelBase Parent { get; }
		private EntityEntryViewModel<Subdivision> EntityEntryViewModel { get; }
		public event EventHandler<EntitySelectedEventArgs> EntitySelected;

		public DepartmentJournalViewModelSelector(
			DialogViewModelBase parentViewModel,
			INavigationManager navigationManagerManager,
			EntityEntryViewModel<Subdivision> entityEntityEntryViewModel) 
		{
			NavigationManager = navigationManagerManager;
			Parent = parentViewModel;
			EntityEntryViewModel = entityEntityEntryViewModel;
		}
		public void OpenSelector(string dialogTitle = null) {
			var page = NavigationManager.OpenViewModel<DepartmentJournalViewModel, int?>(
				Parent,
				EntityEntryViewModel.Entity?.Id, 
				OpenPageOptions.AsSlave);
			page.ViewModel.SelectionMode = JournalSelectionMode.Single;
			if (!String.IsNullOrEmpty(dialogTitle)) 
				page.ViewModel.TabName = dialogTitle;
			//Сначала на всякий случай отписываемся от события, вдруг это повторное открытие не не
			page.ViewModel.OnSelectResult -= ViewModelOnSelectResult;
			page.ViewModel.OnSelectResult += ViewModelOnSelectResult;
			void ViewModelOnSelectResult(object sender, JournalSelectedEventArgs e) => 
				EntitySelected?.Invoke(this, new EntitySelectedEventArgs(e.SelectedObjects.First()));
		}
	}
}
