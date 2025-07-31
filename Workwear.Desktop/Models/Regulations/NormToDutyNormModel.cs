using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;

namespace Workwear.Models.Regulations {
	public class NormToDutyNormModel {
		private readonly UnitOfWorkProvider unitOfWorkProvider;

		public NormToDutyNormModel(
			UnitOfWorkProvider unitOfWorkProvider = null) {
			this.unitOfWorkProvider = unitOfWorkProvider;
		}
		
		#region Helpers
		private IUnitOfWork UoW => unitOfWorkProvider?.UoW;
		#endregion

		public virtual void CopyDataFromNorm(DutyNorm newDutyNorm, Norm norm, EmployeeCard employee) {
			newDutyNorm.Name = norm.Name;
			newDutyNorm.DateFrom = norm.DateFrom;
			newDutyNorm.DateTo = norm.DateTo;
			newDutyNorm.ResponsibleEmployee = employee;
			newDutyNorm.Comment = norm.Comment;
			foreach(var item in norm.Items) {
				newDutyNorm.Items.Add(CopyNormItems(newDutyNorm, item));
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
	}
}
