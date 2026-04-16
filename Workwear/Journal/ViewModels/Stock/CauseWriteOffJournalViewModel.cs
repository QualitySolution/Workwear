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
	public class CauseWriteOffJournalViewModel: EntityJournalViewModelBase<CausesWriteOff, CauseWriteOffViewModel, CauseWriteOffJournalNode>
	{
		public CauseWriteOffJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, IDeleteEntityService deleteEntityService, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
		}

		protected override IQueryOver<CausesWriteOff> ItemsQuery(IUnitOfWork uow)
		{
			CauseWriteOffJournalNode resultAlias = null;
			CausesWriteOff CauseAlias = null;

			return uow.Session.QueryOver<CausesWriteOff>(() => CauseAlias)
				.Where(GetSearchCriterion(
					() => CauseAlias.Id,
					() => CauseAlias.Name
				))
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
				).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<CauseWriteOffJournalNode>());
		}
	}

	public class CauseWriteOffJournalNode 
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
}
