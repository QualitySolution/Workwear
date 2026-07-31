using System;
using System.Globalization;
using System.Linq;
using Autofac;
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
using Workwear.Journal.Filter.ViewModels.Regulations;
using Workwear.Models.Operations;
using Workwear.Repository.Regulations;
using Workwear.Tools;
using Workwear.ViewModels.Regulations;

namespace workwear.Journal.ViewModels.Regulations {
	public class DutyNormsJournalViewModel : EntityJournalViewModelBase<DutyNorm, DutyNormViewModel, DutyNormsJournalNode>, IDialogDocumentation{
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("regulations.html#duty-norms");
		public string ButtonTooltip => DocHelper.GetJournalDocTooltip(typeof(DutyNorm));
		#endregion

		private ILifetimeScope autofacScope;
		private readonly DutyNormIssueModel dutyNormIssueModel;
		private readonly DutyNormRepository dutyNormRepository;
		private readonly ModalProgressCreator progressCreator;
		public DutyNormFilterViewModel Filter { get; set; }
		public DutyNormsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ILifetimeScope autofacScope,
			UnitOfWorkProvider unitOfWorkProvider,
			DutyNormIssueModel dutyNormIssueModel,
			DutyNormRepository dutyNormRepository,
			ModalProgressCreator progressCreator,
			IDeleteEntityService deleteEntityService = null,
			ICurrentPermissionService currentPermissionService = null)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService) {
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.dutyNormIssueModel = dutyNormIssueModel ?? throw new ArgumentNullException(nameof(dutyNormIssueModel));
			this.dutyNormRepository = dutyNormRepository ?? throw new ArgumentNullException(nameof(dutyNormRepository));
			this.progressCreator = progressCreator ?? throw new ArgumentNullException(nameof(progressCreator));
			if(unitOfWorkProvider == null) throw new ArgumentNullException(nameof(unitOfWorkProvider));
			unitOfWorkProvider.UoW = UoW;
			JournalFilter = Filter = autofacScope.Resolve<DutyNormFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));
			CreatePopupActions();
			TableSelectionMode = JournalSelectionMode.Multiple;
			CreateNodeActions();
		}

		protected override IQueryOver<DutyNorm> ItemsQuery(IUnitOfWork uow) {
			{
				DutyNormsJournalNode resultAlias = null;
				DutyNorm dutyNormsAlias = null;
				Subdivision subdivisionAlias = null;

				var query = uow.Session.QueryOver<DutyNorm>(() => dutyNormsAlias);
				if(!Filter.ShowArchival)
					query.Where(x => !x.Archival);
				return query
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

		#region Toolbar action implementation
		public void CreateNodeActions() {
			NodeActionsList.Add(new JournalAction("Пересчитать даты",
				(selected) => selected.Any(),
				(selected) => true,
				RecalculateNextIssue));
		}

		private void RecalculateNextIssue(object[] nodes) {
			var ids = nodes.Cast<DutyNormsJournalNode>().Select(x => x.Id).ToArray();

			progressCreator.Start(3, text: "Загружаем дежурные нормы");
			dutyNormRepository.LoadFullInfo(ids, UoW);
			var items = UoW.GetById<DutyNorm>(ids).SelectMany(x => x.Items).ToArray();

			progressCreator.Add(text: "Пересчитываем даты следующей выдачи");
			dutyNormIssueModel.FillDutyNormItems(items);
			foreach(var item in items)
				item.UpdateNextIssue();

			progressCreator.Add(text: "Сохраняем изменения");
			UoW.Commit();
			progressCreator.Close();
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
