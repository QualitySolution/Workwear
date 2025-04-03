﻿using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Tools;
using Workwear.ViewModels.Company;

namespace workwear.Journal.ViewModels.Company
{
	public class LeadersJournalViewModel : EntityJournalViewModelBase<Leader, LeadersViewModel, LeaderJournalNode>, IDialogDocumentation
	{
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("organization.html#leaders");
		public string ButtonTooltip => DocHelper.GetJournalDocTooltip(typeof(Leader));
		#endregion
		public LeadersJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, IDeleteEntityService deleteEntityService, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
		}

		protected override IQueryOver<Leader> ItemsQuery(IUnitOfWork uow)
		{
			LeaderJournalNode resultAlias = null;
			return uow.Session.QueryOver<Leader>()
				.Where(GetSearchCriterion<Leader>(
					x => x.Id,
					x => x.Name,
					x => x.Surname,
					x => x.Patronymic,
					x => x.Position
					))
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(x => x.Surname).WithAlias(() => resultAlias.SurName)
					.Select(x => x.Patronymic).WithAlias(() => resultAlias.Patronymic)
					.Select(x => x.Position).WithAlias(() => resultAlias.Position)
				)
				.OrderBy(x => x.Surname).Asc
				.ThenBy(x => x.Name).Asc
				.ThenBy(x => x.Patronymic).Asc
				.TransformUsing(Transformers.AliasToBean<LeaderJournalNode>());
		}
	}

	public class LeaderJournalNode
	{
		public int Id { get; set; }
		public string SurName { get; set; }
		public string Name{ get; set; }
		public string Patronymic { get; set; }
		public string Position { get; set; }
	}
}
