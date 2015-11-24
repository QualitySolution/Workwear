using FluentNHibernate.Mapping;
using workwear.Domain;

namespace workwear.HMap
{
	public class NormItemMap : ClassMap<NormItem>
	{
		public NormItemMap ()
		{
			Table ("norms_item");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Amount).Column ("amount");
			Map (x => x.NormPeriod).Column ("period_type").CustomType<NormPeriodTypeType> ();
			Map (x => x.PeriodCount).Column ("period_count");

			References (x => x.Item).Column ("itemtype_id").Not.Nullable ();
		}
	}
}