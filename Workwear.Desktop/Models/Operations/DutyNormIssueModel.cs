using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;

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
		public IObservableList<DutyNormItem> GetAllDutyNormsItemsForResponsibleEmployee(EmployeeCard employeeCard) {
			ProtectionTools protectionToolsAlias = null;
			DutyNorm dutyNormAlias = null;
			EmployeeCard responsibleEmployeeAlias = null;
			ItemsType itemsTypeAlias = null;
			var items = UoW.Session.QueryOver<DutyNormItem>()
				.JoinAlias(x => x.ProtectionTools, () => protectionToolsAlias)
				.JoinAlias(() => protectionToolsAlias.Type, () => itemsTypeAlias)
				.JoinAlias(x => x.DutyNorm, () => dutyNormAlias)
				.JoinAlias(() => dutyNormAlias.ResponsibleEmployee, () => responsibleEmployeeAlias)
				.Where(() => responsibleEmployeeAlias.Id == employeeCard.Id)
				.List();
			
			var operations = UoW.Session.QueryOver<DutyNormIssueOperation>()
				.JoinAlias(x => x.DutyNorm, () => dutyNormAlias)
				.JoinAlias(() => dutyNormAlias.ResponsibleEmployee, () => responsibleEmployeeAlias)
				.Where(() => responsibleEmployeeAlias.Id == employeeCard.Id)
				.List();
			
			foreach(var item in items)
				item.Update(UoW, operations);
			
			return new ObservableList<DutyNormItem>(items);
		}
	}
}
