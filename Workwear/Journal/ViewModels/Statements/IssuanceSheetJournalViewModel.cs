using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using workwear.Domain.Company;
using workwear.Domain.Statements;
using workwear.ViewModels.Statements;

namespace workwear.Journal.ViewModels.Statements
{
	public class IssuanceSheetJournalViewModel : EntityJournalViewModelBase<IssuanceSheet, IssuanceSheetViewModel, IssuanceSheetJournalNode>
	{
		public IssuanceSheetJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager = null, IDeleteEntityService deleteEntityService = null, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
		}

		protected override IQueryOver<IssuanceSheet> ItemsQuery(IUnitOfWork uow)
		{
			IssuanceSheetJournalNode resultAlias = null;

			IssuanceSheet issuanceSheetAlias = null;
			IssuanceSheetItem issuanceSheetItemAlias = null;
			Organization organizationAlias = null;
			Subdivision subdivisionAlias = null;
			EmployeeCard employeeCardAlias = null;

			var employeesSubquery = QueryOver.Of<IssuanceSheetItem>(() => issuanceSheetItemAlias)
				.Where(() => issuanceSheetItemAlias.IssuanceSheet.Id == issuanceSheetAlias.Id)
				.JoinQueryOver(x => x.Employee, () => employeeCardAlias)
				.Select(CustomProjections.GroupConcat(Projections.Property(() => employeeCardAlias.LastName), useDistinct: true, separator: ", "));

			return uow.Session.QueryOver<IssuanceSheet>(() => issuanceSheetAlias)
				.Where(GetSearchCriterion(
					() => issuanceSheetAlias.Id,
					() => organizationAlias.Name,
					() => subdivisionAlias.Name,
					() => subdivisionAlias.Code
					))
				.Left.JoinAlias(s => s.Organization, () => organizationAlias)
				.Left.JoinAlias(s => s.Subdivision, () => subdivisionAlias)
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Date).WithAlias(() => resultAlias.Date)
					.Select(() => organizationAlias.Name).WithAlias(() => resultAlias.Organigation)
					.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.Subdivision)
					.Select(() => subdivisionAlias.Code).WithAlias(() => resultAlias.SubdivisionCode)
					.Select(x => x.Expense.Id).WithAlias(() => resultAlias.DocExpense)
					.Select(x => x.MassExpense.Id).WithAlias(() => resultAlias.DocMassExpense)
					.SelectSubQuery(employeesSubquery).WithAlias(() => resultAlias.Employees)
				).TransformUsing(Transformers.AliasToBean<IssuanceSheetJournalNode>());
		}
	}

	public class IssuanceSheetJournalNode : JournalEntityNodeBase<IssuanceSheet>
	{
		public DateTime Date { get; set; }
		public string Organigation { get; set; }
		public string SubdivisionCode { get; set; }
		public string Subdivision { get; set; }

		public int? DocExpense { get; set; }
		public int? DocMassExpense { get; set; }
		public string Document { 
			get{
				if(DocExpense != null)
					return $"Выдача №{DocExpense}";
				if(DocMassExpense != null)
					return $"Массовая выдача №{DocMassExpense}";
				return null;
			} }

		public string Employees { get; set; }
	}
}
