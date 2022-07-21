using System;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.ViewModels.Regulations;

namespace workwear.Journal.ViewModels.Regulations
{
	public class ProtectionToolsJournalViewModel : EntityJournalViewModelBase<ProtectionTools, ProtectionToolsViewModel, ProtectionToolsJournalNode>
	{
		private readonly ItemsType type;
		public ProtectionToolsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigationManager, 
			IDeleteEntityService deleteEntityService = null, 
			ICurrentPermissionService currentPermissionService = null,
			ItemsType type = null
			) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
			this.type = type;
		}

		protected override IQueryOver<ProtectionTools> ItemsQuery(IUnitOfWork uow)
		{
			ProtectionToolsJournalNode resultAlias = null;
			ItemsType itemsTypeAlias = null;
			var query = uow.Session.QueryOver<ProtectionTools>()
				.Left.JoinAlias(x => x.Type, () => itemsTypeAlias);

			if(type != null)
				query = query.Where(p => itemsTypeAlias.Id == type.Id);
			
			return query
				.Where(GetSearchCriterion<ProtectionTools>(
					x => x.Id,
					x => x.Name
					))
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(() => itemsTypeAlias.Name).WithAlias(() => resultAlias.TypeName)
				).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<ProtectionToolsJournalNode>());
		}
	}

	public class ProtectionToolsJournalNode
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string TypeName { get; set; }
	}
}
