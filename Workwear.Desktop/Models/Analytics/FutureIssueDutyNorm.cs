using Workwear.Domain.Company;
using Workwear.Domain.Regulations;

namespace Workwear.Models.Analytics
{
	/// <summary>
	/// Будущая выдача по дежурной норме
	/// </summary>
	public class FutureIssueDutyNorm : FutureIssue {
		public DutyNormItem DutyNormItem { get; set; }

		public DutyNorm DutyNorm => DutyNormItem.DutyNorm;
		public override ProtectionTools ProtectionTools => DutyNormItem.ProtectionTools;
		public override Subdivision Subdivision => DutyNorm.Subdivision;
		public override EmployeeCard Employee => DutyNorm.ResponsibleEmployee;
		/// <summary>Для дежурных норм пол не отображается.</summary>
		public override Sex? EmployeeSex => null;

		public override string NormAmountText => DutyNormItem == null ? "" :
			$"{DutyNormItem.Amount} {DutyNormItem.AmountUnitText(DutyNormItem.Amount)}".Trim();
		
		public override string NormLifeText => DutyNormItem?.LifeText ?? "";
		public override string NormConditionName => "";
		public override int? NormId => DutyNorm?.Id;
		public override string IssueTypeTitle => "Дежурное";
	}
}
