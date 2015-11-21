using FluentNHibernate.Mapping;
using workwear.Domain;

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
		}
	}
}