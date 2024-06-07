using FluentNHibernate.Mapping;
using Workwear.Domain.Company;
using Workwear.Domain.Stock.Documents;

namespace Workwear.HibernateMapping.Stock.Documents
{
	public class WriteoffMap : ClassMap<Writeoff>
	{
		public WriteoffMap ()
		{
			Table ("stock_write_off");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.DocNumber).Column("doc_number");
			Map (x => x.Date).Column ("date");
			Map(x => x.Comment).Column("comment");
			Map(x => x.CreationDate).Column("creation_date");

			References (x => x.CreatedbyUser).Column ("user_id");
			References (x => x.Director).Column ("director_id");
			References (x => x.Chairman).Column ("chairman_id");
			References (x => x.Organization).Column ("organization_id");
			
			HasManyToMany<Leader>(x => x.Members)
				.Table("stock_write_off_members")
				.ParentKeyColumn("write_off_id")
				.ChildKeyColumn("member_id");

			HasMany (x => x.Items)
				.Inverse()
				.KeyColumn ("stock_write_off_id").Not.KeyNullable ()
				.Cascade.AllDeleteOrphan ()
				.LazyLoad ();
		}
	}
}
