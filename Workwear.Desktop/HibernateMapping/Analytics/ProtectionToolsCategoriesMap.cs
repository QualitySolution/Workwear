using FluentNHibernate.Mapping;
using Workwear.Domain.Analytics;

namespace Workwear.HibernateMapping.Analytics 
{
	public class ProtectionToolsCategoriesMap : ClassMap<ProtectionToolsCategory> 
	{
		public ProtectionToolsCategoriesMap() 
		{
			Table("protection_tools_category_for_analytics");
			if(MappingParams.UseIdsForTest) 
			{
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			}
			else 
			{
				Id(x => x.Id).Column("id").GeneratedBy.Native();
			}

			Map(x => x.Name).Column("name").Not.Nullable();
			Map(x => x.Comment).Column("comment").Nullable();
			HasMany(x => x.ProtectionTools)
				.KeyColumn("category_for_analytic_id")
				.Inverse()
				.LazyLoad();
		}
	}
}
