using FluentNHibernate.Mapping;
using Workwear.Domain.Stock.Documents;

namespace Workwear.HibernateMapping.Stock.Documents {
	public class BarcodingItemMap: ClassMap<BarcodingItem>{

		public BarcodingItemMap() {
			Table ("stock_barcoding_items");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			References (x => x.Document).Column ("stock_barcoding_id").Not.Nullable();
			References(x => x.OperationExpence).Column("operation_expence_id").Cascade.All().Not.Nullable();
			References(x => x.OperationReceipt).Column("operation_receipt_id").Cascade.All().Not.Nullable();

			
			
////1289			
//сейчас получается во вьюмодели документа			
//+ каскадное удаление
			 /*HasMany(x => x.Barcodes)
				.Inverse()
				
				.KeyColumn ("stock_barcoding_id").Not.KeyNullable ().
				.Cascade.AllDeleteOrphan ()
				.LazyLoad ();
				*/
		}
	}
}
