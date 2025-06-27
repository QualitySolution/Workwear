using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;

namespace Workwear.Repository.Regulations {
	public class DutyNormRepository {
		private readonly UnitOfWorkProvider unitOfWorkProvider;
		private IUnitOfWork repoUow;
		public IUnitOfWork RepoUow {
			get => repoUow ?? unitOfWorkProvider.UoW;
			set => repoUow = value;
		}
		public Dictionary<int, int> CalculateWrittenOff(DutyNormIssueOperation[] operations, IUnitOfWork uow, DateTime? onDate = null) {
			var wo = (uow ?? RepoUow).Session.QueryOver<DutyNormIssueOperation>()
				.Where(o => o.IssuedOperation.Id
					.IsIn(operations.Select(x => x.Id).ToArray()));
			if(onDate != null)
				wo.Where(o => o.OperationTime <= onDate);
			
			return wo.List()
				?.GroupBy(o => o.Id)
				.ToDictionary(g => g.Key, g => g.Sum(o => o.Returned));
		}

		/// <summary>
		/// Получаем все операции выдачи по дежурным нормам для выбранного ответственного сотрудника (отсортированы в порядке убывания).
		/// </summary>
		/// <returns></returns>
		/// 
		public virtual IList<DutyNormIssueOperation> AllDutyNormsForResponsibleEmployee(
			EmployeeCard employee, 
			Action<IQueryOver<DutyNormIssueOperation, DutyNormIssueOperation>> makeEager = null, IUnitOfWork uow = null)
		{
			var query = (uow ?? RepoUow).Session.QueryOver<DutyNormIssueOperation>()
				.Where(o => o.DutyNorm.ResponsibleEmployee == employee);

			makeEager?.Invoke(query);

			return query.OrderBy(x => x.OperationTime).Desc.List();
		}
	}
}
