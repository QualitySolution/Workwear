using System;
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
using Workwear.Domain.Company;
using workwear.Journal.Filter.ViewModels.Company;
using workwear.ViewModels.Company;
using QS.Project.DB;

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
			Subdivision parent1SubdivisionAlias = null;
			Subdivision parent2SubdivisionAlias = null;
			Subdivision parent3SubdivisionAlias = null;
			Subdivision parent4SubdivisionAlias = null;
			Subdivision parent5SubdivisionAlias = null;
			Subdivision parent6SubdivisionAlias = null;
			Subdivision subdivisionAlias = null;
			
			var orderProjection = CustomProjections.Concat_WS("",
					() => parent6SubdivisionAlias.Name,
					() => parent5SubdivisionAlias.Name,
					() => parent4SubdivisionAlias.Name,
					() => parent3SubdivisionAlias.Name,
					() => parent2SubdivisionAlias.Name,
					() => parent1SubdivisionAlias.Name,
					() => subdivisionAlias.Name);

			var query = uow.Session.QueryOver(() => subdivisionAlias)
				.Where(GetSearchCriterion<Subdivision>(
					x => x.Code,
					x => x.Name,
					x => x.Address
				))
				.JoinAlias(() => subdivisionAlias.ParentSubdivision, () => parent1SubdivisionAlias,
					JoinType.LeftOuterJoin)
				.JoinAlias(() => parent1SubdivisionAlias.ParentSubdivision, () => parent2SubdivisionAlias,
					JoinType.LeftOuterJoin)
				.JoinAlias(() => parent2SubdivisionAlias.ParentSubdivision, () => parent3SubdivisionAlias,
					JoinType.LeftOuterJoin)
				.JoinAlias(() => parent3SubdivisionAlias.ParentSubdivision, () => parent4SubdivisionAlias,
					JoinType.LeftOuterJoin)
				.JoinAlias(() => parent4SubdivisionAlias.ParentSubdivision, () => parent5SubdivisionAlias,
					JoinType.LeftOuterJoin)
				.JoinAlias(() => parent5SubdivisionAlias.ParentSubdivision, () => parent6SubdivisionAlias,
					JoinType.LeftOuterJoin);
			if (Filter.SortByParent)
				return query.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.Id)
						.Select(x => x.Code).WithAlias(() => resultAlias.Code)
						.Select(x => x.Name).WithAlias(() => resultAlias.Name)
						.Select(x => x.Address).WithAlias(() => resultAlias.Address)
						.Select(() => parent1SubdivisionAlias.Id).WithAlias(() => resultAlias.Parent1Id)
						.Select(() => parent2SubdivisionAlias.Id).WithAlias(() => resultAlias.Parent2Id)
						.Select(() => parent3SubdivisionAlias.Id).WithAlias(() => resultAlias.Parent3Id)
						.Select(() => parent4SubdivisionAlias.Id).WithAlias(() => resultAlias.Parent4Id)
						.Select(() => parent5SubdivisionAlias.Id).WithAlias(() => resultAlias.Parent5Id)
						.Select(() => parent6SubdivisionAlias.Id).WithAlias(() => resultAlias.Parent6Id)
					).OrderBy(orderProjection).Asc
					.TransformUsing(Transformers.AliasToBean<SubdivisionJournalNode>());

			else return query.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Code).WithAlias(() => resultAlias.Code)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(x => x.Address).WithAlias(() => resultAlias.Address)
					.Select(() => parent1SubdivisionAlias.Name).WithAlias(() => resultAlias.ParentName)
					).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<SubdivisionJournalNode>());
		}
	}
	public class SubdivisionJournalNode {
		public int Id { get; set; }
		public string Code { get; set; }
		public string Name { get; set; }
		public string Address { get; set; }
		public string ParentName { get; set; }
		public int? Parent1Id { get; set; }
		public int? Parent2Id { get; set; }
		public int? Parent3Id { get; set; }
		public int? Parent4Id { get; set; }
		public int? Parent5Id { get; set; }
		public int? Parent6Id { get; set; }
		private int SumParent => 
			(Parent1Id.HasValue ? 1 : 0) + (Parent2Id.HasValue ? 1 : 0) + (Parent3Id.HasValue ? 1 : 0) 
			+ (Parent4Id.HasValue ? 1 : 0) + (Parent5Id.HasValue ? 1 : 0) + (Parent6Id.HasValue ? 1 : 0);
		public string IndentedName => 
			new string('	', SumParent) + (Parent1Id.HasValue ? "↑" : String.Empty) + Name;
	}
}
