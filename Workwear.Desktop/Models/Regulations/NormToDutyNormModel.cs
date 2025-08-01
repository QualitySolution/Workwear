using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;

namespace Workwear.Models.Regulations {
	public class NormToDutyNormModel {
		public virtual void CopyDataFromNorm(int normId, int employeeId) {
			DutyNorm newDutyNorm = new DutyNorm();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Копирование данных из нормы")) {
				var norm = uow.GetById<Norm>(normId);
				var employee = uow.GetById<EmployeeCard>(employeeId);
				newDutyNorm.Name = norm.Name != null ? $"{norm.Name} ({employee.ShortName})" : $"Дежурная ({employee.ShortName})";
				newDutyNorm.ResponsibleEmployee = employee;
				newDutyNorm.DateFrom = norm.DateFrom;
				newDutyNorm.DateTo = norm.DateTo;
				newDutyNorm.Comment = norm.Comment ?? "";
				newDutyNorm.Subdivision = employee.Subdivision;
				uow.Save(newDutyNorm);

				foreach(var item in norm.Items) {
					var dutyNormItem = CopyNormItems(newDutyNorm, item);
					uow.Save(dutyNormItem);
				}
				uow.Commit();
			}
		}

		public virtual DutyNormItem CopyNormItems(DutyNorm newDutyNorm, NormItem normItem) {
			DutyNormItem newDutyNormItem = new DutyNormItem();
			
			newDutyNormItem.DutyNorm = newDutyNorm;
			newDutyNormItem.ProtectionTools = normItem.ProtectionTools;
			newDutyNormItem.Amount = normItem.Amount;
			
			switch(normItem.NormPeriod) {
				case NormPeriodType.Year:
					newDutyNormItem.NormPeriod = DutyNormPeriodType.Year; 
				break;
				case NormPeriodType.Month:
					newDutyNormItem.NormPeriod = DutyNormPeriodType.Month;
					break;
				case NormPeriodType.Wearout:
				case NormPeriodType.Shift:
				case NormPeriodType.Duty:
					newDutyNormItem.NormPeriod = DutyNormPeriodType.Wearout;
				break;
			}
			
			newDutyNormItem.PeriodCount = normItem.PeriodCount;
			newDutyNormItem.NormParagraph = normItem.NormParagraph;
			newDutyNormItem.Comment = normItem.Comment;
			newDutyNormItem.Graph = new IssueGraph();
			
			return newDutyNormItem;
		}
		
		public virtual void СopyEmployeeIssueOperations (DutyNorm newDutyNorm, Norm norm) {
			
		}
	}
}
