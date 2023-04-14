using FluentNHibernate.Mapping;
using Workwear.Domain.Stock.Documents;

namespace Workwear.HibernateMapping.Stock.Documents {
	public class InspectionMemberMap : ClassMap<InspectionMember>{
		public InspectionMemberMap () {
			Table ("stock_inspection_members");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();

			References (x => x.Document).Column ("stock_inspection_id").Not.Nullable ();
			References(x => x.Member).Column("member_id").Not.Nullable();
		}
	}
}
