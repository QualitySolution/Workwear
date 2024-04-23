using FluentNHibernate.Mapping;
using Workwear.Domain.Operations;

namespace Workwear.HibernateMapping.Stock 
{
	public class SubstituteFundOperationMap : ClassMap<SubstituteFundOperation> 
	{
		public SubstituteFundOperationMap() 
		{
			Table("operation_substitute_fund");
			if (MappingParams.UseIdsForTest) 
			{
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			}
			else 
			{
				Id(x => x.Id).Column("id").GeneratedBy.Native();
			}

			Map(x => x.OperationTime).Column("operation_time").Not.Nullable();
			References(x => x.EmployeeIssueOperation).Column("operation_issued_by_employee_id").Not.Nullable();
			References(x => x.SubstituteBarcode).Column("substitute_barcode_id").Not.Nullable();
			References(x => x.WarehouseOperation).Column("warehouse_operation_id").Not.Nullable();
			References(x => x.WriteOffSubstituteFundOperation).Column("operation_write_off_id").Nullable();
		}
	}
}
