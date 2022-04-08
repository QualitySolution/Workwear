using System.Linq;
using Autofac;
using Gamma.ColumnConfig;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using workwear.Domain.Sizes;
using workwear.Journal.Filter.ViewModels.Stock;
using workwear.ViewModels.Stock;

namespace workwear.Journal.ViewModels.Stock
{
    public class SizeTypeJournalViewModel: EntityJournalViewModelBase<SizeType, SizeTypeViewModel, SizeTypeJournalNode>
    {
        public SizeTypeFilterViewModel Filter { get;}
        public SizeTypeJournalViewModel(
            IUnitOfWorkFactory unitOfWorkFactory, 
            IInteractiveService interactiveService, 
            INavigationManager navigationManager,
            ILifetimeScope autofacScope,
            IDeleteEntityService deleteEntityService = null, 
            ICurrentPermissionService currentPermissionService = null
            ) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
        {
			UseSlider = true;
            JournalFilter = Filter = autofacScope.Resolve<SizeTypeFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));
            OverrideDeleteAction();
		}

        protected override IQueryOver<SizeType> ItemsQuery(IUnitOfWork uow)
        {
            SizeTypeJournalNode resultAlias = null;
            SizeType sizeTypeAlias = null;
            var query = uow.Session.QueryOver(() => sizeTypeAlias);

            if (Filter.Category != null)
                query.Where(x => x.Category == Filter.Category);

            return query
                .Where(GetSearchCriterion(
                    () => sizeTypeAlias.Id,
                    () => sizeTypeAlias.Name
                ))
                .SelectList(list => list
                    .Select(x => x.Id).WithAlias(() => resultAlias.Id)
                    .Select(x => x.Name).WithAlias(() => resultAlias.Name)
                    .Select(x => x.Category).WithAlias(() => resultAlias.Category)
                    .Select(x => x.Position).WithAlias(() => resultAlias.Position)
                ).OrderBy(x => x.Position).Asc
                .TransformUsing(Transformers.AliasToBean<SizeTypeJournalNode>());
        }
        private void OverrideDeleteAction() {
            NodeActionsList.RemoveAll(x => x.Title == "Удалить");
            var deleteAction = new JournalAction("Удалить",
                (selected) => selected.Length == 1 && selected.First().GetId() >= 100,
                (selected) => VisibleDeleteAction,
                (selected) => DeleteEntities((SizeTypeJournalNode[]) selected.ToArray()),
                "Delete"
            );
            NodeActionsList.Add(deleteAction);
        }
    }

    public class SizeTypeJournalNode
    {
        [SearchHighlight]
        public int Id { get; set; }
        [SearchHighlight]
        public string Name { get; set; }
        [SearchHighlight]
        public Category Category { get; set; }
        public int Position { get; set; }
    }
}