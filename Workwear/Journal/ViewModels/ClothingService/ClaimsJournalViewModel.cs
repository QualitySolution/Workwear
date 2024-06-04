using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.Utilities.Text;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;
using Workwear.Journal.Filter.ViewModels.ClothingService;
using Workwear.ViewModels.ClothingService;

namespace workwear.Journal.ViewModels.ClothingService {
	public class ClaimsJournalViewModel : EntityJournalViewModelBase<ServiceClaim, ServiceClaimViewModel, ClaimsJournalNode> {
		private IInteractiveService interactive;
		
		public ClaimsJournalFilterViewModel Filter { get; set; }
		public ClaimsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ILifetimeScope autofacScope,
			IDeleteEntityService deleteEntityService = null,
			ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			interactive = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			Title = "Обслуживание одежды";
			JournalFilter = Filter = autofacScope.Resolve<ClaimsJournalFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));
			
			CreateActions();
			UpdateOnChanges(typeof(ServiceClaim), typeof(StateOperation));
		}

		protected override IQueryOver<ServiceClaim> ItemsQuery(IUnitOfWork uow) {
			ClaimsJournalNode resultAlias = null;
			ServiceClaim serviceClaimAlias = null;
			StateOperation stateOperationAlias = null;
			Barcode barcodeAlias = null;
			Nomenclature nomenclatureAlias = null;
			EmployeeCard employeeAlias = null;
			
			var subqueryLastState = QueryOver.Of<StateOperation>(() => stateOperationAlias)
				.Where(() => serviceClaimAlias.Id == stateOperationAlias.Claim.Id)
				.OrderBy(() => stateOperationAlias.OperationTime).Desc
				.Select(x => x.State)
				.Take(1);
			
			var subqueryLastOperationTime = QueryOver.Of<StateOperation>(() => stateOperationAlias)
				.Where(() => serviceClaimAlias.Id == stateOperationAlias.Claim.Id)
				.OrderBy(() => stateOperationAlias.OperationTime).Desc
				.Select(x => x.OperationTime)
				.Take(1);

			var query = uow.Session.QueryOver(() => serviceClaimAlias);
			if(!Filter.ShowClosed)
				query.Where(x => x.IsClosed == false);

			return query
				.Where(GetSearchCriterion(
					() => nomenclatureAlias.Name,
					() => barcodeAlias.Title
					))
				.Left.JoinAlias(x => x.Barcode, () => barcodeAlias)
				.Left.JoinAlias(() => barcodeAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias( x => x.Employee, () => employeeAlias)
				.OrderBy(() => serviceClaimAlias.Id).Desc
				.SelectList(list => list
					.SelectGroup(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(() => barcodeAlias.Title).WithAlias(() => resultAlias.Barcode)
					.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmployeeLastName)
					.Select(() => employeeAlias.FirstName).WithAlias(() => resultAlias.EmployeeFirstName)
					.Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)
					.Select(x => x.NeedForRepair).WithAlias(() => resultAlias.NeedForRepair)
					.Select(x => x.Defect).WithAlias(() => resultAlias.Defect)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Nomenclature)
					.Select(x => x.IsClosed).WithAlias(() => resultAlias.IsClosed)
					.SelectSubQuery(subqueryLastState).WithAlias(() => resultAlias.State)
					.SelectSubQuery(subqueryLastOperationTime).WithAlias(() => resultAlias.OperationTime)
				)
				.TransformUsing(Transformers.AliasToBean<ClaimsJournalNode>());
		}

		#region Действия
		private void CreateActions() {
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			
			var receiveAction = new JournalAction("Принять в стирку",
				selected => true,
				selected => true,
				selected => Receive());
			NodeActionsList.Add(receiveAction);

			var changeStateAction = new JournalAction("Выполнить движение",
				selected => true,
				selected => true,
				selected => ChangeState());
			NodeActionsList.Add(changeStateAction);
			
			var cancelAction = new JournalAction("Отменить получение",
				selected => (selected.FirstOrDefault() as ClaimsJournalNode)?.State == ClaimState.WaitService,
				selected => true,
				selected => CancelReceive(selected.Cast<ClaimsJournalNode>()));
			NodeActionsList.Add(cancelAction);
		}

		private void CancelReceive(IEnumerable<ClaimsJournalNode> selected) {
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Отмена получения")) {
				var claim = uow.GetById<ServiceClaim>(selected.First().Id);
				if(claim.States.Count != 1) {
					interactive.ShowMessage(ImportanceLevel.Warning, "Невозможно отменить получение, так как уже были выполнены другие движения.");
					return;
				}
				uow.Delete(claim.States.First());
				uow.Delete(claim);
				uow.Commit();
			}
		}

		private void ChangeState() {
			NavigationManager.OpenViewModel<ClothingMoveViewModel>(this);
		}

		private void Receive() {
			NavigationManager.OpenViewModel<ClothingReceiptViewModel>(this);
		}

		#endregion
	}

	public class ClaimsJournalNode {
		public int Id { get; set; }
		public string Barcode { get; set; }
		public string EmployeeFirstName { get; set; }
		public string EmployeeLastName { get; set; }
		public string EmployeePatronymic { get; set; }
		public bool NeedForRepair { get; set; }
		public bool IsClosed { get; set; }
		public ClaimState State { get; set; }
		public DateTime OperationTime { get; set; }
		public string Nomenclature { get; set; }
		public string Defect { get; set; }

		public string Employee => PersonHelper.PersonFullName(EmployeeLastName, EmployeeFirstName, EmployeePatronymic);
		public string RowColor => IsClosed ? "grey" : null;
	}
}
