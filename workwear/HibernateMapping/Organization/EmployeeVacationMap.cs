using System;
using FluentNHibernate.Mapping;
using workwear.Domain.Organization;

namespace workwear.HibernateMapping.Organization
{
	public class EmployeeVacationMap : ClassMap<EmployeeVacation>
	{
		public EmployeeVacationMap()
		{
			Table("wear_cards_vacations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.BeginDate).Column("begin_date").Not.Nullable();
			Map(x => x.EndDate).Column("end_date").Not.Nullable();
			Map(x => x.Comments).Column("comment");

			References(x => x.Employee).Column("wear_card_id");
			References(x => x.VacationType).Column("vacation_type_id");
		}
	}
}
