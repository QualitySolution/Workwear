using Autofac;
using Gamma.ColumnConfig;
using NHibernate;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using workwear.Domain.Sizes;
using workwear.Domain.Stock;
using workwear.Journal.Filter.ViewModels.Stock;
using workwear.ViewModels.Stock;

namespace workwear.Journal.ViewModels.Stock
{
    public class SizeJournalViewModel: EntityJournalViewModelBase<Size, SizeViewModel, SizeJournalNode>
    {
        public SizeFilterViewModel Filter { get;}
        public SizeJournalViewModel(
            IUnitOfWorkFactory unitOfWorkFactory, 
            IInteractiveService interactiveService, 
            INavigationManager navigationManager,
            ILifetimeScope autofacScope,
            IDeleteEntityService deleteEntityService = null, 
            ICurrentPermissionService currentPermissionService = null
            ) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
        {
			UseSlider = true;
            JournalFilter = Filter = autofacScope.Resolve<SizeFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));
		}

        protected override IQueryOver<Size> ItemsQuery(IUnitOfWork uow)
        {
            SizeJournalNode resultAlias = null;
            Size sizeAlias = null;
            SizeType sizeTypeAlias = null;
            var query = uow.Session.QueryOver(() => sizeAlias)
                .JoinAlias(() => sizeAlias.SizeType, () => sizeTypeAlias, JoinType.LeftOuterJoin);
            if (Filter.SizeType != null)
                query.Where(x => x.SizeType.Id == Filter.SizeType.Id);

            return query
                .Where(GetSearchCriterion(
                    () => sizeAlias.Id,
                    () => sizeAlias.Name
                ))
                .SelectList(list => list
                    .Select(x => x.Id).WithAlias(() => resultAlias.Id)
                    .Select(x => x.Name).WithAlias(() => resultAlias.Name)
                    .Select(() => sizeTypeAlias.Name).WithAlias(() => resultAlias.SizeTypeName)
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
        public string SizeTypeName { get; set; }
    }
}