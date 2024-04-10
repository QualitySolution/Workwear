using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Workwear.Domain.Analytics;
using Workwear.ViewModels.Analytics;

namespace Workwear.Journal.Analytics 
{
	public class ProtectionToolsCategoryJournalViewModel : EntityJournalViewModelBase<ProtectionToolsCategory, ProtectionToolsCategoryViewModel, ProtectionToolsCategoryNode> 
	{
		public ProtectionToolsCategoryJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService = null,
			ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService) 
		{
			UseSlider = true;
		}

		protected override IQueryOver<ProtectionToolsCategory> ItemsQuery(IUnitOfWork uow) 
		{
			ProtectionToolsCategoryNode resultAlias = null;
			return uow.Session.QueryOver<ProtectionToolsCategory>()
				.Where(GetSearchCriterion<ProtectionToolsCategory>(
					x => x.Id,
					x => x.Name
				))
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(x => x.Comment).WithAlias(() => resultAlias.Comment))
				.OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<ProtectionToolsCategoryNode>());
		}
	}

	public class ProtectionToolsCategoryNode 
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Comment { get; set; }
	}
}
