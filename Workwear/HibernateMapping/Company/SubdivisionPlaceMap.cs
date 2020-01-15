using FluentNHibernate.Mapping;
using workwear.Domain.Company;

namespace workwear.HibernateMapping.Company
{
	public class SubdivisionPlaceMap : ClassMap<SubdivisionPlace>
	{
		public SubdivisionPlaceMap ()
		{
			Table ("object_places");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Name).Column ("name").Not.Nullable ();

			References(x => x.Subdivision).Column("object_id").Not.Nullable ();
		}
	}
}