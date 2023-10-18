using FluentNHibernate.Mapping;
using Workwear.Domain.Company;

namespace Workwear.HibernateMapping.Company {
	public class EmployeeGroupItemMap : ClassMap<EmployeeGroupItem> {
		public EmployeeGroupItemMap() {
			Table("wear_card_group_items");

			if( MappingParams.UseIdsForTest)
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			else
				Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Comment).Column("comment");
			References (x => x.Employee).Column ("wear_card_id").Not.Nullable ();
			References (x => x.Group).Column ("wear_card_group_id").Not.Nullable ();
		}
	}
}
