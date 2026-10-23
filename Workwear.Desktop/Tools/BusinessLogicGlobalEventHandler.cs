using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NHibernate;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Visits;
using Workwear.Models.Operations;
using Workwear.Models.Supply;
using Workwear.Models.Visits;
using Workwear.Repository.Operations;

namespace Workwear.Tools
{
	public static class BusinessLogicGlobalEventHandler
	{
		static ILifetimeScope LifetimeScope;

		public static void Init(ILifetimeScope lifetimeScope)
		{
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));

			QS.DomainModel.NotifyChange.NotifyConfiguration.Instance.BatchSubscribe(HandleDeleteEmployeeVacation)
				.IfEntity<EmployeeVacation>()
				.AndChangeType(QS.DomainModel.NotifyChange.TypeOfChangeEvent.Delete);

			QS.DomainModel.NotifyChange.NotifyConfiguration.Instance.BatchSubscribe(HandleDeleteEmployeeIssueOperation)
				.IfEntity<EmployeeIssueOperation>()
				.AndChangeType(QS.DomainModel.NotifyChange.TypeOfChangeEvent.Delete);

			QS.DomainModel.NotifyChange.NotifyConfiguration.Instance.BatchSubscribe(HandleUpdateEmployeeIssueOperation)
				.IfEntity<EmployeeIssueOperation>()
				.AndChangeType(QS.DomainModel.NotifyChange.TypeOfChangeEvent.Update);

			QS.DomainModel.NotifyChange.NotifyConfiguration.Instance.BatchSubscribe(HandleDeleteReturnServiceClaim)
				.IfEntity<Return>()
				.AndChangeType(QS.DomainModel.NotifyChange.TypeOfChangeEvent.Delete);

			QS.DomainModel.NotifyChange.NotifyConfiguration.Instance.BatchSubscribe(HandleDeleteIncome)
				.IfEntity<Income>()
				.AndChangeType(QS.DomainModel.NotifyChange.TypeOfChangeEvent.Delete);

