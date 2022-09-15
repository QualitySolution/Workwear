using FluentNHibernate.Mapping;
using Workwear.Domain.Stock.Documents;

namespace Workwear.HibernateMapping.Stock.Documents
{
	public class TransferItemMap : ClassMap<TransferItem>
	{
		public TransferItemMap()
		{
			Table("stock_transfer_detail");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.Amount).Column("quantity");

			References(x => x.Document).Column("stock_transfer_id").Not.Nullable();
			References(x => x.Nomenclature).Column("nomenclature_id").Not.Nullable();
			References(x => x.WarehouseOperation).Column("warehouse_operation_id").Not.Nullable().Cascade.All();
		}
	}
}
