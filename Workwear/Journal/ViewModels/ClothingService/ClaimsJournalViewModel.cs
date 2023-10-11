using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using workwear.ViewModels.ClothingService;
using Workwear.ViewModels.ClothingService;

namespace workwear.Journal.ViewModels.ClothingService {
	public class ClaimsJournalViewModel : EntityJournalViewModelBase<ServiceClaim, ServiceClaimViewModel, ClaimsJournalNode> {
		private IInteractiveService interactive; 
		public ClaimsJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, IDeleteEntityService deleteEntityService = null, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			interactive = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			Title = "Обслуживание одежды";
			
			CreateActions();
		}

		protected override IQueryOver<ServiceClaim> ItemsQuery(IUnitOfWork uow) {
			ClaimsJournalNode resultAlias = null;
			ServiceClaim serviceClaimAlias = null;
			StateOperation stateOperationAlias = null;
			Barcode barcodeAlias = null;
			Nomenclature nomenclatureAlias = null;
			
			BarcodeOperation barcodeOperationAlias = null;
			EmployeeIssueOperation employeeIssueOperationAlias = null;
			EmployeeCard employeeAlias = null;

			var subqueryLastEmployee = QueryOver.Of<BarcodeOperation>(() => barcodeOperationAlias)
				.Where(() => barcodeOperationAlias.Barcode.Id == barcodeAlias.Id)
				.JoinAlias(() => barcodeOperationAlias.EmployeeIssueOperation, () => employeeIssueOperationAlias)
				.JoinAlias(() => employeeIssueOperationAlias.Employee, () => employeeAlias)
				.OrderBy(() => employeeIssueOperationAlias.OperationTime).Desc
				.Select(CustomProjections.Concat(
					Projections.Property(() => employeeAlias.LastName),
					Projections.Constant(" "),
					Projections.Property(() => employeeAlias.FirstName),
					Projections.Constant(" "),
					Projections.Property(() => employeeAlias.Patronymic)));
			
			var query = uow.Session.QueryOver<ServiceClaim>(() => serviceClaimAlias);

			return query
				.Left.JoinAlias(x => x.States, () => stateOperationAlias)
				.Left.JoinAlias(x => x.Barcode, () => barcodeAlias)
				.Left.JoinAlias(() => barcodeAlias.Nomenclature, () => nomenclatureAlias)
				.OrderBy(() => stateOperationAlias.OperationTime).Desc
				.SelectList(list => list
					.SelectGroup(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(() => barcodeAlias.Title).WithAlias(() => resultAlias.Barcode)
					.Select(x => x.NeedForRepair).WithAlias(() => resultAlias.NeedForRepair)
					.Select(x => x.Defect).WithAlias(() => resultAlias.Defect)
					.Select(() => stateOperationAlias.State).WithAlias(() => resultAlias.State)
					.Select(() => stateOperationAlias.OperationTime).WithAlias(() => resultAlias.OperationTime)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Nomenclature)
					.SelectSubQuery(subqueryLastEmployee).WithAlias(() => resultAlias.Employee)
				)
				.TransformUsing(Transformers.AliasToBean<ClaimsJournalNode>());
		}

		#region Действия
		private void CreateActions() {
			NodeActionsList.Clear();
			
			var receiveAction = new JournalAction("Принять в стирку",
				selected => true,
				selected => true,
				selected => Receive());
			NodeActionsList.Add(receiveAction);
			
			var cancelAction = new JournalAction("Отменить получение",
				selected => (selected.FirstOrDefault() as ClaimsJournalNode)?.State == ClaimState.WaitService,
				selected => true,
				selected => CancelReceive(selected.Cast<ClaimsJournalNode>()));
			NodeActionsList.Add(cancelAction);
			
			var changeStateAction = new JournalAction("Выполнить движение",
				selected => true,
				selected => true,
				selected => ChangeState());
			NodeActionsList.Add(changeStateAction);
		}

		private void CancelReceive(IEnumerable<ClaimsJournalNode> selected) {
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Отмена получения")) {
				var claim = uow.GetById<ServiceClaim>(selected.First().Id);
				if(claim.States.Count != 1)
					interactive.ShowMessage(ImportanceLevel.Warning, "Невозможно отменить получение, так как уже были выполнены другие движения.");
				uow.Delete(claim.States.First());
				uow.Delete(claim);
				uow.Commit();
			}
		}

		private void ChangeState() {
			throw new NotImplementedException();
		}

		private void Receive() {
			NavigationManager.OpenViewModel<ClothingReceiptViewModel>(this);
		}

		#endregion
	}

	public class ClaimsJournalNode {
		public int Id { get; set; }
		public string Barcode { get; set; }
		public string Employee { get; set; }
		public bool NeedForRepair { get; set; }
		public ClaimState State { get; set; }
		public DateTime OperationTime { get; set; }
		public string Nomenclature { get; set; }
		public string Defect { get; set; }
	}
}
