using FluentNHibernate.Mapping;
using workwear.Domain;

namespace workwear.HMap
{
	public class LeaderMap : ClassMap<Leader>
	{
		public LeaderMap ()
		{
			Table ("leaders");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Name).Column ("name");
		}
	}
}