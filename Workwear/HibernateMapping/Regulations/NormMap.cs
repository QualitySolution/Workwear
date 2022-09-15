using FluentNHibernate.Mapping;
using Workwear.Domain.Regulations;

namespace workwear.HMap
{
	public class NormMap : ClassMap<Norm>
	{
		public NormMap ()
		{
			Table ("norms");

			if(workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.Name).Column("name");
			Map(x => x.TONParagraph).Column("ton_paragraph");
			Map(x => x.Comment).Column("comment");
			Map(x => x.DateFrom).Column("datefrom");
			Map(x => x.DateTo).Column("dateto");

			References(x => x.Document).Column("regulations_id");
			References(x => x.Annex).Column("regulations_annex_id");

			HasManyToMany (x => x.Posts).Table ("norms_professions")
				.ParentKeyColumn ("norm_id")
				.ChildKeyColumn ("profession_id")
				.LazyLoad ();

			HasMany (x => x.Items).KeyColumn ("norm_id").Not.KeyNullable ().Inverse ().Cascade.AllDeleteOrphan ()
				.LazyLoad ();
		}
	}
}
