using System;
using FluentNHibernate.Mapping;
using workwear.Domain.Stock;

namespace workwear.HibernateMapping.Stock
{
	public class WareHouseMap: ClassMap<Warehouse>
	{
		public WareHouseMap()
		{

			Table("warehouse");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
		
			References(x => x.Subdivision).Column("subdivision_id");

		}
	}
}
