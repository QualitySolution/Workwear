using FluentNHibernate.Mapping;
using NPOI.SS.Formula.Functions;
using workwear.Domain.Stock;

namespace workwear.HMap
{
	public class ExpenseItemMap : ClassMap<ExpenseItem>
	{
		public ExpenseItemMap ()
		{
			Table ("stock_expense_detail");

			if(workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map (x => x.Amount).Column ("quantity");

			References (x => x.ExpenseDoc).Column ("stock_expense_id").Not.Nullable ();
			References (x => x.Nomenclature).Column ("nomenclature_id").Not.Nullable ();
			References (x => x.SubdivisionPlace).Column ("object_place_id");
			References(x => x.EmployeeIssueOperation).Column("employee_issue_operation_id").Cascade.All();
			References(x => x.SubdivisionIssueOperation).Column("subdivision_issue_operation_id").Cascade.All();
			References(x => x.WarehouseOperation).Column("warehouse_operation_id").Not.Nullable().Cascade.All();
			References(x => x.ProtectionTools).Column("protection_tools_id");
			HasOne(x => x.IssuanceSheetItem)
				.PropertyRef(x => x.ExpenseItem);
			References(x => x.WearSize).Column("size_id");
			References(x => x.Height).Column("height_id");
		}
	}
}