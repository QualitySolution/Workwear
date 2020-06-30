using System;
using FluentNHibernate.Mapping;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;

namespace workwear.HibernateMapping.Regulations
{
	public class ProtectionToolsMap : ClassMap<ProtectionTools>
	{
		public ProtectionToolsMap()
		{
			Table("protection_tools");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name").Not.Nullable();
			Map(x => x.Comment).Column("comments");

			References(x => x.Units).Column("units_id");

			HasManyToMany<ProtectionTools>(x => x.Analogs)
			.Table("protection_tools_replacement")
			.ParentKeyColumn("protection_tools_id")
			.ChildKeyColumn("protection_tools_analog_id").Cascade.All();

			HasManyToMany<Nomenclature>(x => x.Nomenclatures)
			.Table("protection_tools_nomenclature")
			.ParentKeyColumn("protection_tools_id")
			.ChildKeyColumn("nomenclature_id").Cascade.All();
		}
	}
}
