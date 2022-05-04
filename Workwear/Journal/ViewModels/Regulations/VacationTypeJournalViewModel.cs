using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using workwear.Domain.Company;
using workwear.ViewModels.Regulations;

namespace workwear.Journal.ViewModels.Regulations
{
    public class VacationTypeJournalViewModel : EntityJournalViewModelBase<VacationType, VacationTypeViewModel, VacationTypeJournalNode>
    {
        public VacationTypeJournalViewModel(
            IUnitOfWorkFactory unitOfWorkFactory, 
            IInteractiveService interactiveService, 
            INavigationManager navigationManager, 
            IDeleteEntityService deleteEntityService = null, 
            ICurrentPermissionService currentPermissionService = null
            ) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
        {
        }

        protected override IQueryOver<VacationType>ItemsQuery(IUnitOfWork uow)
        {
            VacationTypeJournalNode resultAlias = null;
            return uow.Session.QueryOver<VacationType>()
                .Where(GetSearchCriterion<VacationType>(
                    x => x.Id,
                    x => x.Name
                ))
                .SelectList((list) => list
                    .Select(x => x.Name).WithAlias(() => resultAlias.Name)
                    .Select(x => x.ExcludeFromWearing).WithAlias(() => resultAlias.ExcludeFromWearing)
                    .Select(x => x.Comments).WithAlias(() => resultAlias.Comments)
                ).OrderBy(x => x.Name).Asc
                .TransformUsing(Transformers.AliasToBean<VacationTypeJournalNode>());
        }
    }

    public class VacationTypeJournalNode
    {
        public string Name { get; set; }
        public bool ExcludeFromWearing { get; set; }
        public string Comments { get; set; }
    }
}