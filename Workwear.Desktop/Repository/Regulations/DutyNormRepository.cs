using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;

namespace Workwear.Repository.Regulations {
	public class DutyNormRepository {
		private UnitOfWorkProvider unitOfWorkProvider;
		private IUnitOfWork UoW => unitOfWorkProvider?.UoW;
		
		public DutyNormRepository(UnitOfWorkProvider unitOfWorkProvider) {
			this.unitOfWorkProvider = unitOfWorkProvider;
		}
		public Dictionary<int, int> CalculateWrittenOff(DutyNormIssueOperation[] operations, IUnitOfWork uow, DateTime? onDate = null) {
			var wo = uow.Session.QueryOver<DutyNormIssueOperation>()
				.Where(o => o.IssuedOperation.Id
					.IsIn(operations.Select(x => x.Id).ToArray()));
			if(onDate != null)
				wo.Where(o => o.OperationTime <= onDate);
			
			return wo.List()
				?.GroupBy(o => o.Id)
				.ToDictionary(g => g.Key, g => g.Sum(o => o.Returned));
		}

		/// <summary>
		/// Получаем все дежурные нормам для выбранного ответственного сотрудника (отсортированы в порядке убывания).
		/// </summary>
		/// <returns></returns>
		/// 
		public virtual IList<DutyNormIssueOperation> AllDutyNormsForResponsibleEmployee(
			EmployeeCard employee, 
			IUnitOfWork uow = null) {
			DutyNormIssueOperation dutyNormIssueOperationAlias = null;
			DutyNorm dutyNormAlias = null;
			EmployeeCard employeeCardAlias = null;
			var query = (uow ?? UoW).Session.QueryOver<DutyNormIssueOperation>(()=>dutyNormIssueOperationAlias)
				.JoinEntityAlias(() => dutyNormAlias, (() => dutyNormIssueOperationAlias.DutyNorm.Id == dutyNormAlias.Id))
				.JoinEntityAlias(() => employeeCardAlias, () => dutyNormAlias.ResponsibleEmployee.Id == employeeCardAlias.Id)
				.Where((() => employeeCardAlias.Id == employee.Id));
			return query.OrderBy(x => x.OperationTime).Desc.List();
		}
	}
}
