using FluentNHibernate.Mapping;
using workwear.Domain.Stock;
using workwear.Measurements;

namespace workwear.HMap
{
	public class NomenclatureMap : ClassMap<Nomenclature>
	{
		public NomenclatureMap ()
		{
			Table ("nomenclature");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Name).Column ("name");
			Map (x => x.Sex).Column ("sex").CustomType<ClothesSexType> ();
			Map (x => x.Size).Column ("size");
			Map (x => x.SizeStd).Column ("size_std");
			Map (x => x.WearGrowth).Column ("growth");
			Map (x => x.WearGrowthStd).Column ("growth_std");

			References (x => x.Type).Column ("type_id");
		}
	}
}

