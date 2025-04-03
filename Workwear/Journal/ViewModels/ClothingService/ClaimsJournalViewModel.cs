using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Cloud.Postomat.Client;
using QS.Cloud.Postomat.Manage;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.Utilities.Text;
using QS.ViewModels.Extension;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Company;
using Workwear.Domain.Postomats;
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

			var subqueryInDocument = QueryOver.Of<PostomatDocumentItem>()
				.Where(item => item.ServiceClaim.Id == serviceClaimAlias.Id)
				.Select(item => item.Id);

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
				.OrderBy(() => serviceClaimAlias.Id).Desc
				.SelectList(list => list
					.SelectGroup(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(() => barcodeAlias.Title).WithAlias(() => resultAlias.Barcode)
					.Select(() => employeeAlias.PersonnelNumber).WithAlias(() => resultAlias.EmployeePersonnelNumber)
					.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmployeeLastName)
					.Select(() => employeeAlias.FirstName).WithAlias(() => resultAlias.EmployeeFirstName)
					.Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)
					.Select(x => x.NeedForRepair).WithAlias(() => resultAlias.NeedForRepair)
					.Select(x => x.Defect).WithAlias(() => resultAlias.Defect)
					.Select(x => x.PreferredTerminalId).WithAlias(() => resultAlias.ReferredTerminalId)
					.Select(x => x.Comment).WithAlias(() => resultAlias.Comment)
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
				selected => (selected.FirstOrDefault() as ClaimsJournalNode)?.State == ClaimState.WaitService 
				|| (selected.FirstOrDefault() as ClaimsJournalNode)?.State == ClaimState.InReceiptTerminal,
				selected => true,
				selected => CancelReceive(selected.Cast<ClaimsJournalNode>()));
			NodeActionsList.Add(cancelAction);
		}

		private void CancelReceive(IEnumerable<ClaimsJournalNode> selected) {
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Отмена получения")) {
				var node = selected.First();
				var claim = uow.GetById<ServiceClaim>(node.Id);
				if(claim.States.Count != 1) {
					interactive.ShowMessage(ImportanceLevel.Warning, "Невозможно отменить получение, так как уже были выполнены другие движения.");
					return;
				}
				bool isTerminalReceipt = claim.States.First().State == ClaimState.InReceiptTerminal;
				var terminalWarning = isTerminalReceipt ? "Если одежда все таки находится в терминале сдачи в стирку, может возникнуть путаница. " : "";
				if(interactive.Question($"Данная операция удалит сдачу в стирку, восстановить ее будет невозможно. {terminalWarning}Вы уверены, что хотите продолжить?") == false)
					return;
				if(isTerminalReceipt) {
					//Чтобы форсировать обновление информации на терминале
					claim.Employee.LastUpdate = DateTime.Now;
					uow.Save(claim.Employee);
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
		
		public string GetTerminalLabel(uint id) => postomatsLabels.ContainsKey(id) ? postomatsLabels[id] : string.Empty;
	}

	public class ClaimsJournalNode {
		public int Id { get; set; }
		public string Barcode { get; set; }
		public string EmployeePersonnelNumber { get; set; }
		public string EmployeeFirstName { get; set; }
		public string EmployeeLastName { get; set; }
		public string EmployeePatronymic { get; set; }
		public bool NeedForRepair { get; set; }
		public bool IsClosed { get; set; }
		public ClaimState State { get; set; }
		public DateTime OperationTime { get; set; }
		public string Nomenclature { get; set; }
		public string Defect { get; set; }
		public uint ReferredTerminalId { get; set; }
		public string Comment { get; set; }

		public string Employee => PersonHelper.PersonFullName(EmployeeLastName, EmployeeFirstName, EmployeePatronymic);
		public string RowColor => IsClosed ? "grey" : null;
	}
}
