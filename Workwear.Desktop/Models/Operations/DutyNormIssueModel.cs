using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.SqlCommand;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
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
		public ObservableList<DutyNormItem> GetAllDutyNormsItemsForResponsibleEmployee(EmployeeCard employeeCard) {
			DutyNorm dutyNormAlias = null;
			EmployeeCard responsibleEmployeeAlias = null;
			DutyNormIssueOperation dutyNormIssueOperationAlias = null;
			ObservableList<DutyNormItem> allItems = new ObservableList<DutyNormItem>();
			
			var items = UoW.Session.QueryOver<DutyNormItem>()
				.JoinAlias(x => x.DutyNorm, () => dutyNormAlias)
				.JoinAlias(() => dutyNormAlias.ResponsibleEmployee, () => responsibleEmployeeAlias)
				.JoinEntityAlias(()=>dutyNormIssueOperationAlias, ()=> dutyNormIssueOperationAlias.DutyNormItem.Id == dutyNormAlias.Id, JoinType.LeftOuterJoin)
				.Where(() => responsibleEmployeeAlias.Id == employeeCard.Id)
				.List<DutyNormItem>();

			foreach(var item in items) {
				item.Update(UoW);
				allItems.Add(item);
			}
			return allItems;
		}
	}
}
