using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Autofac;
using NHibernate;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.ViewModels;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using workwear.Domain.Company;
using workwear.Repository;
using workwear.Repository.Operations;
using workwear.Representations.Organization;

namespace workwear.ViewModels.Statements
{
	public class IssuanceSheetFillByViewModel : ViewModelBase
	{
		readonly IssuanceSheetViewModel issuanceSheetViewModel;
		readonly EmployeeIssueRepository employeeIssueRepository;
		readonly IInteractiveQuestion question;

		#region Notify

		private DateTime? beginDate;
		[PropertyChangedAlso(nameof(SensetiveFillButton))]
		public virtual DateTime? BeginDate {
			get => beginDate;
			set => SetField(ref beginDate, value);
		}

		private DateTime? endDate;
		[PropertyChangedAlso(nameof(SensetiveFillButton))]
		public virtual DateTime? EndDate {
			get => endDate;
			set => SetField(ref endDate, value);
		}

		#endregion

		public IssuanceSheetFillByMode Mode;

		#region Sensetive

		public bool SensetiveFillButton => BeginDate.HasValue && EndDate.HasValue && employees.Count > 0;

		#endregion

		List<EmployeeCard> employees = new List<EmployeeCard>();

		GenericObservableList<EmployeeCard> observableEmployees;

		public GenericObservableList<EmployeeCard> ObservableEmployees { get {
				if(observableEmployees == null)
					observableEmployees = new GenericObservableList<EmployeeCard>(employees);

				return observableEmployees;
			}
		}

		public IssuanceSheetFillByViewModel(IssuanceSheetViewModel issuanceSheetViewModel, EmployeeIssueRepository employeeIssueRepository, IInteractiveQuestion question)
		{
			this.issuanceSheetViewModel = issuanceSheetViewModel ?? throw new ArgumentNullException(nameof(issuanceSheetViewModel));
			this.employeeIssueRepository = employeeIssueRepository;
			this.question = question;
		}

		#region Команды	

		public void RemoveEmployees(EmployeeCard[] employees)
		{
			foreach(var employee in employees) {
				ObservableEmployees.Remove(employee);
			}
			OnPropertyChanged(nameof(SensetiveFillButton));
		}
		public void AddEmployees()
		{
			var selectPage = issuanceSheetViewModel.tdiNavigationManager.OpenTdiTab<ReferenceRepresentation>(
				issuanceSheetViewModel.TdiTab,
				OpenPageOptions.AsSlave,
				c => c.RegisterType<EmployeesVM>().As<IRepresentationModel>()
			);

			var selectDialog = selectPage.TdiTab as ReferenceRepresentation;
			selectDialog.Mode = OrmReferenceMode.MultiSelect;
			selectDialog.ObjectSelected += SelectDialog_ObjectSelected;
		}

		void SelectDialog_ObjectSelected(object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			var emploeesToAdd = issuanceSheetViewModel.UoW.GetById<EmployeeCard>(e.GetSelectedIds());

			foreach(var employee in emploeesToAdd) {
				ObservableEmployees.Add(employee);
			}
			OnPropertyChanged(nameof(SensetiveFillButton));
		}

		public void AddEmployeesFromDivision()
		{
			var selectPage = issuanceSheetViewModel.tdiNavigationManager.OpenTdiTab<OrmReference, Type>(
				issuanceSheetViewModel.TdiTab,
				typeof(Subdivision),
				OpenPageOptions.AsSlave
			);

			var selectSubdivisionDialog = selectPage.TdiTab as OrmReference;
			selectSubdivisionDialog.Mode = OrmReferenceMode.MultiSelect;
			selectSubdivisionDialog.ObjectSelected += SelectSubdivisionDialog_ObjectSelected;;
		}

		void SelectSubdivisionDialog_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			foreach(var subdivision in e.GetEntities<Subdivision>()) {
				if(issuanceSheetViewModel.Entity.Subdivision == null)
					issuanceSheetViewModel.Entity.Subdivision = subdivision;
				var inSubdivision = EmployeeRepository.GetActiveEmployeesFromSubdivision(issuanceSheetViewModel.UoW, subdivision);
				foreach(var employee in inSubdivision) {
					if(employees.All(x => x.Id != employee.Id))
						observableEmployees.Add(employee);
				}
			}
			OnPropertyChanged(nameof(SensetiveFillButton));
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
			var issueOperations = employeeIssueRepository.GetOperationsTouchDates(issuanceSheetViewModel.UoW, 
				employees.ToArray(), 
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
				employees.ToArray(),
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
