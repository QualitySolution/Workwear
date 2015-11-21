using FluentNHibernate.Mapping;
using workwear.Domain;

namespace workwear.HMap
{
	public class PostMap : ClassMap<Post>
	{
		public PostMap ()
		{
			Table ("posts");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Name).Column ("name").Not.Nullable ();
		}
	}
}