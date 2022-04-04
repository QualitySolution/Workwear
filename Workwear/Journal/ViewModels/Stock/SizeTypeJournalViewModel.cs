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
    public class SizeTypeJournalViewModel: EntityJournalViewModelBase<SizeType, SizeTypeViewModel, SizeTypeJournalNode>
    {
        public SizeTypeJournalViewModel(
            IUnitOfWorkFactory unitOfWorkFactory, 
            IInteractiveService interactiveService, 
            INavigationManager navigationManager, 
            IDeleteEntityService deleteEntityService = null, 
            ICurrentPermissionService currentPermissionService = null
            ) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
        {
			UseSlider = true;
		}

        protected override IQueryOver<SizeType> ItemsQuery(IUnitOfWork uow)
        {
            SizeTypeJournalNode resultAlias = null;
            SizeType sizeTypeAlias = null;
            var query = uow.Session.QueryOver(() => sizeTypeAlias);

            return query
                .Where(GetSearchCriterion(
                    () => sizeTypeAlias.Id,
                    () => sizeTypeAlias.Name
                ))
                .SelectList(list => list
                    .Select(x => x.Id).WithAlias(() => resultAlias.Id)
                    .Select(x => x.Name).WithAlias(() => resultAlias.Name)
                ).OrderBy(x => x.Name).Asc
                .TransformUsing(Transformers.AliasToBean<SizeTypeJournalNode>());
        }
    }

    public class SizeTypeJournalNode
    {
        [SearchHighlight]
        public int Id { get; set; }
        [SearchHighlight]
        public string Name { get; set; }
    }
}