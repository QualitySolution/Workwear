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
using Workwear.Domain.Stock;
using Workwear.Tools.Features;
using Workwear.ViewModels.Stock;

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
			SizeType sizeTypeAlias = null;
			SizeType heightTypeAlias = null;
			return uow.Session.QueryOver<ItemsType>()
				.Where(GetSearchCriterion<ItemsType>(
					x => x.Id,
					x => x.Name
					))
				.Left.JoinAlias(x => x.SizeType, () => sizeTypeAlias)
				.Left.JoinAlias(x => x.HeightType, () => heightTypeAlias)
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(x => x.IssueType).WithAlias(() => resultAlias.IssueType)
					.Select(() => sizeTypeAlias.Name).WithAlias(() => resultAlias.TypeOfSize)
					.Select(() => heightTypeAlias.Name).WithAlias(() => resultAlias.TypeOfHeight)
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
		public string TypeOfSize { get; set; }
		public string TypeOfHeight { get; set; }
	}
}
