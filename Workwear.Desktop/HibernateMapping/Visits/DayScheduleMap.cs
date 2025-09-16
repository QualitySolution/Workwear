using FluentNHibernate.Mapping;
using Workwear.Domain.Visits;

namespace Workwear.HibernateMapping.Visits {
	public class DayScheduleMap: ClassMap<DaySchedule> {
		public DayScheduleMap() {
			Table("days_schedule");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			else
				Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.Date).Column("date");
			Map(x => x.StartString).Column("start");
			Map(x => x.EndString).Column("end");
			Map(x => x.DayOfWeek).Column("day_of_week");
			Map(x => x.Interval).Column("visit_interval");
		}
	}
}
