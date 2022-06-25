using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Autofac;
using Gamma.Utilities;
using Grpc.Core;
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
using QSReport;
using workwear.Domain.Company;
using Workwear.Domain.Company;
using Workwear.Domain.Sizes;
using workwear.Journal.ViewModels.Company;
using Workwear.Measurements;
using workwear.Models.Company;
using workwear.Repository.Company;
using workwear.Repository.Regulations;
using Workwear.Tools;
using workwear.Tools.Features;
using workwear.ViewModels.Company.EmployeeChilds;
using workwear.ViewModels.IdentityCards;

namespace workwear.ViewModels.Company
{
	public class EmployeeViewModel : EntityDialogViewModelBase<EmployeeCard>, IValidatableObject
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public ILifetimeScope AutofacScope;
		public NormRepository NormRepository { get; }
		
		private readonly PersonNames personNames;
		private readonly IInteractiveService interactive;
		private readonly FeaturesService featuresService;
		private readonly EmployeeRepository employeeRepository;
		private readonly LkUserManagerService lkUserManagerService;
		private readonly BaseParameters baseParameters;
		private readonly CommonMessages messages;
		public SizeService SizeService { get; }

		public EmployeeViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IValidator validator,
			IUserService userService,
			ILifetimeScope autofacScope,
			PersonNames personNames,
			IInteractiveService interactive,
			FeaturesService featuresService,
			EmployeeRepository employeeRepository,
			NormRepository normRepository,
			LkUserManagerService lkUserManagerService,
			BaseParameters baseParameters,
			SizeService sizeService,
			CommonMessages messages) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.personNames = personNames ?? throw new ArgumentNullException(nameof(personNames));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			NormRepository = normRepository ?? throw new ArgumentNullException(nameof(normRepository));
			this.lkUserManagerService = lkUserManagerService ?? throw new ArgumentNullException(nameof(lkUserManagerService));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.messages = messages ?? throw new ArgumentNullException(nameof(messages));
			var builder = new CommonEEVMBuilderFactory<EmployeeCard>(this, Entity, UoW, NavigationManager, AutofacScope);
			SizeService = sizeService;

			EntryLeaderViewModel = builder.ForProperty(x => x.Leader)
				.UseViewModelJournalAndAutocompleter<LeadersJournalViewModel>()
				.UseViewModelDialog<LeadersViewModel>()
				.Finish();

			EntrySubdivisionViewModel = builder.ForProperty(x => x.Subdivision)
				.UseViewModelJournalAndAutocompleter<SubdivisionJournalViewModel>()
				.UseViewModelDialog<SubdivisionViewModel>()
				.Finish();

			EntryDepartmentViewModel = builder.ForProperty(x => x.Department)
				.UseViewModelDialog<DepartmentViewModel>()
				.Finish();

			EntryDepartmentViewModel.EntitySelector = new DepartmentJournalViewModelSelector(
				this, NavigationManager, EntrySubdivisionViewModel);

			EntryPostViewModel = builder.ForProperty(x => x.Post)
				.UseViewModelJournalAndAutocompleter<PostJournalViewModel>()
				.UseViewModelDialog<PostViewModel>()
				.Finish();
			
			Entity.PropertyChanged += Entity_PropertyChanged;
			Entity.PropertyChanged += PostChangedCheck;

			if(UoW.IsNew) {
				Entity.CreatedbyUser = userService.GetCurrentUser(UoW);
				logger.Info("Создание карточки для нового сотрудника");
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
			lkLastPhone = Entity.PhoneNumber;
			LkPassword = Entity.LkRegistered ? unknownPassword : String.Empty;

			Validations.Add(new ValidationRequest(this));
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
		public bool VisibleLkRegistration => featuresService.Available(WorkwearFeature.EmployeeLk);
		public bool VisibleColorsLegend => CurrentTab == 3;

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

		public string CardUidEntryColor => 
			String.IsNullOrEmpty(CardUid) || System.Text.RegularExpressions.Regex.IsMatch(CardUid, @"\A\b[0-9a-fA-F]+\b\Z") ? "black" : "red";

		#endregion

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
			try {
				if(String.IsNullOrWhiteSpace(LkPassword) || String.IsNullOrWhiteSpace(Entity.PhoneNumber)) {
					if(Entity.LkRegistered) {
						lkUserManagerService.RemovePhone(lkLastPhone);
						Entity.LkRegistered = false;
					}
					return true;
				}
				if(lkLastPhone != Entity.PhoneNumber && Entity.LkRegistered)
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
			//Так как склад подбора мог поменяться при смене подразделения.
			if(e.PropertyName == nameof(Entity.Subdivision)) {
				Entity.FillWearInStockInfo(UoW, baseParameters, Entity.Subdivision?.Warehouse, DateTime.Now);
				OnPropertyChanged(nameof(SubdivisionAddress));
			}
			if(e.PropertyName == nameof(Entity.FirstName)) {
				var sex = personNames.GetSexByName(Entity.FirstName);
				if(sex != Sex.None)
					Entity.Sex = sex;
			}
			Console.WriteLine();
		}
		private void CheckSizeChanged() {
			Entity.FillWearInStockInfo(UoW, baseParameters, Entity?.Subdivision?.Warehouse, DateTime.Now);
			//Обновляем подобранную номенклатуру
		}
		#endregion

		#region Вкладки
																	// 0 - Информация
																	// 1 - Размеры
		public EmployeeNormsViewModel NormsViewModel;				//2
		public EmployeeWearItemsViewModel WearItemsViewModel; 		//3
		public EmployeeListedItemsViewModel ListedItemsViewModel;  //4
		public EmployeeMovementsViewModel MovementsViewModel;      //5
		public EmployeeVacationsViewModel VacationsViewModel;       //6

		private int lastTab;
		private int currentTab;
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
					if (UoW.IsNew)
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
				case 4: ListedItemsViewModel.OnShow();
					break;
				case 5: MovementsViewModel.OnShow();
					break;
				case 6:
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
				var employeeSamePhone = employeeRepository.GetEmployeeByPhone(UoW, Entity.PhoneNumber);
				if(employeeSamePhone != null && !employeeSamePhone.IsSame(Entity)) {
					if(interactive.Question($"Телефон {Entity.PhoneNumber} уже привязан к сотруднику {employeeSamePhone.ShortName}. " +
					                        $"Удалить у него телефон? Чтобы сохранить {Entity.ShortName}?")) {
						//Здесь сохраняем удаляем телефон через отдельный uow чтобы избежать ошибки базы по уникальному значению поля.
						using(var uow2 = UnitOfWorkFactory.CreateForRoot<EmployeeCard>(employeeSamePhone.Id)) {
							if(uow2.Root.LkRegistered)
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
			[Display(Name = "Лицевая сторона")]
			[ReportIdentifier("Employee.PersonalCardPage1")]
			PersonalCardPage1,
			[Display(Name = "Оборотная сторона")]
			[ReportIdentifier("Employee.PersonalCardPage2")]
			PersonalCardPage2,
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

		void PostChangedCheck(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Post) && lastPost != null && interactive.Question(
				"Установить новую дату изменения должности или перевода в другое структурное подразделение для сотрудника?")) {
				Entity.ChangeOfPositionDate = DateTime.Today;
			}
			if(e.PropertyName == nameof(Entity.Subdivision) && lastSubdivision != null && interactive.Question(
				"Установить новую дату изменения должности или перевода в другое структурное подразделение для сотрудника?")) {
				Entity.ChangeOfPositionDate = DateTime.Today;
			}
			if(e.PropertyName == nameof(Entity.Post) && Entity.Post != null && Entity.UsedNorms.Count == 0 && interactive.Question("Установить норму по должности?")) {
				Entity.NormFromPost(UoW, NormRepository);
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
		public void SetSizes(Size size, SizeType sizeType) {
			CheckSizeChanged();
			var employeeSize = Entity.ObservableSizes.FirstOrDefault(x => x.SizeType == sizeType);
			if (size is null) {
				if(employeeSize != null)
					Entity.ObservableSizes.Remove(employeeSize);
			}
			else {
				if (employeeSize is null) {
					var newEmployeeSize = 
						new EmployeeSize {Size = size, SizeType = sizeType, Employee = Entity};
					Entity.ObservableSizes.Add(newEmployeeSize);
				}
				else {
					if (employeeSize.Size != size)
						employeeSize.Size = size;
				}
			}
		}
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
