using System;
using FluentNHibernate.Mapping;
using workwear.Domain.Stock;

namespace workwear.HibernateMapping.Stock
{
	public class TransferMap : ClassMap<Transfer>
	{
		public TransferMap()
		{
			Table("stock_transfer");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Date).Column("date");
			Map(x => x.Comment).Column("comment");

			References(x => x.WarehouseFrom).Column("warehouse_from_id");
			References(x => x.WarehouseTo).Column("warehouse_to_id");
			References(x => x.CreatedbyUser).Column("user_id");

			HasMany(x => x.Items)
				.Inverse()
				.KeyColumn("stock_transfer_id").Not.KeyNullable()
				.Cascade.AllDeleteOrphan()
				.LazyLoad();
		}
	}
}
