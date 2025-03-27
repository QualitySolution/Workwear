using FluentNHibernate.Mapping;
using QS.Project.Domain;
using Workwear.Domain.Stock.Documents;

namespace Workwear.HibernateMapping.Stock.Documents 
{
	public class OverNormMap : ClassMap<OverNorm> 
	{
		public OverNormMap() 
		{
			Table("stock_over_norms");
			if (MappingParams.UseIdsForTest) 
			{
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			}
			else 
			{
				Id(x => x.Id).Column("id").GeneratedBy.Native();
			}

			Map(x => x.DocNumber).Column("doc_number").Nullable();
			Map(x => x.Date).Column("date").Not.Nullable();
			Map(x => x.CreationDate).Column("creation_date").Not.Nullable();
			Map(x => x.Type).Column("type").Not.Nullable();
			Map(x => x.Comment).Column("comment").Nullable();
			References(x => x.CreatedbyUser).Column("user_id").Nullable();
			References(x => x.Warehouse).Column("warehouse_id").Not.Nullable();
			
			HasMany (x => x.Items)
				.Inverse()
				.KeyColumn ("document_id").Not.KeyNullable ()
				.Cascade.AllDeleteOrphan ()
				.LazyLoad ();
		}
	}
}
