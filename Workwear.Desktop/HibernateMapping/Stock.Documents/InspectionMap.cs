using FluentNHibernate.Mapping;
using Workwear.Domain.Company;
using Workwear.Domain.Stock.Documents;

namespace Workwear.HibernateMapping.Stock.Documents {
	public class InspectionMap: ClassMap<Inspection>{
		public InspectionMap() {
			Table("stock_inspection");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map (x => x.Date).Column ("date");
			Map(x => x.Comment).Column("comment");
			Map(x => x.CreationDate).Column("creation_date");
			
			References (x => x.CreatedbyUser).Column ("user_id");
			References (x => x.Director).Column ("director_id");
			References (x => x.Chairman).Column ("chairman_id");
			References (x => x.Organization).Column ("organization_id");
			
			HasManyToMany<Leader>(x => x.Members)
				.Table("stock_inspection_members")
				.ParentKeyColumn("stock_inspection_id")
				.ChildKeyColumn("member_id");
			
			//HasMany (x => x.Members)
			//	.Inverse()
			//	.KeyColumn ("member_id").Not.KeyNullable ()
			//	.Cascade.AllDeleteOrphan ()
			//	.LazyLoad ();

			HasMany (x => x.Items)
				.Inverse()
				.KeyColumn ("stock_inspection_id").Not.KeyNullable ()
				.Cascade.AllDeleteOrphan ()
				.LazyLoad ();
		}
	}
}
