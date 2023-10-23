using FluentNHibernate.Mapping;
using Workwear.Domain.Company;

namespace Workwear.HibernateMapping.Company {
	public class EmployeeGroupItemMap : ClassMap<EmployeeGroupItem> {
		public EmployeeGroupItemMap() {
			Table("employee_group_items");

			if( MappingParams.UseIdsForTest)
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			else
				Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Comment).Column("comment");
			References (x => x.Employee).Column ("employee_id").Not.Nullable ();
			References (x => x.Group).Column ("employee_group_id").Not.Nullable ();
		}
	}
}
