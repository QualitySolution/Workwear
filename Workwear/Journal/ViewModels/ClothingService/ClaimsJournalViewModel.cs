using System;
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

namespace workwear.Journal.ViewModels.ClothingService {
	public class ClaimsJournalViewModel : EntityJournalViewModelBase<ServiceClaim, ServiceClaimViewModel, ClaimsJournalNode> {
		public ClaimsJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, IDeleteEntityService deleteEntityService = null, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			Title = "Обслуживание одежды";
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
