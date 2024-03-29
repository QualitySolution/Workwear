﻿using System;
using FluentNHibernate.Mapping;
using Workwear.Domain.Regulations;

namespace Workwear.HibernateMapping.Regulations
{
	public class ProfessionMap : ClassMap<Profession>
	{
		public ProfessionMap()
		{
			Table("professions");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.Name).Column("name").Not.Nullable();
			Map(x => x.Code).Column("code");
		}
	}
}
