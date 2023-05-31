using Autofac;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Workwear.Domain.Company;
using Workwear.ViewModels.Company;
using Workwear.Journal.Filter.ViewModels.Company;

namespace workwear.Journal.ViewModels.Company {

	public class DepartmentJournalViewModel : EntityJournalViewModelBase<Department, DepartmentViewModel, DepartmentJournalNode> {
		private DepartmentFilterViewModel Filter;
		public DepartmentJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService,
			ILifetimeScope autofacScope,
			INavigationManager navigationManager, 
			int? parentSubdivisionId = null,
			IDeleteEntityService deleteEntityService = null, 
			ICurrentPermissionService currentPermissionService = null
			) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
			JournalFilter = Filter = autofacScope
				.Resolve<DepartmentFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));
			if(parentSubdivisionId.HasValue)
				Filter.Subdivision = UoW.GetById<Subdivision>(parentSubdivisionId.Value);
		}

		protected override IQueryOver<Department> ItemsQuery(IUnitOfWork uow)
		{
			DepartmentJournalNode resultAlias = null;

			Department departmentAlias = null;
			Subdivision subdivisionAlias = null;
			

			var query = uow.Session.QueryOver(() => departmentAlias)
				.Left.JoinAlias(x => x.Subdivision, () => subdivisionAlias);
			if (Filter.Subdivision != null)
				query.Where(() => subdivisionAlias.Id == Filter.Subdivision.Id);
			
			query.Where(GetSearchCriterion(
					() => departmentAlias.Name,
					() => subdivisionAlias.Name
					))
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(x => x.Comments).WithAlias(() => resultAlias.Comments)
					.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.Subdivision)
				).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<DepartmentJournalNode>());
			return query;
		}
	}

	public class DepartmentJournalNode
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Subdivision { get; set; }
		public string Comments { get; set; }
	}
}
