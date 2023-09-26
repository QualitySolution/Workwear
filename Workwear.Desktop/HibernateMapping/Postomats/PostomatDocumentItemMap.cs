using FluentNHibernate.Mapping;
using Workwear.Domain.Postomats;

namespace Workwear.HibernateMapping.Postomats
{
	public class PostomatDocumentItemMap : ClassMap<PostomatDocumentItem>
	{
		public PostomatDocumentItemMap ()
		{
			Table ("stock_expense_detail");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map (x => x.Delta).Column ("delta");
			Map(x => x.LocationStorage).Column("loc_storage");
			Map(x => x.LocationShelf).Column("loc_shelf");
			Map(x => x.LocationCell).Column("loc_cell");

			References (x => x.Document).Column ("document_id").Not.Nullable ();
			References (x => x.Nomenclature).Column ("nomenclature_id").Not.Nullable ();
			References (x => x.Barcode).Column ("barcode_id");
		}
	}
}
