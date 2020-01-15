using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using workwear.Domain.Company;
using workwear.Domain.Statements;
using workwear.ViewModels.Statements;

namespace workwear.JournalViewModels.Statements
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
			Organization organizationAlias = null;
			Subdivision subdivisionAlias = null;

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
				).TransformUsing(Transformers.AliasToBean<IssuanceSheetJournalNode>());
		}
	}

	public class IssuanceSheetJournalNode : JournalEntityNodeBase<IssuanceSheet>
	{
		public DateTime Date { get; set; }
		public string Organigation { get; set; }
		public string SubdivisionCode { get; set; }
		public string Subdivision { get; set; }
	}
}
