using FluentNHibernate.Mapping;
using workwear.Domain.Company;

namespace workwear.HMap
{
	public class PostMap : ClassMap<Post>
	{
		public PostMap ()
		{
			Table ("posts");

			if(workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map (x => x.Name).Column ("name").Not.Nullable ();
			Map(x => x.Comments).Column("comments");

			References(x => x.Subdivision).Column("subdivision_id");
			References(x => x.Department).Column("department_id");
			References(x => x.Profession).Column("profession_id");
		}
	}
}