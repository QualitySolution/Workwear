using FluentNHibernate.Mapping;
using Workwear.Domain.Company;

namespace Workwear.HibernateMapping.Company {
	public class WorkDayMap : ClassMap<WorkDay>{
		public WorkDayMap ()
		{
			Table ("work_days");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map (x => x.Date).Column ("date");
            Map (x => x.IsWorkday).Column ("is_work_day");
			Map (x => x.Comment).Column ("comment");
		}
	}
}
