using System;
using System.Collections.Generic;
using System.Linq;
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
using Workwear.Tools;
using Workwear.ViewModels.Company;
using Workwear.ViewModels.Stock;
using Workwear.ViewModels.Stock.Widgets;

namespace Workwear.ViewModels.Visits {
	public class IssuanceRequestViewModel: EntityDialogViewModelBase<IssuanceRequest> {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		private INavigationManager navigation;
		private readonly IInteractiveMessage interactive;
		private readonly EmployeeRepository employeeRepository;
		private readonly IUserService userService;
		private readonly StockBalanceModel stockBalanceModel;
		private readonly EmployeeIssueModel issueModel;
		public BaseParameters BaseParameters { get; }
		
		public IssuanceRequestViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			IUserService userService,
			EmployeeRepository employeeRepository,
			BaseParameters baseParameters,
			StockBalanceModel stockBalanceModel,
			IInteractiveMessage interactive,
			EmployeeIssueModel issueModel,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null): base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) {
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
			BaseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.stockBalanceModel = stockBalanceModel ?? throw new ArgumentNullException(nameof(stockBalanceModel));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.issueModel = issueModel ?? throw new ArgumentNullException(nameof(issueModel));
			
			if(Entity.Id == 0)
				Entity.CreatedByUser = userService.GetCurrentUser();
			
		}

		#region Проброс свойств документа
		public virtual string Id => Entity.Id != 0 ? Entity.Id.ToString() : "Новый";
		public virtual UserBase CreatedByUser => Entity.CreatedByUser;
		public virtual DateTime ReceiptDate => Entity.ReceiptDate;
		public virtual IssuanceRequestStatus Status => Entity.Status;
		public virtual string Comment => Entity.Comment;
		public virtual IObservableList<EmployeeCard> Employees => Entity.Employees;
		public virtual IObservableList<CollectiveExpense> CollectiveExpenses => Entity.CollectiveExpenses;
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
			Dictionary<int, IssueWidgetItem> widgetList = new Dictionary<int, IssueWidgetItem>();
			var pageNewCollectiveExpense = navigation.OpenViewModel<CollectiveExpenseViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
			CollectiveExpenseViewModel newCollectiveExpenseViewModel = pageNewCollectiveExpense.ViewModel;
			newCollectiveExpenseViewModel.Entity.CreatedbyUser = userService.GetCurrentUser();
			newCollectiveExpenseViewModel.Entity.Date = DateTime.Today;
			newCollectiveExpenseViewModel.Entity.IssuanceRequest = Entity;
			
			stockBalanceModel.OnDate = newCollectiveExpenseViewModel.Entity.Date;
			
			foreach (var id in Employees.Select(x => x.Id)) 
				issueModel.PreloadWearItems(id);
			issueModel.FillWearReceivedInfo(Employees.ToArray());
			issueModel.FillWearInStockInfo(Employees, stockBalanceModel);
			
			var needs = Employees
				.SelectMany(x => x.WorkwearItems)
				.Where(x => !newCollectiveExpenseViewModel.Entity.Items.Any(y => y.EmployeeCardItem == x))
				.ToList();
			
			foreach(var item in needs) {
				if(widgetList.ContainsKey(item.ProtectionTools.Id)) {
					widgetList[item.ProtectionTools.Id].NumberOfNeeds++;
					widgetList[item.ProtectionTools.Id].ItemQuantityForIssuse += item.CalculateRequiredIssue(BaseParameters, newCollectiveExpenseViewModel.Entity.Date);
					if(item.CalculateRequiredIssue(BaseParameters, newCollectiveExpenseViewModel.Entity.Date) != 0)
						widgetList[item.ProtectionTools.Id].NumberOfCurrentNeeds++;
				}
				else
					widgetList.Add(item.ProtectionTools.Id, new IssueWidgetItem(item.ProtectionTools,
						item.ProtectionTools.Type.IssueType == IssueType.Collective,
						item.CalculateRequiredIssue(BaseParameters, newCollectiveExpenseViewModel.Entity.Date)>0 ? 1 : 0,
						1,
						item.CalculateRequiredIssue(BaseParameters, newCollectiveExpenseViewModel.Entity.Date),
						stockBalanceModel.ForNomenclature(item.ProtectionTools.Nomenclatures.ToArray())
							.Sum(x =>x.Amount)));
			}
			if(!widgetList.Any()) {
				interactive.ShowMessage(ImportanceLevel.Info, "Нет потребностей для добавления");
				return;
			}
			
			var page = navigation.OpenViewModel<IssueWidgetViewModel, Dictionary<int, IssueWidgetItem>>
			(null, widgetList.OrderByDescending(x => x.Value.Active).ThenBy(x=>x.Value.ProtectionTools.Name)
				.ToDictionary(x => x.Key, x => x.Value));
			page.ViewModel.AddItems = (dic,vac) => AddItemsFromWidget(dic, needs, newCollectiveExpenseViewModel.Entity, page, vac);
			Entity.CollectiveExpenses.Add(newCollectiveExpenseViewModel.Entity);
		}
		public void AddItemsFromWidget(
			Dictionary<int, IssueWidgetItem> widgetItems, 
			List<EmployeeCardItem> needs, 
			CollectiveExpense newCollectiveExpense,
			IPage page, 
			bool excludeOnVacation) 
		{
			foreach(var item in needs) {
				if(widgetItems.First(x => x.Key == item.ProtectionTools.Id).Value.Active)
					if(excludeOnVacation && item.EmployeeCard.OnVacation(newCollectiveExpense.Date))
						continue;
					else
						newCollectiveExpense.AddItem(item, BaseParameters);
			}
			navigation.ForceClosePage(page);
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
