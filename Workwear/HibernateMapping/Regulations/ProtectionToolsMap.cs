using System;
using FluentNHibernate.Mapping;
using workwear.Domain.Regulations;

namespace workwear.HibernateMapping.Regulations
{
	public class ProtectionToolsMap : ClassMap<ProtectionTools>
	{
		public ProtectionToolsMap()
		{
			Table("protection_tools");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name").Not.Nullable();

			HasManyToMany<ProtectionTools>(x => x.Analogs)
			.Table("protection_tools_replacement")
			.ParentKeyColumn("protection_tools_id")
			.ChildKeyColumn("protection_tools_analog_id").Cascade.All();
		}
	}
}
