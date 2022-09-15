using FluentNHibernate.Mapping;
using Workwear.Domain.Company;

namespace Workwear.HibernateMapping.Company
{
	public class SubdivisionMap : ClassMap<Subdivision>
	{
		public SubdivisionMap ()
		{
			Table ("objects");

			if(MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();

			Map(x => x.Code).Column("code");
			Map (x => x.Name).Column ("name").Not.Nullable ();
			Map (x => x.Address).Column ("address");

			References(x => x.Warehouse).Column("warehouse_id");
			References(x => x.ParentSubdivision).Column("parent_object_id");

			HasMany (x => x.Places)
				.KeyColumn ("object_id").Not.KeyNullable ()
				.Cascade.AllDeleteOrphan ().Inverse ()
				.LazyLoad ();
			
			HasMany (x => x.ChildSubdivisions)
				.Inverse()
				.KeyColumn ("parent_object_id").Not.KeyNullable()
				.LazyLoad();
		}
	}
}
