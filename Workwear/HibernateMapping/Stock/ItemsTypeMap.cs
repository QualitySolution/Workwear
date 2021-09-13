using FluentNHibernate.Mapping;
using workwear.Domain.Stock;
using Workwear.Measurements;

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
			Map (x => x.Category).Column ("category").CustomType<ItemTypeCategoryType> ();
			Map (x => x.WearCategory).Column ("wear_category").CustomType<СlothesTypeType> ();
			Map(x => x.IssueType).Column("issue_type").CustomType<IssueTypeEnumType>();
			Map (x => x.LifeMonths).Column ("norm_life");
			Map(x => x.Comment).Column("comment");

			References (x => x.Units).Column ("units_id");

			HasMany(x => x.Nomenclatures).KeyColumn("type_id").Inverse().LazyLoad();

		}
	}
}