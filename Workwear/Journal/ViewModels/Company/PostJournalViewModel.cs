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
using Workwear.Domain.Regulations;
using workwear.Journal.Filter.ViewModels.Company;
using workwear.ViewModels.Company;

namespace workwear.Journal.ViewModels.Company
{
	public class PostJournalViewModel : EntityJournalViewModelBase<Post, PostViewModel, PostJournalNode> 
	{
		private PostFilterViewModel Filter;
		public PostJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigationManager,
			ILifetimeScope autofacScope,
			IDeleteEntityService deleteEntityService = null, 
			ICurrentPermissionService currentPermissionService = null
			) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
			AutofacScope = autofacScope;
			JournalFilter = Filter = AutofacScope
				.Resolve<PostFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));
		}

		protected override IQueryOver<Post> ItemsQuery(IUnitOfWork uow)
		{
			PostJournalNode resultAlias = null;

			Post postAlias = null;
			Department departmentAlias = null;
			Subdivision subdivisionAlias = null;
			Profession professionAlias = null;
			 
			var query = uow.Session.QueryOver<Post>(() => postAlias)
				.Left.JoinAlias(x => x.Subdivision, () => subdivisionAlias)
				.Left.JoinAlias(x => x.Profession, () => professionAlias)
				.Left.JoinAlias(x => x.Department, () => departmentAlias)
				.Where(GetSearchCriterion(
					() => postAlias.Name,
					() => departmentAlias.Name,
					() => subdivisionAlias.Name,
					() => professionAlias.Name
					));

			if(Filter.Subdivision != null)
				query.Where(() => subdivisionAlias.Id == Filter.Subdivision.Id);

			query.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(() => professionAlias.Name).WithAlias(() => resultAlias.Profession)
					.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.Subdivision)
					.Select(() => departmentAlias.Name).WithAlias(() => resultAlias.Department)
				)
				.OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<PostJournalNode>());

			return query;
		}
	}

	public class PostJournalNode
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Subdivision { get; set; }
		public string Department { get; set; }
		public string Profession { get; set; }
	}
}