			QS.DomainModel.NotifyChange.NotifyConfiguration.Instance.BatchSubscribe(HandleChangeCollectiveExpense)
				.IfEntity<CollectiveExpense>()
				.Or.IfEntity<CollectiveExpenseItem>();
		}

		private static void HandleChangeCollectiveExpense(QS.DomainModel.NotifyChange.EntityChangeEvent[] changeEvents)
		{
			var issuanceRequestIds = new HashSet<int>();
			foreach(var changeEvent in changeEvents) {
				if(changeEvent.Entity is CollectiveExpense document) {
					if(document.IssuanceRequest != null)
						issuanceRequestIds.Add(document.IssuanceRequest.Id);
					//Если заявку отвязали или изменили.
					var oldRequest = changeEvent.GetOldValueCast<CollectiveExpense, IssuanceRequest>(e => e.IssuanceRequest);
					if(oldRequest != null)
						issuanceRequestIds.Add(oldRequest.Id);
				}
				else if(changeEvent.Entity is CollectiveExpenseItem item
				   && item.Document != null
				   && NHibernateUtil.IsInitialized(item.Document)
				   && item.Document.IssuanceRequest != null)
					issuanceRequestIds.Add(item.Document.IssuanceRequest.Id);
			}
			if(!issuanceRequestIds.Any())
				return;

			using(var scope = LifetimeScope.BeginLifetimeScope()) {
				var unitOfWorkFactory = scope.Resolve<IUnitOfWorkFactory>();
				using(var uow = unitOfWorkFactory.CreateWithoutRoot("Глобальный обработчик пересчета статуса заявок на выдачу")) {
					var baseParameters = scope.Resolve<BaseParameters>();
					var employeeIssueRepository = scope.Resolve<EmployeeIssueRepository>(new TypedParameter(typeof(UnitOfWorkProvider), new UnitOfWorkProvider(uow)));
					var issueModel = new EmployeeIssueModel(employeeIssueRepository, new UnitOfWorkProvider(uow));
					var issuanceRequestModel = new IssuanceRequestModel(issueModel, baseParameters, new UnitOfWorkProvider(uow));
					issuanceRequestModel.UpdateStatuses(issuanceRequestIds);
					uow.Commit();
				}
			}
		}

		private static void HandleDeleteEmployeeVacation(QS.DomainModel.NotifyChange.EntityChangeEvent[] changeEvents)
		{
			using(var scope = LifetimeScope.BeginLifetimeScope()) {
				var unitOfWorkFactory = scope.Resolve<IUnitOfWorkFactory>();
				using(var uow = unitOfWorkFactory.CreateWithoutRoot("Глобальный обработчик удаления отпусков")) {
					var baseParameters = scope.Resolve<BaseParameters>();
					var interactive = scope.Resolve<IInteractiveQuestion>();
					var employeeIssueRepository = scope.Resolve<EmployeeIssueRepository>(new TypedParameter(typeof(UnitOfWorkProvider), new UnitOfWorkProvider(uow)));
					foreach(var employeeGroup in changeEvents.GroupBy(x => (x.Entity as EmployeeVacation).Employee.Id)) {
						var start = employeeGroup.Min(x => (DateTime)x.GetOldValue<EmployeeVacation>(e => e.BeginDate));
						var end = employeeGroup.Max(x => (DateTime)x.GetOldValue<EmployeeVacation>(e => e.EndDate));
						var employee = uow.GetById<EmployeeCard>(employeeGroup.Key);

						employee.RecalculateDatesOfIssueOperations(uow, employeeIssueRepository, baseParameters, interactive, start, end);
					}
					uow.Commit();
				}
			}
		}

		private static void HandleDeleteEmployeeIssueOperation(QS.DomainModel.NotifyChange.EntityChangeEvent[] changeEvents)
		{
			using(var scope = LifetimeScope.BeginLifetimeScope()) {
				var unitOfWorkFactory = scope.Resolve<IUnitOfWorkFactory>();
				using(var uow = unitOfWorkFactory.CreateWithoutRoot("Глобальный обработчик удаления операций выдачи")) {
					var employeeIssueRepository = scope.Resolve<EmployeeIssueRepository>(new TypedParameter(typeof(UnitOfWorkProvider), new UnitOfWorkProvider(uow)));
					foreach(var employeeGroup in changeEvents.GroupBy(x => (x.Entity as EmployeeIssueOperation).Employee.Id)) {
						var employee = uow.GetById<EmployeeCard>(employeeGroup.Key);
						if(employee == null)
							continue; //Видимо сотрудник был удален, поэтому пересчитывать глупо.
						var protectionTools = employeeGroup.Select(x =>
								x.GetOldValueCast<EmployeeIssueOperation, ProtectionTools>(e => e.ProtectionTools))
							.Where(x => x != null).Distinct().ToArray();
						employee.FillWearReceivedInfo(employeeIssueRepository);
						employee.UpdateNextIssue(uow, protectionTools);
					}
					uow.Commit();
				}
			}
		}

		[ThreadStatic]
		private static bool isRecalculatingIssueOperation;

		private static void HandleUpdateEmployeeIssueOperation(QS.DomainModel.NotifyChange.EntityChangeEvent[] changeEvents)
		{
			if(isRecalculatingIssueOperation)
				return;

			isRecalculatingIssueOperation = true;
			try {
				using(var scope = LifetimeScope.BeginLifetimeScope()) {
					var unitOfWorkFactory = scope.Resolve<IUnitOfWorkFactory>();
					using(var uow = unitOfWorkFactory.CreateWithoutRoot("Глобальный обработчик изменения операций выдачи")) {
						var interactive = scope.Resolve<IInteractiveQuestion>();
						var baseParameters = scope.Resolve<BaseParameters>();
						var employeeIssueRepository = scope.Resolve<EmployeeIssueRepository>(new TypedParameter(typeof(UnitOfWorkProvider), new UnitOfWorkProvider(uow)));
						var issueModel = new EmployeeIssueModel(employeeIssueRepository, new UnitOfWorkProvider(uow));

						//Возврат, списание должны быть в той же номенклатуре нормы, что и выдача, иначе граф их не учтёт.
						var changedProtectionToolsIds = changeEvents
							.Where(x => x.UpdateEvent?.OldState != null
								&& (x.Entity as EmployeeIssueOperation).Issued > 0
								&& !DomainHelper.EqualDomainObjects(
									x.GetOldValueCast<EmployeeIssueOperation, ProtectionTools>(e => e.ProtectionTools),
									(x.Entity as EmployeeIssueOperation).ProtectionTools))
							.Select(x => (x.Entity as EmployeeIssueOperation).Id)
							.ToArray();
						if(changedProtectionToolsIds.Any()) {
							foreach(var child in employeeIssueRepository.GetChildOperations(changedProtectionToolsIds, uow)
								        .Where(c => !DomainHelper.EqualDomainObjects(c.ProtectionTools, c.IssuedOperation.ProtectionTools))) {
								child.ProtectionTools = child.IssuedOperation.ProtectionTools;
								uow.Save(child);
							}
							uow.Commit();
						}

						var operationsToRecalculate = new List<EmployeeIssueOperation>();
						foreach(var employeeGroup in changeEvents.GroupBy(x => (x.Entity as EmployeeIssueOperation).Employee.Id)) {
							var employee = uow.GetById<EmployeeCard>(employeeGroup.Key);
							if(employee == null)
								continue;
							operationsToRecalculate.AddRange(
								employeeGroup
									.Select(x => uow.GetById<EmployeeIssueOperation>((x.Entity as EmployeeIssueOperation).Id))
									.Where(x => x != null));
						}

						if(operationsToRecalculate.Any())
							issueModel.RecalculateDateOfIssue(operationsToRecalculate, baseParameters, interactive, uow: uow);
					}
				}
			} finally {
				isRecalculatingIssueOperation = false;
			}
		}

		private static void HandleDeleteIncome(QS.DomainModel.NotifyChange.EntityChangeEvent[] changeEvents)
		{
			using(var scope = LifetimeScope.BeginLifetimeScope()) {
				var unitOfWorkFactory = scope.Resolve<IUnitOfWorkFactory>();
				using(var uow = unitOfWorkFactory.CreateWithoutRoot("Глобальный обработчик удаления документов поступления на склад")) {
					ShipmentCalculateModel sCalcModel = scope.Resolve<ShipmentCalculateModel>(new TypedParameter(typeof(UnitOfWorkProvider), new UnitOfWorkProvider(uow)));
					foreach(var doc in changeEvents.Select(x => (x.Entity as Income)))
						if(doc?.Shipment != null)
							sCalcModel.UpdateShipment(doc.Shipment.Id, uow);
					uow.Commit();
				}
			}
		}

		private static void HandleDeleteReturnServiceClaim(QS.DomainModel.NotifyChange.EntityChangeEvent[] changeEvents) {
			using(var scope = LifetimeScope.BeginLifetimeScope()) {
				var unitOfWorkFactory = scope.Resolve<IUnitOfWorkFactory>();
				using(var uow = unitOfWorkFactory.CreateWithoutRoot("Глобальный обработчик удаления возврата с обслуживания одежды")) {
					var deletedReturnClaims = changeEvents
						.Select(x => x.Entity as Return)
						.Where(x => x != null && x.Items.Any(item => item.ServiceClaim != null))
						.ToList();
					if(!deletedReturnClaims.Any())
						return;

					foreach(var returnDoc in deletedReturnClaims) {
						foreach(var item in returnDoc.Items.Where(i => i.ServiceClaim != null)) {
							var serviceClaim = uow.GetById<ServiceClaim>(item.ServiceClaim.Id);
							if(serviceClaim == null)
								continue;

							serviceClaim.IsClosed = false;
							serviceClaim.ChangeState(ClaimState.AwaitIssue);
							uow.Save(serviceClaim);
						}
					}
					uow.Commit();
				}

			}
		}
	}
}
