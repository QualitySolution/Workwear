using System;
using Autofac;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using Workwear.Domain.Visits;
using Workwear.Journal.Filter.ViewModels.Visits;
using Workwear.Tools.Features;
using Workwear.ViewModels.Visits;

namespace workwear.Journal.ViewModels.Visits {
	public class IssuanceRequestJournalViewModel: EntityJournalViewModelBase<IssuanceRequest, IssuanceRequestViewModel, IssuanceRequestJournalNode> {
		public FeaturesService FeaturesService { get; }
		public IssuanceRequestFilterViewModel Filter { get; set; }
		public IssuanceRequestJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ILifetimeScope autofacScope, 
			FeaturesService featuresService,
			IDeleteEntityService deleteEntityService = null,
			ICurrentPermissionService currentPermissionService = null
			): base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService) {
			FeaturesService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			
			Title = "Журнал заявок на выдачу";
			JournalFilter = Filter = autofacScope.Resolve<IssuanceRequestFilterViewModel>(
				new TypedParameter(typeof(JournalViewModelBase), this));
			TableSelectionMode = JournalSelectionMode.Multiple;
		}

		protected override IQueryOver<IssuanceRequest> ItemsQuery(IUnitOfWork uow) {
			IssuanceRequestJournalNode resultAlias = null;
			IssuanceRequest issuanceRequestAlias = null;
			UserBase authorAlias = null;
			
			var query = uow.Session.QueryOver(() => issuanceRequestAlias)
				.Where(
					MakeSearchCriterion()
						.WithLikeMode(MatchMode.Exact)
						.By(() => issuanceRequestAlias.Id)
						.WithLikeMode(MatchMode.Start)
						.By(() => issuanceRequestAlias.ReceiptDate)
						.WithLikeMode(MatchMode.Anywhere)
						.By(() => issuanceRequestAlias.Comment)
						.Finish()
					);
			if(Filter.Status != null)
				query.Where(x => x.Status == Filter.Status);

			query
				.JoinAlias(() => issuanceRequestAlias.CreatedByUser, () => authorAlias, JoinType.LeftOuterJoin)
				.OrderBy(() => issuanceRequestAlias.ReceiptDate).Desc
				.SelectList(list => list
					.Select(() => issuanceRequestAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => issuanceRequestAlias.ReceiptDate).WithAlias(() => resultAlias.ReceiptDate)
					.Select(() => issuanceRequestAlias.Status).WithAlias(() => resultAlias.Status)
					.Select(() => issuanceRequestAlias.Comment).WithAlias(() => resultAlias.Comment)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.Author)
					.Select(() => issuanceRequestAlias.ReceiptDate).WithAlias(() => resultAlias.ReceiptDate)
				)
				.TransformUsing(Transformers.AliasToBean<IssuanceRequestJournalNode>());

			return query;
		}
		
		public static string ColorsLegendText = 
			"<span color='black'>●</span> — новая\n" +
			"<span color='orange'>●</span> — закрыта частично\n" +
			"<span color='gray'>●</span> — закрыта\n";
	}

	public class IssuanceRequestJournalNode {
		[SearchHighlight] public int Id { get; set; }
		[SearchHighlight] public DateTime ReceiptDate { get; set; }
		public IssuanceRequestStatus Status { get; set; }
		public string StatusString => Status.GetEnumTitle();
		[SearchHighlight] public string Comment { get; set; }
		public string Author { get; set; }
		public DateTime? CreationDate { get; set; }
		public string CreationDateString => CreationDate?.ToString("dd/MM/yyyy") ?? String.Empty;

		public string RowColor {
			get {
				//Не забываем править ColorsLegendText
				switch(Status) {
					case IssuanceRequestStatus.New: return "black";
					case IssuanceRequestStatus.PartiallyIssued: return "orange";
					case IssuanceRequestStatus.Issued: return "gray";
					default: return "black";
				}
			}
		}
	}

}
