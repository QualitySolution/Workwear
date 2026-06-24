using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;

namespace Workwear.Models.Analytics
{
	/// <summary>
	/// Будущая выдача сотруднику
	/// </summary>
	public class FutureIssueEmployee : FutureIssue {
		public EmployeeCardItem EmployeeCardItem { get; set; }

		public override EmployeeCard Employee => EmployeeCardItem.EmployeeCard;
		public override Subdivision Subdivision => Employee.Subdivision;
		public override ProtectionTools ProtectionTools => EmployeeCardItem.ProtectionTools;
		public NormItem NormItem => EmployeeCardItem.ActiveNormItem;
		public Norm Norm => NormItem.Norm;

		public override Size Size =>
			Employee.Sizes.FirstOrDefault(s => DomainHelper.EqualDomainObjects(s.SizeType, ProtectionTools.Type.SizeType))?.Size;
		public override Size Height =>
			Employee.Sizes.FirstOrDefault(s => DomainHelper.EqualDomainObjects(s.SizeType, ProtectionTools.Type.HeightType))?.Size;

		public override string NormAmountText => NormItem?.AmountText ?? "";
		public override string NormLifeText => NormItem?.LifeText ?? "";
		public override string NormConditionName => NormItem?.NormCondition?.Name ?? "";
		public override int? NormId => Norm?.Id;
		public override string IssueTypeTitle => ItemsType?.IssueType.GetEnumTitle() ?? "";
	}
}
