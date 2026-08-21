using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Cloud.Postomat.Client;
using QS.Cloud.Postomat.Manage;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Journal;
using QS.Project.Services;
using QS.ViewModels.Extension;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Postomats;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Journal.Filter.ViewModels.ClothingService;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.ViewModels.ClothingService;

namespace workwear.Journal.ViewModels.ClothingService {
	public class ClaimsJournalViewModel : EntityJournalViewModelBase<ServiceClaim, ServiceClaimViewModel, ClaimsJournalNode>, IDialogDocumentation {
		private IInteractiveService interactive;
		public readonly FeaturesService FeaturesService;
		readonly IDictionary<uint, string> postomatsLabels = new Dictionary<uint, string>();

		#region Внешние прараметры
		public bool ExcludeInDocs = false;
		#endregion

		public ClaimsJournalFilterViewModel Filter { get; set; }

		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("employees.html#employees");
		public string ButtonTooltip => DocHelper.GetJournalDocTooltip(typeof(ServiceClaim));
		#endregion
		public ClaimsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ILifetimeScope autofacScope,
			FeaturesService featuresService,
			PostomatManagerService postomatService,
			IDeleteEntityService deleteEntityService = null,
			ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			interactive = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			this.FeaturesService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			if(postomatService == null) throw new ArgumentNullException(nameof(postomatService));
			Title = "Обслуживание одежды";
			JournalFilter = Filter = autofacScope.Resolve<ClaimsJournalFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));

			if(featuresService.Available(WorkwearFeature.Postomats))
				postomatsLabels = postomatService.GetPostomatList(PostomatListType.Aso).ToDictionary(x => x.Id, x => $"{x.Name} ({x.Location})");

			CreateActions();
			UpdateOnChanges(typeof(ServiceClaim), typeof(StateOperation));
		}

		private static IProjection ShortNameProjection(IProjection lastName, IProjection firstName, IProjection patronymic) =>
			Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String,
					"CONCAT_WS(' ', ?1, NULLIF(CONCAT(" +
					"IF(?2 IS NOT NULL AND ?2 <> '', CONCAT(LEFT(?2, 1), '.'), ''), " +
					"IF(?3 IS NOT NULL AND ?3 <> '', CONCAT(LEFT(?3, 1), '.'), '')" +
					"), ''))"),
				NHibernateUtil.String,
				lastName, firstName, patronymic);

		protected override IQueryOver<ServiceClaim> ItemsQuery(IUnitOfWork uow) {
			ClaimsJournalNode resultAlias = null;
			ServiceClaim serviceClaimAlias = null;
			StateOperation stateOperationAlias = null;
			PostomatDocument postomatDocumentAlias = null;
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

			var subqueryInDocument = QueryOver.Of<PostomatDocumentItem>()
				.Left.JoinAlias(x => x.Document, () => postomatDocumentAlias)
				.Where(() => postomatDocumentAlias.Status != DocumentStatus.Deleted)
				.Where(item => item.ServiceClaim.Id == serviceClaimAlias.Id)
				.Select(item => item.Id);
			
			BarcodeOperation lastOperationSubAlias = null;
			EmployeeIssueOperation lastEmpSubAlias = null;
			DutyNormIssueOperation lastDutyNormSubAlias = null;
			OverNormOperation lastOverNormSubAlias = null;
			WarehouseOperation lastWhSubAlias = null;

			var lastOperationIdSubQuery = QueryOver.Of(() => lastOperationSubAlias)
				.Left.JoinAlias(() => lastOperationSubAlias.EmployeeIssueOperation, () => lastEmpSubAlias)
				.Left.JoinAlias(() => lastOperationSubAlias.DutyNormIssueOperation, () => lastDutyNormSubAlias)
				.Left.JoinAlias(() => lastOperationSubAlias.OverNormOperation, () => lastOverNormSubAlias)
				.Left.JoinAlias(() => lastOperationSubAlias.WarehouseOperation, () => lastWhSubAlias)
				.Where(() => lastOperationSubAlias.Barcode.Id == barcodeAlias.Id)
				.OrderBy(Projections.SqlFunction("coalesce", NHibernateUtil.Date,
					Projections.Property(() => lastEmpSubAlias.OperationTime),
					Projections.Property(() => lastDutyNormSubAlias.OperationTime),
					Projections.Property(() => lastOverNormSubAlias.OperationTime),
					Projections.Property(() => lastWhSubAlias.OperationTime)))
					.Desc
				.Select(x => x.Id)
				.Take(1);

			BarcodeOperation lastBarcodeOperationAlias = null;
			DutyNormIssueOperation lastDutyNormIssueOperationAlias = null;
			DutyNorm lastDutyNormAlias = null;
			EmployeeCard lastDutyNormResponsibleEmployeeAlias = null;
			Leader lastDutyNormResponsibleLeaderAlias = null;
			OverNormOperation lastOverNormOperationAlias = null;
			EmployeeCard lastOverNormEmployeeAlias = null;
			WarehouseOperation lastWarehouseOperationAlias = null;
			Warehouse lastReceiptWarehouseAlias = null;

			var query = uow.Session.QueryOver(() => serviceClaimAlias);
			if(!Filter.ShowClosed)
				query.Where(x => x.IsClosed == false);
			if(Filter.ShowOnlyRepair)
				query.Where(x => x.NeedForRepair == true);
			if(Filter.PostomatId != 0)
				query.Where(x => x.PreferredTerminalId == Filter.PostomatId);
			if(Filter.Status != null)
				query.WithSubquery.WhereValue(Filter.Status).Eq(subqueryLastState);

			if(ExcludeInDocs)
				query.WithSubquery.WhereNotExists(subqueryInDocument);

			return query
				.Where(GetSearchCriterion(
					() => nomenclatureAlias.Name,
					() => barcodeAlias.Title,
					() => employeeAlias.PersonnelNumber,
					() => employeeAlias.LastName,
					() => employeeAlias.FirstName,
					() => employeeAlias.Patronymic,
					() => serviceClaimAlias.Comment
					)
				)
				.Left.JoinAlias(x => x.Barcode, () => barcodeAlias)
				.Left.JoinAlias(() => barcodeAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias( x => x.Employee, () => employeeAlias)
				.Left.JoinAlias(() => barcodeAlias.BarcodeOperations, () => lastBarcodeOperationAlias,
					Subqueries.WhereProperty(() => lastBarcodeOperationAlias.Id).Eq(lastOperationIdSubQuery))
				.Left.JoinAlias(() => lastBarcodeOperationAlias.DutyNormIssueOperation, () => lastDutyNormIssueOperationAlias,
					Restrictions.Gt(Projections.Property(() => lastDutyNormIssueOperationAlias.Issued), 0))
				.Left.JoinAlias(() => lastDutyNormIssueOperationAlias.DutyNorm, () => lastDutyNormAlias)
				.Left.JoinAlias(() => lastDutyNormAlias.ResponsibleEmployee, () => lastDutyNormResponsibleEmployeeAlias)
				.Left.JoinAlias(() => lastDutyNormAlias.ResponsibleLeader, () => lastDutyNormResponsibleLeaderAlias)
				.Left.JoinAlias(() => lastBarcodeOperationAlias.OverNormOperation, () => lastOverNormOperationAlias)
				.Left.JoinAlias(() => lastOverNormOperationAlias.Employee, () => lastOverNormEmployeeAlias)
				.Left.JoinAlias(() => lastBarcodeOperationAlias.WarehouseOperation, () => lastWarehouseOperationAlias)
				.Left.JoinAlias(() => lastWarehouseOperationAlias.ReceiptWarehouse, () => lastReceiptWarehouseAlias)
				.OrderBy(() => serviceClaimAlias.Id).Desc
				.SelectList(list => list
					.SelectGroup(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(() => barcodeAlias.Title).WithAlias(() => resultAlias.Barcode)
					.Select(() => employeeAlias.PersonnelNumber).WithAlias(() => resultAlias.EmployeePersonnelNumber)
					.Select(Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(' ', ?1, ?2, ?3)"),
						NHibernateUtil.String,
						Projections.Property(() => employeeAlias.LastName),
						Projections.Property(() => employeeAlias.FirstName),
						Projections.Property(() => employeeAlias.Patronymic)))
						.WithAlias(() => resultAlias.EmployeeFullName)
					.Select(x => x.NeedForRepair).WithAlias(() => resultAlias.NeedForRepair)
					.Select(x => x.Defect).WithAlias(() => resultAlias.Defect)
					.Select(x => x.PreferredTerminalId).WithAlias(() => resultAlias.ReferredTerminalId)
					.Select(x => x.Comment).WithAlias(() => resultAlias.Comment)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Nomenclature)
					.Select(x => x.IsClosed).WithAlias(() => resultAlias.IsClosed)
					.SelectSubQuery(subqueryLastState).WithAlias(() => resultAlias.State)
					.SelectSubQuery(subqueryLastOperationTime).WithAlias(() => resultAlias.OperationTime)
					.Select(() => lastOverNormOperationAlias.Type).WithAlias(() => resultAlias.LastOverNormType)
					.Select(ShortNameProjection(
						Projections.Property(() => lastOverNormEmployeeAlias.LastName),
						Projections.Property(() => lastOverNormEmployeeAlias.FirstName),
						Projections.Property(() => lastOverNormEmployeeAlias.Patronymic)))
						.WithAlias(() => resultAlias.LastOverNormEmployeeShortName)
					.Select(() => lastReceiptWarehouseAlias.Id).WithAlias(() => resultAlias.LastOperationReceiptWarehouseId)
					.Select(() => lastReceiptWarehouseAlias.Name).WithAlias(() => resultAlias.LastOperationReceiptWarehouseName)
					.Select(() => lastDutyNormAlias.Id).WithAlias(() => resultAlias.LastDutyNormId)
					.Select(ShortNameProjection(
						Projections.Property(() => lastDutyNormResponsibleEmployeeAlias.LastName),
						Projections.Property(() => lastDutyNormResponsibleEmployeeAlias.FirstName),
						Projections.Property(() => lastDutyNormResponsibleEmployeeAlias.Patronymic)))
						.WithAlias(() => resultAlias.LastDutyNormResponsibleEmployeeShortName)
					.Select(ShortNameProjection(
						Projections.Property(() => lastDutyNormResponsibleLeaderAlias.Surname),
						Projections.Property(() => lastDutyNormResponsibleLeaderAlias.Name),
						Projections.Property(() => lastDutyNormResponsibleLeaderAlias.Patronymic)))
						.WithAlias(() => resultAlias.LastDutyNormResponsibleLeaderShortName)
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

			var cancelAction = new JournalAction("Удалить заявку",
				selected => selected.FirstOrDefault() != null,
				selected => true,
				selected => RemoveClaim(selected.Cast<ClaimsJournalNode>()));
			NodeActionsList.Add(cancelAction);
		}

		private void RemoveClaim(IEnumerable<ClaimsJournalNode> selected) {
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Удаление заявки на обслуживание")) {
				var node = selected.First();
				var claim = uow.GetById<ServiceClaim>(node.Id);
				var postomatDocumentItems = uow.Session.QueryOver<PostomatDocumentItem>().Where(x => x.ServiceClaim.Id == claim.Id).RowCount();
				if(uow.Session.QueryOver<PostomatDocumentItem>().Where(x => x.ServiceClaim.Id == claim.Id).RowCount() != 0) {
					interactive.ShowMessage(ImportanceLevel.Warning, "Заявка уже добавлена в документ для закладки в постомат. Её нельзя удалить.");
					return;
				}
				bool isTerminalReceipt = claim.States.OrderByDescending(i => i.OperationTime).FirstOrDefault()?.State == ClaimState.InReceiptTerminal;
				var terminalWarning = isTerminalReceipt ? "Если одежда все таки находится в терминале сдачи в стирку, может возникнуть путаница. " : "";
				if(interactive.Question($"Данная операция удалит сдачу в стирку, восстановить ее будет невозможно. {terminalWarning}Вы уверены, что хотите продолжить?") == false)
					return;
				if(isTerminalReceipt) {
					//Чтобы форсировать обновление информации на терминале
					claim.Employee.LastUpdate = DateTime.Now;
					uow.Save(claim.Employee);
				}
				foreach(var state in claim.States)
					uow.Delete(state);
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

		public string GetTerminalLabel(uint id) => postomatsLabels.ContainsKey(id) ? postomatsLabels[id] : string.Empty;
	}

	public class ClaimsJournalNode {
		public int Id { get; set; }
		public string Barcode { get; set; }
		public string EmployeePersonnelNumber { get; set; }
		public string EmployeeFullName { get; set; }
		public bool NeedForRepair { get; set; }
		public bool IsClosed { get; set; }
		public ClaimState State { get; set; }
		public DateTime OperationTime { get; set; }
		public string Nomenclature { get; set; }
		public string Defect { get; set; }
		public uint ReferredTerminalId { get; set; }
		public string Comment { get; set; }

		public OverNormType? LastOverNormType { get; set; }
		public string LastOverNormEmployeeShortName { get; set; }
		public int? LastOperationReceiptWarehouseId { get; set; }
		public string LastOperationReceiptWarehouseName { get; set; }
		public int? LastDutyNormId { get; set; }
		public string LastDutyNormResponsibleEmployeeShortName { get; set; }
		public string LastDutyNormResponsibleLeaderShortName { get; set; }

		public string EmployeeString {
			get {
				if(LastDutyNormId.HasValue) {
					var responsible = !string.IsNullOrEmpty(LastDutyNormResponsibleEmployeeShortName)
						? LastDutyNormResponsibleEmployeeShortName
						: LastDutyNormResponsibleLeaderShortName;
					return $"Дежурная №{LastDutyNormId} {responsible}".TrimEnd();
				}
				if(LastOverNormType.HasValue && !LastOperationReceiptWarehouseId.HasValue)
					return $"{LastOverNormType.Value.GetEnumTitle()} {LastOverNormEmployeeShortName}";
				if(LastOperationReceiptWarehouseId.HasValue)
					return LastOperationReceiptWarehouseName;
				return EmployeeFullName;
			}
		}
		public string RowColor => IsClosed ? "grey" : null;
	}
}
