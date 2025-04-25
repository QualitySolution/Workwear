using FluentNHibernate.Mapping;
using Workwear.Domain.Supply;

namespace Workwear.HibernateMapping.Supply {
	public class ShipmentMap : ClassMap<Shipment> {
		public ShipmentMap() {
			Table("shipment");

			if(MappingParams.UseIdsForTest)
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			else
				Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.StartPeriod).Column("start_period");
			Map(x => x.EndPeriod).Column("end_period");
			Map(x => x.Submitted).Column("submitted");
			Map(x => x.Status).Column("status");
			Map(x => x.FullOrdered).Column("full_ordered");
			Map(x => x.FullReceived).Column("full_received");
			Map(x => x.HasReceive).Column("has_receive");
			Map(x => x.Comment).Column("comment");
			Map(x => x.CreationDate).Column("creation_date");

			References(x => x.CreatedbyUser).Column("user_id");

			HasMany(x => x.Items)
				.Inverse()
				.KeyColumn("shipment_id").Not.KeyNullable()
				.Cascade.AllDeleteOrphan()
				.LazyLoad();
			HasMany (x => x.Incomes)
				.Inverse()
				.KeyColumn ("shipment_id").Not.KeyNullable()
				.LazyLoad();
		}
	}
}
