using FluentNHibernate.Mapping;
using Workwear.Domain.Stock.Documents;

namespace Workwear.HibernateMapping.Stock.Documents
{
    public class ComplectionResultItemMap : ClassMap<CompletionResultItem>
    {
        public ComplectionResultItemMap()
        {
            Table ("stock_completion_result_item");
            if(MappingParams.UseIdsForTest)
                Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
            else 
                Id (x => x.Id).Column ("id").GeneratedBy.Native();
            
            References (x => x.Completion).Column ("stock_completion_id");
            References(x => x.WarehouseOperation).Column("warehouse_operation_id")
                .Not.Nullable().Cascade.All();
        }
    }
}
