using System;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Visits;
using workwear.Journal.ViewModels.Company;
using Workwear.Repository.Company;
using Workwear.ViewModels.Company;

namespace Workwear.ViewModels.Visits {
	public class IssuanceRequestViewModel: EntityDialogViewModelBase<IssuanceRequest> {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		private INavigationManager navigation;
		private readonly EmployeeRepository employeeRepository;
		public IssuanceRequestViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			IUserService userService,
			EmployeeRepository employeeRepository,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null): base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) {
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
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
		#endregion

		#region Действия View

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
