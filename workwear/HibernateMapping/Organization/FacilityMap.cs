using FluentNHibernate.Mapping;
using workwear.Domain.Organization;

namespace workwear.HMap
{
	public class FacilityMap : ClassMap<Facility>
	{
		public FacilityMap ()
		{
			Table ("objects");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Name).Column ("name").Not.Nullable ();
			Map (x => x.Address).Column ("address");

			HasMany (x => x.Places)
				.KeyColumn ("object_id").Not.KeyNullable ()
				.Cascade.AllDeleteOrphan ().Inverse ()
				.LazyLoad ();
		}
	}
}