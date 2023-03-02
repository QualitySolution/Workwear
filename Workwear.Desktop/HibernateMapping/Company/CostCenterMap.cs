using FluentNHibernate.Mapping;
using Workwear.Domain.Company;

namespace Workwear.HibernateMapping.Company
{
	public class CostCenterMap : ClassMap<CostCenter>
	{
		public CostCenterMap ()
		{
			Table ("cost_center");

			if(MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();

			Map(x => x.Code).Column("code");
			Map (x => x.Name).Column ("name").Not.Nullable ();
		}
	}
}
