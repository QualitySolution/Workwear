using FluentNHibernate.Mapping;
using Workwear.Domain.Stock;

namespace Workwear.HibernateMapping.Stock {
	public class NomenclatureSizesMap : ClassMap<NomenclatureSizes>{
		public NomenclatureSizesMap ()
		{
			Table ("nomenclature_sizes");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			References (x => x.Nomenclature).Column ("nomenclature_id").Not.Nullable ();
			References(x => x.WearSize).Column("size_id");
			References(x => x.Height).Column("height_id");
			
			Map(x => x.Comment).Column("comment");
		}
	}
}
