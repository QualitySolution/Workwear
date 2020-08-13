using FluentNHibernate.Mapping;
using workwear.Domain.Regulations;

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

			References (x => x.Norm).Column ("norm_id").Not.Nullable ();
			References (x => x.ProtectionTools).Column ("protection_tools_id").Not.Nullable ();
		}
	}
}