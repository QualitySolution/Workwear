using FluentNHibernate.Mapping;
using Workwear.Domain.Regulations;

namespace workwear.HibernateMapping.Regulations
{
	public class NormConditionMap : ClassMap<NormCondition>
	{
		public NormConditionMap()
		{
			Table("norm_conditions");

			if(workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			else
				Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.SexNormCondition).Column("sex");
			Map(x => x.IssuanceStart).Column("issuance_start");
			Map(x => x.IssuanceEnd).Column("issuance_end");
		}
	}
}
