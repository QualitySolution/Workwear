using FluentNHibernate.Mapping;
using Workwear.Domain.Company;

namespace Workwear.HibernateMapping.Company {
	public class EmployeeGroupMap : ClassMap<EmployeeGroup> {
		public EmployeeGroupMap() {
			Table("employee_groups");

			if(MappingParams.UseIdsForTest)
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			else
				Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.Comment).Column("comment");
			
			HasMany (x => x.Items).KeyColumn ("employee_group_id").Not.KeyNullable ().Inverse ().Cascade.AllDeleteOrphan ()
				.LazyLoad ();
		}
	}
}
