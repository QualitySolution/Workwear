using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using workwear.Domain.Regulations;
using workwear.ViewModels.Regulations;

namespace workwear.Journal.ViewModels.Regulations
{
	public class ProtectionToolsJournalViewModel : EntityJournalViewModelBase<ProtectionTools, ProtectionToolsViewModel, ProtectionToolsJournalNode>
	{
		public ProtectionToolsJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, IDeleteEntityService deleteEntityService = null, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
		}

		protected override IQueryOver<ProtectionTools> ItemsQuery(IUnitOfWork uow)
		{
			ProtectionToolsJournalNode resultAlias = null;
			return uow.Session.QueryOver<ProtectionTools>()
				.Where(GetSearchCriterion<ProtectionTools>(
					x => x.Id,
					x => x.Name
					))
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
				).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<ProtectionToolsJournalNode>());
		}
	}

	public class ProtectionToolsJournalNode
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
}
