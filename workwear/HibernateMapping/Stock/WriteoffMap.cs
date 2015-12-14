using FluentNHibernate.Mapping;
using workwear.Domain.Stock;

namespace workwear.HMap
{
	public class WriteoffMap : ClassMap<Writeoff>
	{
		public WriteoffMap ()
		{
			Table ("stock_write_off");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Date).Column ("date");

			References (x => x.CreatedbyUser).Column ("user_id");

			HasMany (x => x.Items)
				.KeyColumn ("stock_write_off_id").Not.KeyNullable ()
				.Cascade.AllDeleteOrphan ()
				.LazyLoad ();
		}
	}
}