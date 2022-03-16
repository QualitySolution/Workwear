using FluentNHibernate.Mapping;
using workwear.Domain.Stock;

namespace workwear.HMap
{
	public class CollectiveExpenseItemMap : ClassMap<CollectiveExpenseItem>
	{
		public CollectiveExpenseItemMap()
		{
			Table ("stock_collective_expense_detail");

			if(workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map (x => x.Amount).Column ("quantity");

			References (x => x.Document).Column ("stock_collective_expense_id").Not.Nullable ();
			References (x => x.Employee).Column ("employee_id");
			References(x => x.ProtectionTools).Column("protection_tools_id");
			References(x => x.Nomenclature).Column ("nomenclature_id").Not.Nullable ();
			References(x => x.EmployeeIssueOperation).Column("employee_issue_operation_id").Cascade.All();
			References(x => x.WarehouseOperation).Column("warehouse_operation_id").Not.Nullable().Cascade.All();
			References(x => x.SizeType).Column("size_id");
			References(x => x.Height).Column("height_id");

			HasOne(x => x.IssuanceSheetItem)
				.PropertyRef(x => x.CollectiveExpenseItem);
		}
	}
}