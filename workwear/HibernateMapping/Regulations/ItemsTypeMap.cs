using FluentNHibernate.Mapping;
using workwear.Domain.Regulations;
using workwear.Measurements;

namespace workwear.HMap
{
	public class ItemsTypeMap : ClassMap<ItemsType>
	{
		public ItemsTypeMap ()
		{
			Table ("item_types");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Name).Column ("name").Not.Nullable ();
			Map (x => x.Category).Column ("category").CustomType<ItemTypeCategoryType> ();
			Map (x => x.WearCategory).Column ("wear_category").CustomType<СlothesTypeType> ();
			Map (x => x.LifeMonths).Column ("norm_life");
			Map(x => x.Comment).Column("comment");

			References (x => x.Units).Column ("units_id");
		}
	}
}