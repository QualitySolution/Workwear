using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Visits;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using Workwear.Repository.Company;
using Workwear.Repository.Stock;
using Workwear.Tools.Features;
using Workwear.ViewModels.Company;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.Visits {
	public class IssuanceRequestViewModel: EntityDialogViewModelBase<IssuanceRequest> {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		private INavigationManager navigation;
		private readonly IInteractiveQuestion interactive;
		private readonly EmployeeRepository employeeRepository;
		private readonly FeaturesService featuresService;
		private readonly IEntityChangeWatcher changeWatcher;
		
		public IssuanceRequestViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			IUserService userService,
			EmployeeRepository employeeRepository,
			StockRepository stockRepository,
			IInteractiveQuestion interactive,
			ILifetimeScope autofacScope,
			FeaturesService featuresService,
			IEntityChangeWatcher changeWatcher,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null): base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) {
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.changeWatcher = changeWatcher ?? throw new ArgumentNullException(nameof(changeWatcher));
			changeWatcher.BatchSubscribe(IssuanceRequestChangeEvent)
				.IfEntity<CollectiveExpense>();
			if(Entity.Id == 0)
				Entity.CreatedByUser = userService.GetCurrentUser();
			Warehouses = UoW.GetAll<Warehouse>().ToList();
			SelectWarehouse =  stockRepository.GetDefaultWarehouse(UoW, featuresService, userService.CurrentUserId);

			var thisViewModel = new TypedParameter(typeof(IssuanceRequestViewModel), this);
			EmployeeCardItemsViewModel = autofacScope.Resolve<IssuanceRequestEmployeeCardItemsViewModel>(thisViewModel);
		}

		#region Дочерние модели
		public IssuanceRequestEmployeeCardItemsViewModel EmployeeCardItemsViewModel { get; }

		#endregion
		
		#region Проброс свойств документа
		public virtual string Id => Entity.Id != 0 ? Entity.Id.ToString() : "Новый";
		public virtual UserBase CreatedByUser => Entity.CreatedByUser;
		public virtual DateTime ReceiptDate {
			get => Entity.ReceiptDate;
			set => Entity.ReceiptDate = value;
		}
		public virtual IssuanceRequestStatus Status {
			get => Entity.Status;
			set => Entity.Status = value;
		}
		public virtual string Comment {
			get => Entity.Comment;
			set => Entity.Comment = value;
		}
		public virtual IObservableList<EmployeeCard> Employees => Entity.Employees;
		public virtual IObservableList<CollectiveExpense> CollectiveExpenses => Entity.CollectiveExpenses;
		
		private int currentTab = 0;
		[PropertyChangedAlso(nameof(VisibleColorsLegend))]
		public virtual int CurrentTab {
			get => currentTab;
			set {
				SetField(ref currentTab, value);
				if(currentTab == 3) {
					EmployeeCardItemsViewModel.OnShow();
					OnPropertyChanged(nameof(GroupedEmployeeCardItems));
				}
			}
		}
		#endregion

		#region Visible
		public bool VisibleColorsLegend => CurrentTab == 3;
		#endregion

		#region Sensitive
		public bool CanCreateCollectiveExpense => SelectWarehouse != null;
		#endregion
		#region Работа со складом
		private List<Warehouse> warehouses = new List<Warehouse>();
		public List<Warehouse> Warehouses {
			get => warehouses;
			set => SetField(ref warehouses, value);
		}
		private Warehouse selectWarehouse;
		[PropertyChangedAlso(nameof(CanCreateCollectiveExpense))]
		public Warehouse SelectWarehouse {
			get => selectWarehouse;
			set => SetField(ref selectWarehouse, value);
		}
		#endregion
		
		#region Действия View

		#region Сотрудники
		#region Добавление
		public void AddEmployees() {
			var selectJournal = navigation.OpenViewModel<EmployeeJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectJournal.ViewModel.Filter.ShowOnlyWork = true;
			selectJournal.ViewModel.OnSelectResult += LoadEmployees;
		}

		private void LoadEmployees(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {
			var employeeIds = e.GetSelectedObjects<EmployeeJournalNode>().Select(x => x.Id).ToArray();
			var employees = UoW.GetById<EmployeeCard>(employeeIds);
			foreach(var emp in employees)
				Employees.Add(emp);
			EmployeeCardItemsViewModel.UpdateNodes();
		}
		
		public void AddSubdivisions() {
			var selectJournal = navigation.OpenViewModel<SubdivisionJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += LoadSubdivisions;
		}
		private void LoadSubdivisions(object sender, JournalSelectedEventArgs e) {
			
			var subdivisionIds = e.GetSelectedObjects<SubdivisionJournalNode>().Select(x => x.Id).ToArray();
			var employees = employeeRepository.GetActiveEmployeesFromSubdivisions(UoW, subdivisionIds);
			foreach(var emp in employees)
				Employees.Add(emp);
			EmployeeCardItemsViewModel.UpdateNodes();
		}

		public void AddDepartments() {
			var selectJournal = navigation.OpenViewModel<DepartmentJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += LoadDepartments;
		}

		private void LoadDepartments(object sender, JournalSelectedEventArgs e) {
			var departmentIds = e.GetSelectedObjects<DepartmentJournalNode>().Select(x => x.Id).ToArray();
			var employees = employeeRepository.GetActiveEmployeesFromDepartments(UoW, departmentIds);
			foreach(var emp in employees)
				Employees.Add(emp);
			EmployeeCardItemsViewModel.UpdateNodes();
		}

		public void AddGroups() {
			var selectJournal = navigation.OpenViewModel<EmployeeGroupJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += LoadGroups;
		}

		private void LoadGroups(object sender, JournalSelectedEventArgs e) {
			var groupIds = e.GetSelectedObjects<EmployeeGroupJournalNode>().Select(x => x.Id).ToArray();
			var employees = employeeRepository.GetActiveEmployeesFromGroups(UoW, groupIds);
			foreach(var emp in employees)
				Employees.Add(emp);
			EmployeeCardItemsViewModel.UpdateNodes();
		}
		#endregion

		#region Удаление
		public void RemoveEmployees(EmployeeCard[] employees) {
			foreach(var emp in employees) {
				Entity.Employees.Remove(emp);
			}
			EmployeeCardItemsViewModel.UpdateNodes();
		}
		#endregion

		#region Контекстное меню
		public void OpenEmployee(EmployeeCard employee) {
			navigation.OpenViewModel<EmployeeViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForOpen(employee.Id));
		}
		#endregion
		#endregion

		#region Выдачи

		#region Добавление
		public void AddCollectiveExpense() {
			var selectJournal = navigation.OpenViewModel<StockDocumentsJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			selectJournal.ViewModel.Filter.StockDocumentType = StockDocumentType.CollectiveExpense;
			selectJournal.ViewModel.OnSelectResult += LoadCollectiveExpense;
		}
		private void LoadCollectiveExpense(object sender, JournalSelectedEventArgs e) {
			var collectiveExpenseIds = e.GetSelectedObjects<StockDocumentsJournalNode>().Select(x => x.Id).ToArray();
			var collectiveExpense = UoW.GetById<CollectiveExpense>(collectiveExpenseIds);
			foreach(var ce in collectiveExpense) {
				ce.IssuanceRequest = Entity;
				CollectiveExpenses.Add(ce);
			}
			EmployeeCardItemsViewModel.UpdateNodes();
		}
		#endregion

		#region Удаление ссылки из заявки
		public void RemoveCollectiveExpense(CollectiveExpense[] collectiveExpenses) {
			foreach(var ce in collectiveExpenses) {
				ce.IssuanceRequest = null;
				Entity.CollectiveExpenses.Remove(ce);
			}
			EmployeeCardItemsViewModel.UpdateNodes();
		}
		#endregion

		#endregion

		#region Создание документа коллективной выдачи
		public void CreateCollectiveExpense() {
			if(UoW.HasChanges) {
				if(!interactive.Question("Перед созданием документа коллективной выдачи необходимо сохранить заявку. Сохранить?") || !Save())	
					return;
			}
			navigation.OpenViewModel<CollectiveExpenseViewModel, IEntityUoWBuilder, 
				IssuanceRequest, Warehouse>(this, EntityUoWBuilder.ForCreate(), Entity, SelectWarehouse);
		}

		private IList<CollectiveExpense> LoadCollectiveExpenses() {
			var collectiveExpenses = UoW.GetAll<CollectiveExpense>()
				 .Where(x => x.IssuanceRequest.Id == Entity.Id)
				 .ToList();
			return collectiveExpenses;
		}
		private void IssuanceRequestChangeEvent(EntityChangeEvent[] changeevents) {
			Entity.CollectiveExpenses.Clear();
			foreach(var doc in  LoadCollectiveExpenses())
				Entity.CollectiveExpenses.Add(doc);
			EmployeeCardItemsViewModel.UpdateNodes(false);
			OnPropertyChanged(nameof(GroupedEmployeeCardItems));
		}
		
		#endregion

		#region Потребности
		public IObservableList<EmployeeCardItemsVmNode> GroupedEmployeeCardItems => EmployeeCardItemsViewModel.GroupedList;
		#endregion
		
		#endregion
		
		#region Валидация, сохранение
		public override bool Save() {
			logger.Info ("Запись заявки...");
			
			logger.Info("Валидация...");
			if(!Validate()) {
				logger.Warn("Валидация не пройдена, сохранение отменено.");
				return false;
			} else 
				logger.Info("Валидация пройдена.");
			if(Entity.Id == 0) 
				Entity.CreationDate = DateTime.Today;
			UoW.Save();
			logger.Info("Заявка сохранена");
			return true;
		}
		#endregion
	}
}
