using FluentNHibernate.Mapping;
using Workwear.Domain.Stock.Documents;

namespace Workwear.HibernateMapping.Stock.Documents
{
	public class ExpenseDutyNormItemMap : ClassMap<ExpenseDutyNormItem>
	{
		public ExpenseDutyNormItemMap ()
		{
			Table ("stock_expense_duty_norm_items");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map (x => x.Amount).Column ("quantity").Not.Nullable();

			References (x => x.Document).Column ("stock_expense_duty_norm_id").Not.Nullable ();
			References (x => x.Nomenclature).Column ("nomenclature_id");
			References(x => x.Operation).Column("operation_issued_by_duty_norm_id");
			References(x => x.WarehouseOperation).Column("warehouse_operation_id");
			References(x => x.ProtectionTools).Column("protection_tools_id");
			References(x => x.WearSize).Column("size_id");
			References(x => x.Height).Column("height_id");
		}
	}
}
