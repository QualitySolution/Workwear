using FluentNHibernate.Mapping;
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
			Map(x => x.Lifetime).Column("lifetime");
			Map(x => x.Size).Column("size");
			Map(x => x.WearGrowth).Column("growth");

			References(x => x.IssuanceSheet).Column("issuance_sheet_id").Not.Nullable();
			References(x => x.Employee).Column("employee_id").Not.Nullable();
			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.ItemsType).Column("itemtype_id");
			References(x => x.IssueOperation).Column("issued_operation_id");
			References(x => x.ExpenseItem).Column("stock_expense_detail_id");
		}
	}
}
