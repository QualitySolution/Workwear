using NHibernate.Transform;
using NHibernate;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Journal;
using QS.Project.Services;
using Workwear.Domain.Stock;
using Workwear.ViewModels.Stock;

namespace workwear.Journal.ViewModels.Stock {
	public class CauseIssueJournalViewModel: EntityJournalViewModelBase<CausesIssue, CauseIssueViewModel, CauseIssueJournalNode>
	{
		public CauseIssueJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, IDeleteEntityService deleteEntityService, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
		}

		protected override IQueryOver<CausesIssue> ItemsQuery(IUnitOfWork uow)
		{
			CauseIssueJournalNode resultAlias = null;
			CausesIssue CauseAlias = null;

			return uow.Session.QueryOver<CausesIssue>(() => CauseAlias)
				.Where(GetSearchCriterion(
					() => CauseAlias.Id,
					() => CauseAlias.Name
				))
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
				).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<CauseIssueJournalNode>());
		}
	}

	public class CauseIssueJournalNode 
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
}
