﻿using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Workwear.Domain.Stock;
using Workwear.ViewModels.Stock;

namespace workwear.Journal.ViewModels.Stock
{
	public class WarehouseJournalViewModel: EntityJournalViewModelBase<Warehouse, WarehouseViewModel, WarehouseJournalNode>
	{
		public WarehouseJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, IDeleteEntityService deleteEntityService, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
		}

		protected override IQueryOver<Warehouse> ItemsQuery(IUnitOfWork uow)
		{
			WarehouseJournalNode resultAlias = null;
			Warehouse WareHouseAlias = null;

			return uow.Session.QueryOver<Warehouse>(() => WareHouseAlias)
				.Where(GetSearchCriterion(
					() => WareHouseAlias.Id,
					() => WareHouseAlias.Name
					))
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
				).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<WarehouseJournalNode>());
		}
	}

	public class WarehouseJournalNode
	{
		public int Id { get; set; }
		public string Name { get; set; }

	}
}



