using System;
using System.Linq;
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
		public static IInteractiveQuestion InteractiveQuestion;

		static IUnitOfWorkFactory unitOfWorkFactory;

		static IUnitOfWorkFactory GetUnitOfWorkFactory => unitOfWorkFactory ?? UnitOfWorkFactory.GetDefaultFactory;

		public static void Init(IInteractiveQuestion interactiveQuestion, IUnitOfWorkFactory uowFactory = null)
		{
			InteractiveQuestion = interactiveQuestion;

			if(uowFactory != null)
				unitOfWorkFactory = uowFactory;

			QS.DomainModel.NotifyChange.NotifyConfiguration.Instance.BatchSubscribe(HandleDeleteEmployeeVacation)
				.IfEntity<EmployeeVacation>()
				.AndChangeType(QS.DomainModel.NotifyChange.TypeOfChangeEvent.Delete);

			QS.DomainModel.NotifyChange.NotifyConfiguration.Instance.BatchSubscribe(HandleDeleteEmployeeIssueOperation)
				.IfEntity<EmployeeIssueOperation>()
				.AndChangeType(QS.DomainModel.NotifyChange.TypeOfChangeEvent.Delete);
		}

		private static void HandleDeleteEmployeeVacation(QS.DomainModel.NotifyChange.EntityChangeEvent[] changeEvents)
		{
			using(var uow = GetUnitOfWorkFactory.CreateWithoutRoot("Глобальный обработчик удаления отпусков")) {
				var baseParameters = new BaseParameters(uow.Session.Connection);
				foreach(var employeeGroup in changeEvents.GroupBy(x => (x.Entity as EmployeeVacation).Employee.Id)) {
					var start = employeeGroup.Min(x => (DateTime)x.GetOldValue<EmployeeVacation>(e => e.BeginDate));
					var end = employeeGroup.Max(x => (DateTime)x.GetOldValue<EmployeeVacation>(e => e.EndDate));
					var employee = uow.GetById<EmployeeCard>(employeeGroup.Key);

					employee.RecalculateDatesOfIssueOperations(uow, new EmployeeIssueRepository(new UnitOfWorkProvider(uow)), baseParameters, InteractiveQuestion, start, end);
				}
				uow.Commit();
			}
		}

		private static void HandleDeleteEmployeeIssueOperation(QS.DomainModel.NotifyChange.EntityChangeEvent[] changeEvents)
		{
			using(var uow = GetUnitOfWorkFactory.CreateWithoutRoot("Глобальный обработчик удаления операций выдачи")) {
				foreach(var employeeGroup in changeEvents.GroupBy(x => (x.Entity as EmployeeIssueOperation).Employee.Id)) {
					var employee = uow.GetById<EmployeeCard>(employeeGroup.Key);
					if(employee == null)
						continue; //Видимо сотрудник был удален, поэтому пересчитывать глупо.
					var protectionTools = employeeGroup.Select(x => x.GetOldValueCast<EmployeeIssueOperation, ProtectionTools>(e => e.ProtectionTools))
						.Where(x => x != null).Distinct().ToArray();
					employee.FillWearReceivedInfo(new EmployeeIssueRepository(uow));
					employee.UpdateNextIssue(protectionTools);
				}
				uow.Commit();
			}
		}
	}
}
