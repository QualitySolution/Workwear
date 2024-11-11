using FluentNHibernate.Mapping;
using Workwear.Domain.Stock;

namespace Workwear.HibernateMapping.Stock {
	public class CauseWriteOffMap: ClassMap<CausesWriteOff> 
	{
		public CauseWriteOffMap() {
			Table("causes_write_off");
			
			if(MappingParams.UseIdsForTest)
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			else 
				Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.Name).Column("name");
			
		}
	}
}
