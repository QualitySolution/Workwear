using FluentNHibernate.Mapping;
using workwear.Domain.Regulations;

namespace workwear.HMap
{
	public class NormMap : ClassMap<Norm>
	{
		public NormMap ()
		{
			Table ("norms");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map(x => x.Name).Column("name");
			Map(x => x.TONParagraph).Column("ton_paragraph");
			Map(x => x.Comment).Column("comment");
			Map(x => x.DateFrom).Column("datefrom");
			Map(x => x.DateTo).Column("dateto");
			#region NLMK
			Map(x => x.NlmkNormId).Column("nlmk_norm_id");
			Map(x => x.NlmkProffId).Column("nlmk_proff_id");
			#endregion

			References(x => x.Document).Column("regulations_id");
			References(x => x.Annex).Column("regulations_annex_id");

			HasManyToMany (x => x.Professions).Table ("norms_professions")
				.ParentKeyColumn ("norm_id")
				.ChildKeyColumn ("profession_id")
				.LazyLoad ();

			HasMany (x => x.Items).KeyColumn ("norm_id").Not.KeyNullable ().Inverse ().Cascade.AllDeleteOrphan ()
				.LazyLoad ();
		}
	}
}