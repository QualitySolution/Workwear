using FluentNHibernate.Mapping;
using workwear.Domain;

namespace workwear.HMap
{
	public class NormMap : ClassMap<Norm>
	{
		public NormMap ()
		{
			Table ("norms");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.TONNumber).Column ("ton_number");
			Map (x => x.TONAttachment).Column ("ton_attachment");
			Map (x => x.TONParagraph).Column ("ton_paragraph");

			HasManyToMany (x => x.Professions).Table ("norms_professions")
				.ParentKeyColumn ("norm_id")
				.ChildKeyColumn ("profession_id")
				.LazyLoad ();

			HasMany (x => x.Items).KeyColumn ("norm_id").Not.KeyNullable ().Inverse ().Cascade.AllDeleteOrphan ()
				.LazyLoad ();
		}
	}
}