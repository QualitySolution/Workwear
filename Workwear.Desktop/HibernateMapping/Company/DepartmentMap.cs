﻿using FluentNHibernate.Mapping;
using Workwear.Domain.Company;

namespace Workwear.HibernateMapping.Company
{
	public class DepartmentMap : ClassMap<Department>
	{
		public DepartmentMap()
		{
			Table("departments");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.Name).Column("name");
			Map(x => x.Code).Column("code");
			Map(x => x.Comments).Column("comments");

			References(x => x.Subdivision).Column("subdivision_id");
		}
	}
}
