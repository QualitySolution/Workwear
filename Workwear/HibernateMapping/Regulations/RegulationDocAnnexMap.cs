using FluentNHibernate.Mapping;
using Workwear.Domain.Regulations;

namespace workwear.HMap
{
	public class RegulationDocAnnexMap : ClassMap<RegulationDocAnnex>
	{
		public RegulationDocAnnexMap ()
		{
			Table ("regulations_annex");

			if(workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map (x => x.Name).Column ("name");
			Map(x => x.Number).Column("number");

			References(x => x.Document).Column("regulations_id");
		}
	}
}
