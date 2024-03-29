using FluentNHibernate.Mapping;
using Workwear.Domain.Stock.Documents;

namespace Workwear.HibernateMapping.Stock.Documents {
	public class InspectionItemMap: ClassMap<InspectionItem>{

		public InspectionItemMap() {

			Table ("stock_inspection_items");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			Map (x => x.Cause).Column ("cause");

			References (x => x.Document).Column ("stock_inspection_id").Not.Nullable ();
			References(x => x.OperationIssue).Column("operation_issue_id").Not.Nullable();
			References(x => x.NewOperationIssue).Column("new_operation_issue_id").Not.Nullable().Cascade.All();
		}
	}
}
