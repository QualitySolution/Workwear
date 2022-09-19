using System;
using FluentNHibernate.Mapping;
using Workwear.Domain.Stock;

namespace Workwear.HibernateMapping.Stock
{
	public class WareHouseMap: ClassMap<Warehouse>
	{
		public WareHouseMap()
		{

			Table("warehouse");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.Name).Column("name");


		}
	}
}
