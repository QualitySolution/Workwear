using FluentNHibernate.Mapping;
using Workwear.Domain.Operations;

namespace Workwear.HibernateMapping.Stock
{
	public class BarcodeOperationMap : ClassMap<BarcodeOperation>
	{
		public BarcodeOperationMap()
		{
			Table ("operation_barcodes");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			References(x => x.Barcode).Column("barcode_id").Not.Nullable();
			References(x => x.EmployeeIssueOperation).Column("employee_issue_operation_id");
			References(x => x.WarehouseOperation).Column("warehouse_operation_id");
		}
	}
}
