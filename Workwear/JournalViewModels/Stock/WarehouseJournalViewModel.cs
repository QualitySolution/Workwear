using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.JournalViewModels.Company;
using workwear.ViewModels.Stock;

namespace workwear.JournalViewModels.Stock
{
	public class WarehouseJournalViewModel: EntityJournalViewModelBase<WareHouse, WarehouseViewModel, WarehouseJournalNode>
	{
		public WarehouseJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, IDeleteEntityService deleteEntityService, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
		}

		protected override IQueryOver<WareHouse> ItemsQuery(IUnitOfWork uow)
		{
			WarehouseJournalNode resultAlias = null;
			Subdivision subdivisionAlias = null;
			WareHouse WareHouseAlias = null;

			return uow.Session.QueryOver<WareHouse>(() => WareHouseAlias)
				.Where(GetSearchCriterion(
					() => WareHouseAlias.Id,
					() => WareHouseAlias.Name,
					() => subdivisionAlias.Name
					))
					.JoinAlias(x => x.Subdivision, () => subdivisionAlias)
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.Subdivision)
				).TransformUsing(Transformers.AliasToBean<WarehouseJournalNode>());
		}
	}

	public class WarehouseJournalNode
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Subdivision { get; set; }

	}
}



