using System;
using FluentNHibernate.Mapping;
using workwear.Domain.Organization;

namespace workwear.HibernateMapping.Organization
{
	public class VacationTypeMap : ClassMap<VacationType>
	{
		public VacationTypeMap()
		{
			Table("vacation_type");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name").Not.Nullable();
			Map(x => x.ExcludeFromWearing).Column("exclude_from_wearing");
			Map(x => x.Comments).Column("comment");
		}
	}
}
