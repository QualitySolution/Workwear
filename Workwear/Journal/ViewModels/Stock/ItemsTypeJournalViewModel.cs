using Gamma.Utilities;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using workwear.Domain.Stock;
using workwear.Measurements;
using workwear.ViewModels.Stock;

namespace workwear.Journal.ViewModels.Stock
{
	public class ItemsTypeJournalViewModel : EntityJournalViewModelBase<ItemsType, ItemTypeViewModel, ItemsTypeJournalNode>
	{
		public ItemsTypeJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, IDeleteEntityService deleteEntityService = null, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
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
					.Select(x => x.WearCategory).WithAlias(() => resultAlias.WearCategory)
				).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<ItemsTypeJournalNode>());
		}
	}

	public class ItemsTypeJournalNode
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public СlothesType? WearCategory { get; set; }

		public string WearCategoryText => WearCategory?.GetEnumTitle();
	}
}
