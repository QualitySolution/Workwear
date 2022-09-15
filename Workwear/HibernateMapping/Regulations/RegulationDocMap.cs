using FluentNHibernate.Mapping;
using Workwear.Domain.Regulations;

namespace workwear.HMap
{
	public class RegulationDocMap : ClassMap<RegulationDoc>
	{
		public RegulationDocMap ()
		{
			Table ("regulations");

			if(workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map (x => x.Name).Column ("name").Not.Nullable ();
			Map (x => x.DocDate).Column ("doc_date");
			Map(x => x.Number).Column("number");

			HasMany(x => x.Annexess).KeyColumn("regulations_id").Not.KeyNullable().Inverse().Cascade.All()
				.LazyLoad();
		}
	}
}
