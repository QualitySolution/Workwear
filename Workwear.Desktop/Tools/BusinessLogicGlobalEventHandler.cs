using System;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
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
						employee.UpdateNextIssue(protectionTools);
					}

					uow.Commit();
				}
			}
		}
	}
}
