using FluentNHibernate.Mapping;
using workwear.Domain.Regulations;

namespace workwear.HMap
{
	public class RegulationDocAnnexMap : ClassMap<RegulationDocAnnex>
	{
		public RegulationDocAnnexMap ()
		{
			Table ("regulations_annex");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Name).Column ("name").Not.Nullable ();
			Map(x => x.Number).Column("number");

			References(x => x.Document).Column("regulations_id");
		}
	}
}