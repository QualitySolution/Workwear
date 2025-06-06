using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Journal;
using QS.Project.Services;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Domain.Visits;
using Workwear.ViewModels.Visits;

namespace workwear.Journal.ViewModels.Visits {
	public class VisitJournalViewModel: EntityJournalViewModelBase<Visit, VisitViewModel, VisitJournalNode>, IDialogDocumentation
	{
		public VisitJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, IDeleteEntityService deleteEntityService = null, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
		}
		
		protected override IQueryOver<Visit> ItemsQuery(IUnitOfWork uow) {
			VisitJournalNode resultAlias = null;
			EmployeeCard employeeAlias = null;

			var query = uow.Session.QueryOver<Visit>()
				.Where(GetSearchCriterion<Visit>(
					x => x.Id
				));
			
			var concatProjectionEmployee = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(' ', ?1, ?2, ?3)"),
				NHibernateUtil.String,
				Projections.Property(() => employeeAlias.LastName),
				Projections.Property(() => employeeAlias.FirstName),
				Projections.Property(() => employeeAlias.Patronymic));
			
			var result = query
				.JoinAlias(x => x.Employee, () => employeeAlias, JoinType.LeftOuterJoin)
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.CreateDate).WithAlias(() => resultAlias.CreateDate)
					.Select(x => x.VisitTime).WithAlias(() => resultAlias.VisitDate)
					.Select(x => x.EmployeeCreate).WithAlias(() => resultAlias.EmployeeCreate)
					.Select(x => x.Cancelled).WithAlias(() => resultAlias.Cancelled)
					.Select(x => x.Done).WithAlias(() => resultAlias.Done)
					.Select(x => x.Comment).WithAlias(() => resultAlias.Comment)
					.Select(concatProjectionEmployee).WithAlias(() => resultAlias.FIO)
				)
				.OrderBy(x => x.VisitTime).Asc
				.TransformUsing(Transformers.AliasToBean<VisitJournalNode>());

			return result;
		}

		public string DocumentationUrl { get; }
		public string ButtonTooltip { get; }
	}

	public class VisitJournalNode {
		
		public int Id { get; set; }
		public DateTime CreateDate { get; set; }
		public DateTime VisitDate { get; set; }
		public string FIO { get; set; }
////EmployeeCard Employee
		public bool EmployeeCreate { get; set; }
		public bool Done { get; set; }
		public bool Cancelled { get; set; }
		public string Comment { get; set; }
		protected EmployeeCard EmployeeCard { get; set; }
	}
}
