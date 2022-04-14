using NHibernate;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using workwear.Domain.Company;
using workwear.ViewModels.Company;

namespace workwear.Journal.ViewModels.Company
{
	public class SubdivisionJournalViewModel : EntityJournalViewModelBase<Subdivision, SubdivisionViewModel, SubdivisionJournalNode>
	{
		public SubdivisionJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, IDeleteEntityService deleteEntityService = null, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
		}

		protected override IQueryOver<Subdivision> ItemsQuery(IUnitOfWork uow)
		{
			SubdivisionJournalNode resultAlias = null;
			Subdivision parentSubdivisionAlias = null;
			Subdivision subdivisionAlias = null;
			
			return uow.Session.QueryOver(() => subdivisionAlias)
				.Where(GetSearchCriterion<Subdivision>(
					x => x.Code,
					x => x.Name,
					x => x.Address
					))
				.JoinAlias(() => subdivisionAlias.ParentSubdivision, () => parentSubdivisionAlias, JoinType.LeftOuterJoin)
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Code).WithAlias(() => resultAlias.Code)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(x => x.Address).WithAlias(() => resultAlias.Address)
					.Select(() => parentSubdivisionAlias.Name).WithAlias(() => resultAlias.ParentName)
				)
				.OrderBy(x => x.ParentSubdivision).Desc
				.ThenBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<SubdivisionJournalNode>());
		}
	}

	public class SubdivisionJournalNode
	{
		public int Id { get; set; }
		public string Code { get; set; }
		public string Name { get; set; }
		public string Address { get; set; }
		public string ParentName { get; set; }
	}
}
