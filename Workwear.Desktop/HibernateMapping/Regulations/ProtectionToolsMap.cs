using System;
using FluentNHibernate.Mapping;
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

			References(x => x.Type).Column("item_types_id");

			HasManyToMany<ProtectionTools>(x => x.Analogs)
			.Table("protection_tools_replacement")
			.ParentKeyColumn("protection_tools_id")
			.ChildKeyColumn("protection_tools_analog_id");

			HasManyToMany<Nomenclature>(x => x.Nomenclatures)
			.Table("protection_tools_nomenclature")
			.ParentKeyColumn("protection_tools_id")
			.ChildKeyColumn("nomenclature_id");
		}
	}
}
