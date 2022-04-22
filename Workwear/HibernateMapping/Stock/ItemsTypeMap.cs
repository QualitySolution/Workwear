using FluentNHibernate.Mapping;
using workwear.Domain.Stock;

namespace workwear.HMap
{
	public class ItemsTypeMap : ClassMap<ItemsType>
	{
		public ItemsTypeMap ()
		{
			Table ("item_types");

			if(workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map (x => x.Name).Column ("name").Not.Nullable ();
			Map(x => x.Category).Column("category");
			Map(x => x.IssueType).Column("issue_type");
			Map (x => x.LifeMonths).Column ("norm_life");
			Map(x => x.Comment).Column("comment");
			Map(x => x.WearCategory).Column("wear_category");

			References (x => x.Units).Column ("units_id");
			References(x => x.SizeType).Column("size_type_id");
			References(x => x.HeightType).Column("height_type_id");

			HasMany(x => x.Nomenclatures).KeyColumn("type_id").Inverse().LazyLoad();

		}
	}
}