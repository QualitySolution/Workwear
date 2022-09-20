using FluentNHibernate.Mapping;
using Workwear.Domain.Stock.Documents;

namespace Workwear.HibernateMapping.Stock.Documents
{
	public class TransferMap : ClassMap<Transfer>
	{
		public TransferMap()
		{
			Table("stock_transfer");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.Date).Column("date");
			Map(x => x.Comment).Column("comment");
			Map(x => x.CreationDate).Column("creation_date");

			References(x => x.WarehouseFrom).Column("warehouse_from_id").Not.Nullable();
			References(x => x.WarehouseTo).Column("warehouse_to_id").Not.Nullable();
			References(x => x.CreatedbyUser).Column("user_id");

			HasMany(x => x.Items)
				.Inverse()
				.KeyColumn("stock_transfer_id").Not.KeyNullable()
				.Cascade.AllDeleteOrphan()
				.LazyLoad();
		}
	}
}
