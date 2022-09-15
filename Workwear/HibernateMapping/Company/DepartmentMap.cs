using FluentNHibernate.Mapping;
using Workwear.Domain.Company;

namespace workwear.HibernateMapping.Company
{
	public class DepartmentMap : ClassMap<Department>
	{
		public DepartmentMap()
		{
			Table("departments");

			if(workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.Name).Column("name");
			Map(x => x.Comments).Column("comments");

			References(x => x.Subdivision).Column("subdivision_id");
		}
	}
}
