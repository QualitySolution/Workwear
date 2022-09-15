using FluentNHibernate.Mapping;
using Workwear.Domain.Company;

namespace Workwear.HibernateMapping.Company
{
	public class LeaderMap : ClassMap<Leader>
	{
		public LeaderMap ()
		{
			Table ("leaders");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map (x => x.Surname).Column ("surname");
			Map(x => x.Name).Column("name");
			Map(x => x.Patronymic).Column("patronymic");
			Map(x => x.Position).Column("position");

			References(x => x.Employee).Column("employee_id");
		}
	}
}
