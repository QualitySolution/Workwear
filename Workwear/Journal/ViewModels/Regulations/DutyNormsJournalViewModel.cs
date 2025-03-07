﻿using System;
using System.Globalization;
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
			UseSlider = true;
		}

		protected override IQueryOver<DutyNorm> ItemsQuery(IUnitOfWork uow) {
			{
				DutyNormsJournalNode resultAlias = null;
				DutyNorm dutyNormsAlias = null;
				Subdivision subdivisionAlias = null;
				
				return uow.Session.QueryOver<DutyNorm>(() => dutyNormsAlias)
					.Where(GetSearchCriterion<DutyNorm>(
						x => x.Id, 
						x => x.Name,
						x => x.Subdivision,
						x => x.Comment
					))
					.JoinAlias(() => dutyNormsAlias.Subdivision, () => subdivisionAlias, JoinType.LeftOuterJoin)
                    .SelectList((list) => list
						.Select(x => x.Id).WithAlias(() => resultAlias.Id)
						.Select(x => x.Name).WithAlias(() => resultAlias.Name)
						.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.SubdivisionName)
						.Select(x => x.DateFrom).WithAlias(() => resultAlias.DateFrom)
						.Select(x => x.DateTo).WithAlias(() => resultAlias.DateTo)
						.Select(x => x.Comment).WithAlias(() => resultAlias.Comment)
					).OrderBy(x => x.Name).Asc
					.TransformUsing(Transformers.AliasToBean<DutyNormsJournalNode>());
			}
		}
	}
	
	public class DutyNormsJournalNode
	{
		static CultureInfo _culture = CultureInfo.GetCultureInfo("ru-RU");
		public int Id { get; set; }
		public string Name { get; set; }
		public string Comment { get; set; }
		public string SubdivisionName { get; set; } 
		public DateTime? DateFrom { get; set; }
		public DateTime? DateTo { get; set; }
		public string DateFromString => DateFrom == null ? String.Empty : DateFrom.Value.ToString("d", _culture); 
		public string DateToString => DateTo == null ? String.Empty : DateTo.Value.ToString("d", _culture); 
	}
}
