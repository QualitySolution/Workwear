using FluentNHibernate.Mapping;
using workwear.Domain.Regulations;

namespace workwear.HMap
{
	public class RegulationDocMap : ClassMap<RegulationDoc>
	{
		public RegulationDocMap ()
		{
			Table ("regulations");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Name).Column ("name").Not.Nullable ();
			Map (x => x.DocDate).Column ("doc_date");
			Map(x => x.Number).Column("number");

			HasMany(x => x.Annexess).KeyColumn("regulations_id").Not.KeyNullable().Inverse().Cascade.AllDeleteOrphan()
				.LazyLoad();
		}
	}
}