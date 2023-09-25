using System.Linq;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.ViewModels.Regulations;

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
			
			TableSelectionMode = JournalSelectionMode.Multiple;
			CreateActions();
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
		
		#region Actions
		private void CreateActions() {
			base.CreateNodeActions();
			var changeTypeAction = new JournalAction("Изменить тип",
				(selected) => selected.Any(),
				(selected) => SelectionMode == JournalSelectionMode.None
			);
			NodeActionsList.Add(changeTypeAction);
			
			var itemTypes = UoW.GetAll<ItemsType>().OrderBy(x => x.Name).ToList();
			foreach(ItemsType itemsType in itemTypes) {
				var updateTypeAction = new JournalAction(itemsType?.Name,
					(selected) => selected.Any(),
					(selected) => true,
					(selected) => SetType(selected.Cast<ProtectionToolsJournalNode>().ToArray(), itemsType)
				);
				changeTypeAction.ChildActionsList.Add(updateTypeAction);
			}
		}
		
		private void SetType(ProtectionToolsJournalNode[] nodes, ItemsType itemsType) {
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var ids = nodes.Select(n => n.Id).ToArray();
				uow.GetAll<ProtectionTools>()
					.Where(p => ids.Contains(p.Id))
					.UpdateBuilder()
					.Set(n => n.Type, itemsType)
					.Update();
			}
			Refresh();
		}
		#endregion
	}

	public class ProtectionToolsJournalNode
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string TypeName { get; set; }
	}
}
