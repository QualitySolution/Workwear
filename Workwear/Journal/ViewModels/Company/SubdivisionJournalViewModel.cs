using Autofac;
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
using workwear.Journal.Filter.ViewModels.Company;
using workwear.ViewModels.Company;

namespace workwear.Journal.ViewModels.Company
{
	public class SubdivisionJournalViewModel : EntityJournalViewModelBase<Subdivision, SubdivisionViewModel, SubdivisionJournalNode>
	{
		public SubdivisionFilterViewModel Filter { get; }
		public SubdivisionJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigationManager, 
			ILifetimeScope autofacScope,
			IDeleteEntityService deleteEntityService = null, 
			ICurrentPermissionService currentPermissionService = null
			) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			AutofacScope = autofacScope;
			JournalFilter = Filter = AutofacScope.Resolve<SubdivisionFilterViewModel>(
				new TypedParameter(typeof(JournalViewModelBase), this));
		}
		protected override IQueryOver<Subdivision> ItemsQuery(IUnitOfWork uow)
		{
			SubdivisionJournalNode resultAlias = null;
			Subdivision parentSubdivisionAlias = null;
			Subdivision subdivisionAlias = null;

			var query = uow.Session.QueryOver(() => subdivisionAlias)
				.Where(GetSearchCriterion<Subdivision>(
					x => x.Code,
					x => x.Name,
					x => x.Address
				))
				.JoinAlias(() => subdivisionAlias.ParentSubdivision, () => parentSubdivisionAlias,
					JoinType.LeftOuterJoin)
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Code).WithAlias(() => resultAlias.Code)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(x => x.Address).WithAlias(() => resultAlias.Address)
					.Select(() => parentSubdivisionAlias.Name).WithAlias(() => resultAlias.ParentName)
				);
			if (Filter.SortByParent)
				query = query
					.OrderBy(() => parentSubdivisionAlias.Name).Asc
					.ThenBy(x => x.Name).Asc;
			else
				query = query.OrderBy(x => x.Name).Asc;
			return query.TransformUsing(Transformers.AliasToBean<SubdivisionJournalNode>());
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
