﻿using System;
using FluentNHibernate.Mapping;
using workwear.Domain.Company;

namespace workwear.HibernateMapping.Company
{
	public class VacationTypeMap : ClassMap<VacationType>
	{
		public VacationTypeMap()
		{
			Table("vacation_type");

			if(workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.Name).Column("name").Not.Nullable();
			Map(x => x.ExcludeFromWearing).Column("exclude_from_wearing");
			Map(x => x.Comments).Column("comment");
		}
	}
}
