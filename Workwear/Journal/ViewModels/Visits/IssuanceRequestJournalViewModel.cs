using System;
using Autofac;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using NHibernate;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using Workwear.Domain.Visits;
using Workwear.Journal.Filter.ViewModels.Visits;
using Workwear.ViewModels.Visits;

namespace workwear.Journal.ViewModels.Visits {
	public class IssuanceRequestJournalViewModel: EntityJournalViewModelBase<IssuanceRequest, IssuanceRequestViewModel, IssuanceRequestJournalNode> {
		public IssuanceRequestFilterViewModel Filter { get; set; }
		public IssuanceRequestJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ILifetimeScope autofacScope
			): base(unitOfWorkFactory, interactiveService, navigationManager) {

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
				.Where(GetSearchCriterion(
					() => issuanceRequestAlias.Id,
					() => issuanceRequestAlias.ReceiptDate,
					() => issuanceRequestAlias.Comment));
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
					.Select(() => issuanceRequestAlias.CreationDate).WithAlias(() => resultAlias.CreationDate)
				)
				.TransformUsing(Transformers.AliasToBean<IssuanceRequestJournalNode>());

			return query;
		}
	}

	public class IssuanceRequestJournalNode {
		[SearchHighlight]
		public int Id { get; set; }
		[SearchHighlight]
		public DateTime ReceiptDate { get; set; }
		public IssuanceRequestStatus Status { get; set; }
		public string StatusString => Status.GetEnumTitle();
		[SearchHighlight]
		public string Comment { get; set; }
		public string Author { get; set; }
		public DateTime? CreationDate { get; set; }
		public string CreationDateString => CreationDate?.ToString("dd/MM/yyyy") ?? String.Empty;
	}

	
}
