using FluentNHibernate.Mapping;
using Workwear.Domain.Stock.Barcodes;

namespace Workwear.HibernateMapping.Stock 
{
	public class BarcodeEan13Map : ClassMap<BarcodeEan13>
	{
		public BarcodeEan13Map() 
		{
			Table("barcodes");

			if(MappingParams.UseIdsForTest)
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			else 
				Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.Value).Column("value");
			Map(x => x.Fractional).Column("fractional");
			
			References(x => x.EmployeeIssueOperation).Column ("employee_issue_operation_id");
		}
	}
}
