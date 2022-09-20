using FluentNHibernate.Mapping;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;

namespace Workwear.HibernateMapping.Stock
{
	public class NomenclatureMap : ClassMap<Nomenclature>
	{
		public NomenclatureMap ()
		{
			Table ("nomenclature");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map (x => x.Name).Column ("name");
			Map(x => x.Sex).Column("sex");
			Map(x => x.Comment).Column("comment");
			Map(x => x.Number).Column("number");
			Map(x => x.Archival).Column("archival");
			Map(x => x.Rating).Column("rating");
			Map(x => x.RatingCount).Column("rating_count");

			References (x => x.Type).Column ("type_id");

			HasManyToMany<ProtectionTools>(x => x.ProtectionTools)
				.Table("protection_tools_nomenclature")
				.ParentKeyColumn("nomenclature_id")
				.ChildKeyColumn("protection_tools_id");
		}
	}
}

