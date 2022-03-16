using FluentNHibernate.Mapping;
using workwear.Domain.Company;

namespace workwear.HibernateMapping.Company
{
	public class EmployeeVacationMap : ClassMap<EmployeeVacation>
	{
		public EmployeeVacationMap()
		{
			Table("wear_cards_vacations");

			if(workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.BeginDate).Column("begin_date").Not.Nullable();
			Map(x => x.EndDate).Column("end_date").Not.Nullable();
			Map(x => x.Comments).Column("comment");

			References(x => x.Employee).Column("wear_card_id");
			References(x => x.VacationType).Column("vacation_type_id");
		}
	}
}
