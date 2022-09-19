using FluentNHibernate.Mapping;
using Workwear.Domain.Stock.Documents;

namespace Workwear.HibernateMapping.Stock.Documents
{
    public class CompletionMap: ClassMap<Completion>
    {
        public CompletionMap()
        {
            Table ("stock_completion");
            if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
                Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
            else 
                Id (x => x.Id).Column ("id").GeneratedBy.Native();
            
            Map(x => x.Date).Column ("date");
            Map(x => x.Comment).Column("comment");
            Map(x => x.CreationDate).Column("creation_date");

            References (x => x.ResultWarehouse).Column ("warehouse_receipt_id");
            References (x => x.SourceWarehouse).Column ("warehouse_expense_id");
            References (x => x.CreatedbyUser).Column ("user_id");

            HasMany (x => x.ResultItems)
                .Inverse()
                .KeyColumn ("stock_completion_id").Not.KeyNullable()
                .Cascade.AllDeleteOrphan ()
                .LazyLoad ();
            HasMany (x => x.SourceItems)
                .Inverse()
                .KeyColumn ("stock_completion_id").Not.KeyNullable ()
                .Cascade.AllDeleteOrphan ()
                .LazyLoad ();
        }
    }
}
