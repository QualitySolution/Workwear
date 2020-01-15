using FluentNHibernate.Mapping;
using workwear.Domain.Company;

namespace workwear.HibernateMapping.Company
{
	public class SubdivisionMap : ClassMap<Subdivision>
	{
		public SubdivisionMap ()
		{
			Table ("objects");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();

			Map(x => x.Code).Column("code");
			Map (x => x.Name).Column ("name").Not.Nullable ();
			Map (x => x.Address).Column ("address");

			HasMany (x => x.Places)
				.KeyColumn ("object_id").Not.KeyNullable ()
				.Cascade.AllDeleteOrphan ().Inverse ()
				.LazyLoad ();
		}
	}
}