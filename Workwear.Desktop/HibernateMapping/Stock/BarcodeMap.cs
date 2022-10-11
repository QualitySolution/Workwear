using FluentNHibernate.Mapping;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;

namespace Workwear.HibernateMapping.Stock 
{
	public class BarcodeMap : ClassMap<Barcode>
	{
		public BarcodeMap() 
		{
			Table("barcodes");

			if(MappingParams.UseIdsForTest)
				Id(x => x.Id).Column("id").GeneratedBy.HiLo("0");
			else 
				Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.CreateDate).Column("creation_date");
			Map(x => x.Title).Column("title");
			
			References(x => x.Nomenclature).Column ("nomenclature_id");
			References(x => x.Size).Column ("size_id");
			References(x => x.Height).Column ("height_id");
			
			HasMany<BarcodeOperation>(x => x.BarcodeOperations)
				.KeyColumn("barcode_id").Inverse().LazyLoad();
		}
	}
}
