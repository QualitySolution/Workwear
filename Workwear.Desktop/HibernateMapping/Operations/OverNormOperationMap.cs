using FluentNHibernate.Mapping;
using Workwear.Domain.Operations;

namespace Workwear.HibernateMapping.Stock 
{
	public class OverNormOperationMap : ClassMap<OverNormOperation> 
	{
		public OverNormOperationMap() 
		{
			Table("operation_over_norm");
			if (MappingParams.UseIdsForTest) 
			{
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			}
			else 
			{
				Id(x => x.Id).Column("id").GeneratedBy.Native();
			}

			Map(x => x.OperationTime).Column("operation_time").Not.Nullable();
			Map(x => x.LastUpdate).Column("last_update").Not.Nullable();
			Map(x => x.Type).Column("type").Not.Nullable();

			References(x => x.Employee).Column("employee_id").Not.Nullable();
			References(x => x.WarehouseOperation).Column("operation_warehouse_id").Not.Nullable().Cascade.All();
			References(x => x.EmployeeIssueOperation).Column("operation_issued_by_employee_id").Nullable();
			References(x => x.WriteOffOverNormOperation).Column("operation_write_off_id").Nullable().Cascade.All();
			
			HasMany(x => x.BarcodeOperations)
				.KeyColumn("over_norm_id").Inverse()
				.Not.KeyNullable()
				.Cascade.AllDeleteOrphan()
				.LazyLoad();
		}
	}
}
