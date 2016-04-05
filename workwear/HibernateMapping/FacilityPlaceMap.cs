using FluentNHibernate.Mapping;
using workwear.Domain;

namespace workwear.HMap
{
	public class FacilityPlaceMap : ClassMap<FacilityPlace>
	{
		public FacilityPlaceMap ()
		{
			Table ("object_places");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Name).Column ("name").Not.Nullable ();

			References(x => x.Facility).Column("object_id").Not.Nullable ();
		}
	}
}