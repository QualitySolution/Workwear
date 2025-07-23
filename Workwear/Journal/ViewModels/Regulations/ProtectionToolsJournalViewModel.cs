using System;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Journal;
using QS.Project.Services;
using QS.ViewModels.Extension;
using Workwear.Domain.Analytics;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Journal.Filter.ViewModels.Regulations;
using Workwear.Tools;
using Workwear.ViewModels.Regulations;

namespace workwear.Journal.ViewModels.Regulations
{
	public class ProtectionToolsJournalViewModel : EntityJournalViewModelBase<ProtectionTools, ProtectionToolsViewModel, ProtectionToolsJournalNode>, IDialogDocumentation
	{
		private readonly ItemsType type;
		public ProtectionToolsFilterViewModel Filter { get; private set; }
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("regulations.html#protection-tools");
		public string ButtonTooltip => DocHelper.GetJournalDocTooltip(typeof(ProtectionTools));
		#endregion
		public ProtectionToolsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigationManager, 
			ILifetimeScope autofacScope,
			IDeleteEntityService deleteEntityService = null, 
			ICurrentPermissionService currentPermissionService = null,
			ItemsType type = null
			) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			JournalFilter = Filter = autofacScope.Resolve<ProtectionToolsFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));
			
			UseSlider = true;
			this.type = type;
			
			TableSelectionMode = JournalSelectionMode.Multiple;
			CreateActions();
		}

		protected override IQueryOver<ProtectionTools> ItemsQuery(IUnitOfWork uow)
		{
			ProtectionToolsJournalNode resultAlias = null;
			ItemsType itemsTypeAlias = null;
			ProtectionToolsCategory categoryForAnalyticAlias = null;
			var query = uow.Session.QueryOver<ProtectionTools>()
				.Left.JoinAlias(x => x.Type, () => itemsTypeAlias)
				.Left.JoinAlias(x => x.CategoryForAnalytic, () => categoryForAnalyticAlias);
			
			if (Filter.OnlyDermal)
				query.Where(x => x.DermalPpe == true);			
			if (Filter.NotDispenser)
				query.Where(x => x.Dispenser == false);
			if(!Filter.ShowArchival)
				query.Where(x => x.Archival == false);
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
					.Select(x => x.DermalPpe).WithAlias(() => resultAlias.WashingPpe)
					.Select(x => x.Dispenser).WithAlias(() => resultAlias.Dispenser)
					.Select(x=>x.Archival).WithAlias(()=>resultAlias.Archival)
					.Select(() => itemsTypeAlias.Name).WithAlias(() => resultAlias.TypeName)
					.Select(() => categoryForAnalyticAlias.Name).WithAlias(() => resultAlias.CategoryForAnalytic)
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
		public string CategoryForAnalytic { get; set; }
		public bool WashingPpe { get; set; }
		public bool Dispenser { get; set; }
		public bool Archival { get; set; }
		public string WashingText => Dispenser ? "Дозатор" : WashingPpe ? "Да" : String.Empty;
	}
}
