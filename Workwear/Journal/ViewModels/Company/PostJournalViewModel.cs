using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.ViewModels.Company;

namespace workwear.Journal.ViewModels.Company
{
	public class PostJournalViewModel : EntityJournalViewModelBase<Post, PostViewModel, PostJournalNode>
	{
		public PostJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, IDeleteEntityService deleteEntityService = null, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
		}

		protected override IQueryOver<Post> ItemsQuery(IUnitOfWork uow)
		{
			PostJournalNode resultAlias = null;

			Post postAlias = null;
			Department departmentAlias = null;
			Subdivision subdivisionAlias = null;
			Profession professionAlias = null;
			 
			return uow.Session.QueryOver<Post>(() => postAlias)
				.Left.JoinAlias(x => x.Subdivision, () => subdivisionAlias)
				.Left.JoinAlias(x => x.Profession, () => professionAlias)
				.Left.JoinAlias(x => x.Department, () => departmentAlias)
				.Where(GetSearchCriterion(
					() => postAlias.Name,
					() => departmentAlias.Name,
					() => subdivisionAlias.Name,
					() => professionAlias.Name
					))
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(() => professionAlias.Name).WithAlias(() => resultAlias.Profession)
					.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.Subdivision)
					.Select(() => departmentAlias.Name).WithAlias(() => resultAlias.Department)
				)
				.OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<PostJournalNode>());
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
