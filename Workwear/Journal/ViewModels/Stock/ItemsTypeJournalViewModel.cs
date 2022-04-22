using Gamma.Utilities;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Workwear.Domain.Sizes;
using workwear.Domain.Stock;
using workwear.Tools.Features;
using workwear.ViewModels.Stock;
using Workwear.Measurements;

namespace workwear.Journal.ViewModels.Stock
{
	public class ItemsTypeJournalViewModel : EntityJournalViewModelBase<ItemsType, ItemTypeViewModel, ItemsTypeJournalNode>
	{
		public FeaturesService FeaturesService { get; }

		public ItemsTypeJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, FeaturesService featuresService, IDeleteEntityService deleteEntityService = null, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
			FeaturesService = featuresService ?? throw new System.ArgumentNullException(nameof(featuresService));
		}

		protected override IQueryOver<ItemsType> ItemsQuery(IUnitOfWork uow)
		{
			ItemsTypeJournalNode resultAlias = null;
			return uow.Session.QueryOver<ItemsType>()
				.Where(GetSearchCriterion<ItemsType>(
					x => x.Id,
					x => x.Name
					))
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(x => x.IssueType).WithAlias(() => resultAlias.IssueType)
				).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<ItemsTypeJournalNode>());
		}
	}

	public class ItemsTypeJournalNode
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public IssueType IssueType { get; set; }
		public string IssueTypeText => IssueType.GetEnumTitle();
	}
}
