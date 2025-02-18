using FluentNHibernate.Mapping;
using Workwear.Domain.Stock.Documents;

namespace Workwear.HibernateMapping.Stock.Documents {
	public class ProcurementItemMap:ClassMap<ProcurementItem> {
		public ProcurementItemMap() {
			Table("stock_procurement_items");
			
			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x=>x.Amount).Column ("quantity");
			Map(x => x.Cost).Column ("cost");
			Map(x=>x.Comment).Column("comment");
			
			References(x=>x.Procurement).Column("stock_procurement_id");
			References(x=>x.Nomenclature).Column("nomenclature_id");
			References(x=>x.WearSize).Column("size_id");
			References(x=>x.Height).Column("height_id");
		}
	}
}
