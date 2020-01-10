using FluentNHibernate.Mapping;
using workwear.Domain.Regulations;
using workwear.Domain.Statements;

namespace workwear.HibernateMapping.Statements
{
	public class IssuanceSheetItemMap : ClassMap<IssuanceSheetItem>
	{
		public IssuanceSheetItemMap()
		{
			Table("issuance_sheet_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Amount).Column("amount");
			Map(x => x.StartOfUse).Column("start_of_use");
			Map(x => x.PeriodCount).Column("period_count");
			Map(x => x.PeriodType).Column("period_type").CustomType<NormPeriodTypeType>();

			References(x => x.IssuanceSheet).Column("issuance_sheet_id");
			References(x => x.Employee).Column("employee_id");
			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.IssueOperation).Column("issued_operation_id");
		}
	}
}
