using FluentNHibernate.Mapping;
using workwear.Domain.Stock;

namespace workwear.HMap
{
	public class IncomeItemMap : ClassMap<IncomeItem>
	{
		public IncomeItemMap ()
		{
			Table ("stock_income_detail");

			if(workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map (x => x.Amount).Column ("quantity");
			Map (x => x.Cost).Column ("cost");
			Map(x => x.Certificate).Column("certificate");

			References(x => x.Document).Column("stock_income_id");
			References (x => x.Nomenclature).Column ("nomenclature_id");
			References(x => x.ReturnFromEmployeeOperation).Column("employee_issue_operation_id").Cascade.All();
			References(x => x.ReturnFromSubdivisionOperation).Column("subdivision_issue_operation_id").Cascade.All();
			References(x => x.WarehouseOperation).Column("warehouse_operation_id").Cascade.All().Not.Nullable();
			References(x => x.SizeType).Column("size_id");
			References(x => x.Height).Column("height_id");
		}
	}
}