using FluentNHibernate.Mapping;
using Workwear.Domain.Stock;

namespace Workwear.HibernateMapping.Stock {
	public class CauseIssueMap: ClassMap<CausesIssue> 
	{
		public CauseIssueMap() {
			Table("causes_issue");
			
			if(MappingParams.UseIdsForTest)
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			else 
				Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.Name).Column("name");
			
		}
	}
}
