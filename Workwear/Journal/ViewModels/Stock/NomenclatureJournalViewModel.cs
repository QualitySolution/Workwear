using Gamma.ColumnConfig;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using workwear.Domain.Stock;
using workwear.ViewModels.Stock;

namespace workwear.Journal.ViewModels.Stock
{
	public class NomenclatureJournalViewModel : EntityJournalViewModelBase<Nomenclature, NomenclatureViewModel, NomenclatureJournalNode>
	{
		public NomenclatureJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, IDeleteEntityService deleteEntityService = null, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
		}

		protected override IQueryOver<Nomenclature> ItemsQuery(IUnitOfWork uow)
		{
			NomenclatureJournalNode resultAlias = null;
			ItemsType itemsTypeAlias = null;
			Nomenclature nomenclatureAlias = null;

			return uow.Session.QueryOver<Nomenclature>(() => nomenclatureAlias)
				.Left.JoinAlias(n => n.Type, () => itemsTypeAlias)
				.Where(GetSearchCriterion(
					() => nomenclatureAlias.Id,
					() => nomenclatureAlias.Name,
					() => itemsTypeAlias.Name
					))
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(() => itemsTypeAlias.Name).WithAlias(() => resultAlias.ItemType)
				).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<NomenclatureJournalNode>());
		}
	}

	public class NomenclatureJournalNode
	{
		public int Id { get; set; }
		[SearchHighlight]
		public string Name { get; set; }
		[SearchHighlight]
		public string ItemType { get; set; }
	}
}
