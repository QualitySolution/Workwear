using System;
using Gamma.ColumnConfig;
using NHibernate;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Journal;
using QS.Project.Services;
using Workwear.Domain.Visits;
using Workwear.ViewModels.Visits;

namespace workwear.Journal.ViewModels.Visits {
	public class IssuanceRequestJournalViewModel: EntityJournalViewModelBase<IssuanceRequest, IssuanceRequestViewModel, IssuanceRequestJournalNode> {
		public IssuanceRequestJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService = null, 
			ICurrentPermissionService currentPermissionService = null
			): base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService) {
			TableSelectionMode = JournalSelectionMode.Multiple;

		}

		protected override IQueryOver<IssuanceRequest> ItemsQuery(IUnitOfWork uow) {
			throw new NotImplementedException();
		}
	}

	public class IssuanceRequestJournalNode {
		[SearchHighlight]
		public int Id { get; set; }
		[SearchHighlight]
		public DateTime ReceiptDate { get; set; }
		public IssuanceRequestStatus Status { get; set; }
		[SearchHighlight]
		public string Comment { get; set; }
		public string Author { get; set; }
		public DateTime? CreationDate { get; set; }
	}

	
}
