﻿using FluentNHibernate.Mapping;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;

namespace Workwear.HibernateMapping.Regulations
{
	public class ProtectionToolsMap : ClassMap<ProtectionTools>
	{
		public ProtectionToolsMap()
		{
			Table("protection_tools");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.Name).Column("name").Not.Nullable();
			Map(x => x.Comment).Column("comments");
			Map(x => x.AssessedCost).Column("assessed_cost");
			Map(x => x.SupplyType).Column("supply_type").Not.Nullable();
			Map(x => x.WashingPpe).Column("washing_ppe").Not.Nullable();
			Map(x => x.Dispenser).Column("dispenser").Not.Nullable();

			References(x => x.SupplyNomenclatureUnisex).Column("supply_uni_id").Nullable();
			References(x => x.SupplyNomenclatureMale).Column("supply_male_id").Nullable();
			References(x => x.SupplyNomenclatureFemale).Column("supply_female_id").Nullable();
			References(x => x.CategoryForAnalytic).Column("category_for_analytic_id").Nullable();
			References(x => x.Type).Column("item_types_id");

			HasManyToMany<Nomenclature>(x => x.Nomenclatures)
			.Table("protection_tools_nomenclature")
			.ParentKeyColumn("protection_tools_id")
			.ChildKeyColumn("nomenclature_id");
		}
	}
}
