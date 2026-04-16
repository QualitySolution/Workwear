using FluentNHibernate.Mapping;
using Workwear.Domain.Stock.Documents;

namespace Workwear.HibernateMapping.Stock.Documents 
{
	public class OverNormItemMap : ClassMap<OverNormItem> 
	{
		public OverNormItemMap() 
		{
			Table("stock_over_norm_items");
			if (MappingParams.UseIdsForTest) 
			{
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			}
			else 
			{
				Id(x => x.Id).Column("id").GeneratedBy.Native();
			}
			References(x => x.Document).Column("document_id").Not.Nullable();
			References(x => x.OverNormOperation).Column("over_norm_operation_id").Not.Nullable().Cascade.All();
		}
	}
}
