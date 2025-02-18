using FluentNHibernate.Mapping;
using Workwear.Domain.Stock.Documents;

namespace Workwear.HibernateMapping.Stock.Documents {
	public class ProcurementMap: ClassMap<Procurement> {
		public ProcurementMap() {
			Table("stock_procurement");
			
			if(MappingParams.UseIdsForTest)
				Id(x=>x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id(x=>x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x=>x.StartPeriod).Column("start_period");
			Map(x=>x.EndPeriod).Column("end_period");
			Map(x=>x.Comment).Column("comment");
			Map(x=>x.CreationDate).Column("creation_date");
			
			References(x=>x.CreatedbyUser).Column("user_id");

			HasMany(x => x.Items)
				.Inverse()
				.KeyColumn("stock_procurement_id").Not.KeyNullable()
				.Cascade.AllDeleteOrphan()
				.LazyLoad();

		}
	}
}
