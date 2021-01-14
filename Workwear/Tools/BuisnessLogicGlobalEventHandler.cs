using System;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using workwear.Domain.Operations;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using workwear.Repository.Stock;

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

			QS.DomainModel.NotifyChange.NotifyConfiguration.Instance.BatchSubscribe(HandleDeleteEmployeeIssueOperation)
				.IfEntity<EmployeeIssueOperation>()
				.AndChangeType(QS.DomainModel.NotifyChange.TypeOfChangeEvent.Delete);
		}

		private static void HandleDeleteEmployeeVacation(QS.DomainModel.NotifyChange.EntityChangeEvent[] changeEvents)
		{
			using(var uow = GetUnitOfWorkFactory.CreateWithoutRoot("Глобальный обработчик удаления отпусков")) {
				var baseParameters = new BaseParameters();
				foreach(var employeeGroup in changeEvents.GroupBy(x => (x.Entity as EmployeeVacation).Employee.Id)) {
					var start = employeeGroup.Min(x => (DateTime)x.GetOldValue<EmployeeVacation>(e => e.BeginDate));
					var end = employeeGroup.Max(x => (DateTime)x.GetOldValue<EmployeeVacation>(e => e.EndDate));
					var employee = uow.GetById<EmployeeCard>(employeeGroup.Key);

					employee.RecalculateDatesOfIssueOperations(uow, baseParameters, InteractiveQuestion, start, end);
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
					var nomenclatures = employeeGroup.Select(x => x.GetOldValueCast<EmployeeIssueOperation, Nomenclature>(e => e.Nomenclature)).ToArray();
					var types = NomenclatureRepository.GetTypesOfNomenclatures(uow, nomenclatures).ToArray();
					employee.UpdateNextIssue(types);
				}
				uow.Commit();
			}
		}
	}
}
