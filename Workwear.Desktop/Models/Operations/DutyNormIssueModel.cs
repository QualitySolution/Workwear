using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.SqlCommand;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;

namespace Workwear.Models.Operations {
	public class DutyNormIssueModel {
		private readonly UnitOfWorkProvider unitOfWorkProvider;

		public DutyNormIssueModel(UnitOfWorkProvider unitOfWorkProvider = null) {
			this.unitOfWorkProvider = unitOfWorkProvider;
		}
		
		#region Helpers
		private IUnitOfWork UoW => unitOfWorkProvider?.UoW;
		#endregion
		
		/// <summary>
		/// Получаем все строки дежурных норм для выбранного ответственного сотрудника. Здесь операции для дальнейшего получения числящегося.
		/// <returns></returns>
		/// 
		public List<DutyNormItem> PreloadDutyNormIssueOperations(int employeeId) {
			DutyNorm dutyNormAlias = null;
			EmployeeCard responsibleEmployeeAlias = null;
			DutyNormIssueOperation dutyNormIssueOperationAlias = null;
			var query = UoW.Session.QueryOver<DutyNormItem>()
				.JoinAlias(x => x.DutyNorm, () => dutyNormAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => dutyNormAlias.ResponsibleEmployee, () => responsibleEmployeeAlias)
				.JoinAlias(() => dutyNormIssueOperationAlias.DutyNorm, () => dutyNormAlias, JoinType.LeftOuterJoin)
				.Fetch(SelectMode.Fetch, x => x.DutyNorm)
				.Fetch(SelectMode.Fetch, () => dutyNormAlias.ResponsibleEmployee)
				.Fetch(SelectMode.Fetch,()=>dutyNormIssueOperationAlias)
				.Where(() => responsibleEmployeeAlias.Id == employeeId)
				.Future();
			return query.ToList();
		}
	}
}
