using FluentNHibernate.Mapping;
using workwear.Domain;

namespace workwear.HMap
{
	public class MeasurementUnitsMap : ClassMap<MeasurementUnits>
	{
		public MeasurementUnitsMap ()
		{
			Table ("units");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Name).Column ("name").Not.Nullable ();
		}
	}
}