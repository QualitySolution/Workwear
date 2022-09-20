using FluentNHibernate.Mapping;
using Workwear.Domain.Company;

namespace Workwear.HibernateMapping.Company
{
	public class SubdivisionPlaceMap : ClassMap<SubdivisionPlace>
	{
		public SubdivisionPlaceMap ()
		{
			Table ("object_places");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map (x => x.Name).Column ("name").Not.Nullable ();

			References(x => x.Subdivision).Column("object_id").Not.Nullable ();
		}
	}
}
