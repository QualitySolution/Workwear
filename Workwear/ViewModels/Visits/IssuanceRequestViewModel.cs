using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Dialog;
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
using Workwear.Models.Operations;
using Workwear.Repository.Company;
using Workwear.Repository.Stock;
using workwear.Representations.Organization;
using Workwear.Tools;
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
		private readonly StockBalanceModel stockBalanceModel;
		private readonly EmployeeIssueModel issueModel;
		private readonly BaseParameters baseParameters;
		private bool alreadyLoaded;
		
		public IssuanceRequestViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			IUserService userService,
			EmployeeRepository employeeRepository,
			StockRepository stockRepository,
			IInteractiveQuestion interactive,
			ILifetimeScope autofacScope,
			StockBalanceModel stockBalanceModel,
			EmployeeIssueModel issueModel,
			BaseParameters baseParameters,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null): base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) {
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.stockBalanceModel = stockBalanceModel ?? throw new ArgumentNullException(nameof(stockBalanceModel));
			this.issueModel = issueModel ?? throw new ArgumentNullException(nameof(issueModel));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			featuresService = autofacScope.Resolve<FeaturesService>();
			
			if(Entity.Id == 0)
				Entity.CreatedByUser = userService.GetCurrentUser();
			Warehouses = UoW.GetAll<Warehouse>().ToList();
			DefaultWarehouse = stockRepository.GetDefaultWarehouse(UoW, featuresService, userService.CurrentUserId);
			SelectWarehouse = DefaultWarehouse ?? Warehouses.FirstOrDefault();
		}

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
		#endregion

		#region Работа со складом
		
		private Warehouse defaultWarehouse;
		public virtual Warehouse DefaultWarehouse {
			get => defaultWarehouse;
			set => SetField(ref defaultWarehouse, value);
		}
		private List<Warehouse> warehouses = new List<Warehouse>();
		public virtual List<Warehouse> Warehouses {
			get => warehouses;
			set => SetField(ref warehouses, value);
		}
		
		private Warehouse selectWarehouse;
		public virtual Warehouse SelectWarehouse {
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
		}
		#endregion

		#region Удаление

		public void RemoveEmployees(EmployeeCard[] employees) {
			foreach(var emp in employees) {
				Entity.Employees.Remove(emp);
			}
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
		}

		#endregion

		#region Удаление ссылки из заявки

		public void RemoveCollectiveExpense(CollectiveExpense[] collectiveExpenses) {
			foreach(var ce in collectiveExpenses) {
				ce.IssuanceRequest = null;
				Entity.CollectiveExpenses.Remove(ce);
			}
		}

		#endregion

		#endregion

		#region Создание документа коллективной выдачи

		public void CreateCollectiveExpense() {
			if(UoW.HasChanges) {
				if(!interactive.Question("Перед созданием документа коллективной выдачи необходимо сохранить заявку. Сохранить?") || !Save())	
					return;
			}
			var pageNewCollectiveExpense = navigation.OpenViewModel<CollectiveExpenseViewModel, IEntityUoWBuilder, 
				IssuanceRequest, Warehouse>(this, EntityUoWBuilder.ForCreate(), Entity, SelectWarehouse);
		}

		#endregion

		#region Потребности
		
		private EmployeeWearItemsVM employeeWearItemsVm;
		public EmployeeWearItemsVM EmployeeWearItemsVm {
			get => employeeWearItemsVm;
			set => SetField(ref employeeWearItemsVm, value);
		}
		
		private bool isConfigured = false;
		public void OnShow() {
			stockBalanceModel.OnDate = Entity.ReceiptDate;
			issueModel.FillWearInStockInfo(Employees, stockBalanceModel);
			issueModel.FillWearReceivedInfo(Employees.ToArray());
			if(!isConfigured) {
				isConfigured = true;
				EmployeeWearItemsVm = new EmployeeWearItemsVM(stockBalanceModel, issueModel, baseParameters, UoW) {
					IssuanceRequest = Entity
				};
			}
		}
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
