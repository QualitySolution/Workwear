using System;
using FluentNHibernate.Mapping;
using workwear.Domain.Regulations;

namespace workwear.HibernateMapping.Regulations
{
	public class ProfessionMap : ClassMap<Profession>
	{
		public ProfessionMap()
		{
			Table("professions");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name").Not.Nullable();
			Map(x => x.Code).Column("code");
		}
	}
}
