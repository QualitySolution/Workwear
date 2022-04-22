using System.Linq;
using Autofac;
using Gamma.ColumnConfig;
using NHibernate;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Workwear.Domain.Sizes;
using workwear.Domain.Stock;
using workwear.Journal.Filter.ViewModels.Sizes;
using workwear.Journal.Filter.ViewModels.Stock;
using Workwear.Measurements;
using workwear.ViewModels.Sizes;
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
            OverrideDeleteAction();
		}

        protected override IQueryOver<Size> ItemsQuery(IUnitOfWork uow)
        {
            SizeJournalNode resultAlias = null;
            Size sizeAlias = null;
            SizeType sizeTypeAlias = null;
            var query = uow.Session.QueryOver(() => sizeAlias)
                .JoinAlias(() => sizeAlias.SizeType, () => sizeTypeAlias, JoinType.LeftOuterJoin);
            if (Filter.SelectedSizeType != null)
                query.Where(x => x.SizeType.Id == Filter.SelectedSizeType.Id);

            return query
                .Where(GetSearchCriterion(
                    () => sizeAlias.Id,
                    () => sizeAlias.Name,
                    () => sizeTypeAlias.Name
                ))
                .SelectList(list => list
                    .Select(x => x.Id).WithAlias(() => resultAlias.Id)
                    .Select(x => x.Name).WithAlias(() => resultAlias.Name)
                    .Select(() => sizeTypeAlias.Name).WithAlias(() => resultAlias.SizeTypeName)
                )
                .OrderBy(x => x.SizeType).Asc
                .ThenBy(x => x.Name).Asc
                .TransformUsing(Transformers.AliasToBean<SizeJournalNode>());
        }

        private void OverrideDeleteAction() {
            NodeActionsList.RemoveAll(x => x.Title == "Удалить");
            var deleteAction = new JournalAction("Удалить",
                selected => selected.Length == 1 && selected.First().GetId() >= SizeService.MaxStandartSizeId,
                selected => VisibleDeleteAction,
                selected => DeleteEntities(selected.Cast<SizeJournalNode>().ToArray()),
                "Delete"
            );
            NodeActionsList.Add(deleteAction);
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