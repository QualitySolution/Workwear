using System;
using System.Linq;
using NHibernate;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.ViewModels;
using Workwear.Domain.Company;
using workwear.Journal.ViewModels.Company;
using Workwear.Repository.Company;
using Workwear.Repository.Operations;

namespace Workwear.ViewModels.Statements
{
	public class IssuanceSheetFillByViewModel : ViewModelBase
	{
		readonly IssuanceSheetViewModel issuanceSheetViewModel;
		readonly EmployeeIssueRepository employeeIssueRepository;
		private readonly EmployeeRepository employeeRepository;
		readonly IInteractiveQuestion question;

		#region Notify

		private DateTime? beginDate;
		[PropertyChangedAlso(nameof(SensitiveFillButton))]
		public virtual DateTime? BeginDate {
			get => beginDate;
			set => SetField(ref beginDate, value);
		}

		private DateTime? endDate;
		[PropertyChangedAlso(nameof(SensitiveFillButton))]
		public virtual DateTime? EndDate {
			get => endDate;
			set => SetField(ref endDate, value);
		}

		#endregion

		public IssuanceSheetFillByMode Mode;

		#region Sensetive

		public bool SensitiveFillButton => BeginDate.HasValue && EndDate.HasValue && ObservableEmployees.Count > 0;

		#endregion

		#region Helpers

		IUnitOfWork UoW => issuanceSheetViewModel.UoW;

		#endregion

		public readonly IObservableList<EmployeeCard> ObservableEmployees = new ObservableList<EmployeeCard>();

		public IssuanceSheetFillByViewModel(IssuanceSheetViewModel issuanceSheetViewModel, EmployeeIssueRepository employeeIssueRepository, EmployeeRepository employeeRepository, IInteractiveQuestion question)
		{
			this.issuanceSheetViewModel = issuanceSheetViewModel ?? throw new ArgumentNullException(nameof(issuanceSheetViewModel));
			this.employeeIssueRepository = employeeIssueRepository;
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			this.question = question;

			employeeIssueRepository.RepoUow = UoW;
		}

		#region Команды	

		public void RemoveEmployees(EmployeeCard[] employees)
		{
			foreach(var employee in employees) {
				ObservableEmployees.Remove(employee);
			}
			OnPropertyChanged(nameof(SensitiveFillButton));
		}
		public void AddEmployees()
		{
			var selectPage = issuanceSheetViewModel.NavigationManager.OpenViewModel<EmployeeJournalViewModel>(
				issuanceSheetViewModel,
				OpenPageOptions.AsSlave);

			var selectDialog = selectPage.ViewModel;
			selectDialog.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectDialog.OnSelectResult += SelectDialog_OnSelectResult;
		}

		void SelectDialog_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			var employeesToAdd = issuanceSheetViewModel.UoW.GetById<EmployeeCard>(e.SelectedObjects.Select(DomainHelper.GetId));

			foreach(var employee in employeesToAdd) {
				if(!ObservableEmployees.Contains(employee))
					ObservableEmployees.Add(employee);
			}
			OnPropertyChanged(nameof(SensitiveFillButton));
		}

		public void AddEmployeesFromDivision()
		{
			var selectPage = issuanceSheetViewModel.NavigationManager.OpenViewModel<SubdivisionJournalViewModel>(
				issuanceSheetViewModel,
				OpenPageOptions.AsSlave
			);

			var selectSubdivisionDialog = selectPage.ViewModel;
			selectSubdivisionDialog.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectSubdivisionDialog.OnSelectResult += SelectSubdivisionDialog_OnSelectResult;;
		}

		void SelectSubdivisionDialog_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			foreach(var subdivisionNode in e.GetSelectedObjects<SubdivisionJournalNode>()) {
				if(issuanceSheetViewModel.Entity.Subdivision == null)
					issuanceSheetViewModel.Entity.Subdivision = issuanceSheetViewModel.UoW.GetById<Subdivision>(subdivisionNode.Id);
				var inSubdivision = employeeRepository.GetActiveEmployeesFromSubdivisions(issuanceSheetViewModel.UoW, new int[] {subdivisionNode.Id });
				foreach(var employee in inSubdivision) {
					if(ObservableEmployees.All(x => x.Id != employee.Id))
						ObservableEmployees.Add(employee);
				}
			}
			OnPropertyChanged(nameof(SensitiveFillButton));
		}

		public void FillIssuanceSheet()
		{
			if(issuanceSheetViewModel.Entity.Items.Count > 0) {
				if(question.Question("Табличная часть ведомости не пустая. Заполнение очистить имеющиеся данные. Продолжить?"))
					issuanceSheetViewModel.Entity.Items.Clear();
				else
					return;
			}

			switch(Mode) {
				case IssuanceSheetFillByMode.ByExpense:
					FillByExpense();
					break;
				case IssuanceSheetFillByMode.ByNeed:
					FillByNeed();
					break;
			}

			issuanceSheetViewModel.CloseFillBy();
		}

		private void FillByExpense()
		{
			var issueOperations = employeeIssueRepository.GetOperationsByDates( 
				ObservableEmployees.ToArray(), 
				BeginDate.Value,
				EndDate.Value,
				x => x.Fetch(SelectMode.Fetch, f => f.Nomenclature));

			foreach(var operation in issueOperations.OrderBy(x => x.Employee.FullName).ThenBy(x => x.OperationTime)) {
				issuanceSheetViewModel.Entity.AddItem(operation);
			}
		}

		private void FillByNeed()
		{
			var items = EmployeeRepository.GetItems(issuanceSheetViewModel.UoW,
				ObservableEmployees.ToArray(),
				BeginDate.Value,
				EndDate.Value);
			foreach(var item in items.OrderBy(x => x.EmployeeCard.FullName).ThenBy(x => x.NextIssue)) {
				issuanceSheetViewModel.Entity.AddItem(item);
			}
		}

		#endregion
	}

	public enum IssuanceSheetFillByMode
	{
		ByExpense,
		ByNeed
	}
}
