using Gamma.ColumnConfig;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using workwear.Domain.Sizes;
using workwear.Domain.Stock;
using workwear.ViewModels.Stock;

namespace workwear.Journal.ViewModels.Stock
{
    public class SizeJournalViewModel: EntityJournalViewModelBase<Size, SizeViewModel, SizeJournalNode>
    {
        public SizeJournalViewModel(
            IUnitOfWorkFactory unitOfWorkFactory, 
            IInteractiveService interactiveService, 
            INavigationManager navigationManager, 
            IDeleteEntityService deleteEntityService = null, 
            ICurrentPermissionService currentPermissionService = null
            ) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
        {
        }

        protected override IQueryOver<Size> ItemsQuery(IUnitOfWork uow)
        {
            SizeJournalNode resultAlias = null;
            Size sizeAlias = null;
            var query = uow.Session.QueryOver(() => sizeAlias);

            return query
                .Where(GetSearchCriterion(
                    () => sizeAlias.Id,
                    () => sizeAlias.Name
                ))
                .SelectList(list => list
                    .Select(x => x.Id).WithAlias(() => resultAlias.Id)
                    .Select(x => x.Name).WithAlias(() => resultAlias.Name)
                ).OrderBy(x => x.Name).Asc
                .TransformUsing(Transformers.AliasToBean<SizeJournalNode>());
        }
    }

    public class SizeJournalNode
    {
        [SearchHighlight]
        public int Id { get; set; }
        [SearchHighlight]
        public string Name { get; set; }
    }
}