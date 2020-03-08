using FluentNHibernate.Mapping;
using workwear.Domain.Stock;

namespace workwear.HMap
{
	public class ExpenseItemMap : ClassMap<ExpenseItem>
	{
		public ExpenseItemMap ()
		{
			Table ("stock_expense_detail");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Amount).Column ("quantity");
			Map (x => x.AutoWriteoffDate).Column ("auto_writeoff_date");
			Map(x => x.Size).Column("size");
			Map(x => x.WearGrowth).Column("growth");

			References (x => x.ExpenseDoc).Column ("stock_expense_id").Not.Nullable ();
			References (x => x.Nomenclature).Column ("nomenclature_id").Not.Nullable ();
			References (x => x.SubdivisionPlace).Column ("object_place_id");
			References(x => x.EmployeeIssueOperation).Column("employee_issue_operation_id").Cascade.All();
			References(x => x.WarehouseOperation).Column("warehouse_operation_id").Not.Nullable();

			HasOne(x => x.IssuanceSheetItem)
				.Cascade.All()
				.PropertyRef(x => x.ExpenseItem);
		}
	}
}