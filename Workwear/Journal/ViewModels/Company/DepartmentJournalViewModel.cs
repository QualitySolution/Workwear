using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using workwear.Domain.Company;
using workwear.ViewModels.Company;

namespace workwear.Journal.ViewModels.Company
{

	public class DepartmentJournalViewModel : EntityJournalViewModelBase<Department, DepartmentViewModel, DepartmentJournalNode>
	{
		public DepartmentJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, IDeleteEntityService deleteEntityService = null, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
		}

		protected override IQueryOver<Department> ItemsQuery(IUnitOfWork uow)
		{
			DepartmentJournalNode resultAlias = null;

			Department departmentAlias = null;
			Subdivision subdivisionAlias = null;

			return uow.Session.QueryOver<Department>(() => departmentAlias)
				.Left.JoinAlias(x => x.Subdivision, () => subdivisionAlias)
				.Where(GetSearchCriterion(
					() => departmentAlias.Name,
					() => subdivisionAlias.Name
					))
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(x => x.Comments).WithAlias(() => resultAlias.Comments)
					.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.Subdivision)
				).TransformUsing(Transformers.AliasToBean<DepartmentJournalNode>());
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
