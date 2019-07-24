using System;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using workwear.Domain.Organization;

namespace workwear.Tools
{
	public static class BuisnessLogicGlobalEventHandler
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
		}

		private static void HandleDeleteEmployeeVacation(QS.DomainModel.NotifyChange.EntityChangeEvent[] changeEvents)
		{
			using(var uow = GetUnitOfWorkFactory.CreateWithoutRoot("Глобальный обработчик удаления отпусков")) {
				foreach(var employeeGroup in changeEvents.GroupBy(x => (x.Entity as EmployeeVacation).Employee.Id)) {
					var start = employeeGroup.Min(x => (DateTime)x.GetOldValue<EmployeeVacation>(e => e.BeginDate));
					var end = employeeGroup.Max(x => (DateTime)x.GetOldValue<EmployeeVacation>(e => e.EndDate));
					var employee = uow.GetById<EmployeeCard>(employeeGroup.Key);

					employee.RecalculateDatesOfIssueOperations(uow, InteractiveQuestion, start, end);
				}
				uow.Commit();
			}
		}

	}
}
