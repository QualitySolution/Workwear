using FluentNHibernate.Mapping;
using Workwear.Domain.Stock.Documents;

namespace Workwear.HibernateMapping.Stock.Documents
{
	public class ReturnItemMap : ClassMap<ReturnItem>
	{
		public ReturnItemMap ()
		{
			Table ("stock_return_items");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map (x => x.Amount).Column ("quantity");
			Map (x => x.СommentReturn).Column ("comment_return");

			References(x => x.Document).Column("stock_return_id");
			References (x => x.Nomenclature).Column ("nomenclature_id");
			References(x => x.ReturnFromEmployeeOperation).Column("employee_issue_operation_id").Cascade.All();
			References(x => x.WarehouseOperation).Column("warehouse_operation_id").Cascade.All().Not.Nullable();
			References(x => x.WearSize).Column("size_id");
			References(x => x.Height).Column("height_id");
			References(x => x.EmployeeCard).Column("employee_id");
			References(x => x.DutyNorm).Column("duty_norm_id");
			References(x=>x.ReturnFromDutyNormOperation).Column("duty_norm_issue_operation_id").Cascade.All();
			References(x => x.ServiceClaim).Column("claim_id");
		}
	}
}
