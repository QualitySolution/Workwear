﻿using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Workwear.Domain.Regulations;
using Workwear.ViewModels.Regulations;

namespace workwear.Journal.ViewModels.Regulations {
	public class DutyNormsJournalViewModel : EntityJournalViewModelBase<DutyNorm, DutyNormViewModel, DutyNormsJournalNode>{
		public DutyNormsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigationManager, 
			IDeleteEntityService deleteEntityService = null, 
			ICurrentPermissionService currentPermissionService = null) 
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
		}

		protected override IQueryOver<DutyNorm> ItemsQuery(IUnitOfWork uow) {
			{
				DutyNormsJournalNode resultAlias = null;
				DutyNorm dutyNormsAlias = null;
				
				return uow.Session.QueryOver<DutyNorm>(() => dutyNormsAlias)
					.Where(GetSearchCriterion<DutyNorm>(
						x => x.Id, 
						x => x.Name,
						x => x.Comment
					))
					.SelectList((list) => list
						.Select(x => x.Id).WithAlias(() => resultAlias.Id)
						.Select(x => x.Name).WithAlias(() => resultAlias.Name)
						.Select(x => x.Comment).WithAlias(() => resultAlias.Comment)
					).OrderBy(x => x.Name).Asc
					.TransformUsing(Transformers.AliasToBean<DutyNormsJournalNode>());
			}
		}
	}
	
	public class DutyNormsJournalNode
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Comment { get; set; }
	}
}