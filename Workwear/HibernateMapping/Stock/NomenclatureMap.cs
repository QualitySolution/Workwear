using FluentNHibernate.Mapping;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using Workwear.Measurements;

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
			Map (x => x.SizeStd).Column ("size_std");
			Map(x => x.Comment).Column("comment");
			Map(x => x.Number).Column("number");

			References (x => x.Type).Column ("type_id");

			HasManyToMany<ProtectionTools>(x => x.ProtectionTools)
				.Table("protection_tools_nomenclature")
				.ParentKeyColumn("nomenclature_id")
				.ChildKeyColumn("protection_tools_id").Cascade.All();
		}
	}
}

