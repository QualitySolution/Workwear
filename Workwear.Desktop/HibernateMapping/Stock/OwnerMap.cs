using FluentNHibernate.Mapping;
using Workwear.Domain.Stock;

namespace Workwear.HibernateMapping.Stock 
{
	public class OwnerMap : ClassMap<Owner>
	{
		public OwnerMap() 
		{
			Table ("owner");

			if(MappingParams.UseIdsForTest)
				Id (x => x.Id).Column("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column("id").GeneratedBy.Native();
			
			Map (x => x.Name).Column("name");
			Map(x => x.Description).Column("description");
		}
	}
}
