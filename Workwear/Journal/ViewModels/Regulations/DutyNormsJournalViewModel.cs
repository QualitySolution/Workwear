using System;
using System.Globalization;
using NHibernate;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Tools;
using Workwear.ViewModels.Regulations;

namespace workwear.Journal.ViewModels.Regulations {
	public class DutyNormsJournalViewModel : EntityJournalViewModelBase<DutyNorm, DutyNormViewModel, DutyNormsJournalNode>, IDialogDocumentation{
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("regulations.html#duty-norms");
		public string ButtonTooltip => DocHelper.GetJournalDocTooltip(typeof(DutyNorm));
		#endregion
		public DutyNormsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigationManager, 
			IDeleteEntityService deleteEntityService = null, 
			ICurrentPermissionService currentPermissionService = null) 
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
			CreatePopupActions();
		}

		protected override IQueryOver<DutyNorm> ItemsQuery(IUnitOfWork uow) {
			{
				DutyNormsJournalNode resultAlias = null;
				DutyNorm dutyNormsAlias = null;
				Subdivision subdivisionAlias = null;
				
				return uow.Session.QueryOver<DutyNorm>(() => dutyNormsAlias)
					.JoinAlias(x => dutyNormsAlias.Subdivision, () => subdivisionAlias, JoinType.LeftOuterJoin)
					.Where(GetSearchCriterion(
						() => dutyNormsAlias.Id,
						() => dutyNormsAlias.Name,
						() => subdivisionAlias.Name,
						() => dutyNormsAlias.Comment
					))
					.SelectList((list) => list
						.Select(x => x.Id).WithAlias(() => resultAlias.Id)
						.Select(x => x.Name).WithAlias(() => resultAlias.Name)
						.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.SubdivisionName)
						.Select(x => x.DateFrom).WithAlias(() => resultAlias.DateFrom)
						.Select(x => x.DateTo).WithAlias(() => resultAlias.DateTo)
						.Select(x => x.Archival).WithAlias(() => resultAlias.Archival)
						.Select(x => x.Comment).WithAlias(() => resultAlias.Comment)
					).OrderBy(x => x.Name).Asc
					.TransformUsing(Transformers.AliasToBean<DutyNormsJournalNode>());
			}
		}
		#region Popupmenu action implementation

		protected override void CreatePopupActions() {
			PopupActionsList.Add(new JournalAction("Создать копию дежурной нормы", (arg)=>arg.Length == 1, (arg)=>true, CopyDutyNorm));
		}
		private void CopyDutyNorm(object[] nodes)
		{
			if(nodes.Length != 1)
				return;
			int dutyNormId = (nodes[0] as DutyNormsJournalNode).Id;
			var page = NavigationManager.OpenViewModel<DutyNormViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
			page.ViewModel.CopyDutyNormFrom(dutyNormId);
		}
		#endregion
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
		public bool Archival { get; set; }
		public string DateFromString => DateFrom == null ? String.Empty : DateFrom.Value.ToString("d", _culture); 
		public string DateToString => DateTo == null ? String.Empty : DateTo.Value.ToString("d", _culture); 
	}
}
