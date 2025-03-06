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
			Map(x => x.Comment).Column("comment");
			Map(x => x.CreationDate).Column("creation_date");

			References(x => x.CreatedbyUser).Column("user_id");
			References(x=>x.Status).Column("status_id");

			HasMany(x => x.Items)
				.Inverse()
				.KeyColumn("shipment_id").Not.KeyNullable()
				.Cascade.AllDeleteOrphan()
				.LazyLoad();

		}
	}
}
