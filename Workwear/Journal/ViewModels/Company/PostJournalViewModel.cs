using System;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.Linq;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using workwear.Journal.Filter.ViewModels.Company;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Tools.Features;
using Workwear.ViewModels.Company;

namespace workwear.Journal.ViewModels.Company {
	public class PostJournalViewModel : EntityJournalViewModelBase<Post, PostViewModel, PostJournalNode> 
	{
		private PostFilterViewModel Filter;
		public PostJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigationManager,
			ILifetimeScope autofacScope,
			FeaturesService featuresService,
			IDeleteEntityService deleteEntityService = null, 
			ICurrentPermissionService currentPermissionService = null
			) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
			JournalFilter = Filter = autofacScope
				.Resolve<PostFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));

			if(featuresService.Available(WorkwearFeature.CostCenter)) {
				//Обход проблемы с тем что SelectionMode одновременно управляет и выбором в журнале, и самим режимом журнала.
				//То есть создает действие выбора. Удалить после того как появится рефакторинг действий журнала. 
				SelectionMode = JournalSelectionMode.Multiple;
				NodeActionsList.Clear();
				CreateNodeActions();
				RowActivatedAction = NodeActionsList.First(x => x.Title == "Изменить");

				var setCostCenterAction = new JournalAction("Изменить МВЗ",
					(selected) => selected.Any(),
					(selected) => true
				);
				NodeActionsList.Add(setCostCenterAction);

				var listCostCenters = UoW.GetAll<CostCenter>().OrderBy(x => x.Code).ThenBy(x => x.Name).ToList();
				listCostCenters.Insert(0, null);
				foreach(CostCenter costCenter in listCostCenters) {
					var updateCostCenterAction = new JournalAction(costCenter?.Title ?? "Очистить",
						(selected) => selected.Any(),
						(selected) => true,
						(selected) => SetCostCenter(selected.Cast<PostJournalNode>().ToArray(), costCenter)
					);
					setCostCenterAction.ChildActionsList.Add(updateCostCenterAction);
				}
			}

			FeaturesService = featuresService;
		}

		public FeaturesService FeaturesService { get; }

		protected override IQueryOver<Post> ItemsQuery(IUnitOfWork uow)
		{
			PostJournalNode resultAlias = null;

			Post postAlias = null;
			Department departmentAlias = null;
			Subdivision subdivisionAlias = null;
			Profession professionAlias = null;
			CostCenter costCenterAlias = null;
			EmployeeCard employeeAlias = null;
			 
			var query = uow.Session.QueryOver<Post>(() => postAlias)
				.Left.JoinAlias(x => x.Subdivision, () => subdivisionAlias)
				.Left.JoinAlias(x => x.Profession, () => professionAlias)
				.Left.JoinAlias(x => x.Department, () => departmentAlias)
				.Left.JoinAlias(x => x.CostCenter, () => costCenterAlias)
				.JoinEntityAlias(() => employeeAlias, () => employeeAlias.Post.Id == postAlias.Id, JoinType.LeftOuterJoin)
				.Where(GetSearchCriterion(
					() => postAlias.Name,
					() => postAlias.Comments,
					() => departmentAlias.Name,
					() => subdivisionAlias.Name,
					() => professionAlias.Name,
					() => costCenterAlias.Code,
					() => costCenterAlias.Name
					));

			if(Filter.Subdivision != null)
				query.Where(() => subdivisionAlias.Id == Filter.Subdivision.Id);
			
			if(Filter.Department != null)
				query.Where(() => departmentAlias.Id == Filter.Department.Id);

			query.SelectList((list) => list
					.SelectGroup(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(x => x.Comments).WithAlias(() => resultAlias.Comments)
					.Select(() => professionAlias.Name).WithAlias(() => resultAlias.Profession)
					.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.Subdivision)
					.Select(() => departmentAlias.Name).WithAlias(() => resultAlias.Department)
					.Select(() => costCenterAlias.Code).WithAlias(() => resultAlias.CostCenterCode)
					.Select(() => costCenterAlias.Name).WithAlias(() => resultAlias.CostCenterName)
					.SelectCount(() => employeeAlias.Id).WithAlias(() => resultAlias.Employees)
				)
				.OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<PostJournalNode>());

			return query;
		}

		private void SetCostCenter(PostJournalNode[] nodes, CostCenter costCenter) {
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var ids = nodes.Select(n => n.Id).ToArray();
				uow.GetAll<Post>()
					.Where(post => ids.Contains(post.Id))
					.UpdateBuilder()
					.Set(bug => bug.CostCenter, costCenter)
					.Update();
			}
			Refresh();
		}
	}

	public class PostJournalNode
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int Employees { get; set; }
		public string Subdivision { get; set; }
		public string Department { get; set; }
		public string Profession { get; set; }
		public string CostCenterCode { get; set; }
		public string CostCenterName { get; set; }
		public string Comments { get; set; }

		public string CostCenterText => String.IsNullOrEmpty(CostCenterCode) ? CostCenterName : $"{CostCenterCode} {CostCenterName}";
	}
}
