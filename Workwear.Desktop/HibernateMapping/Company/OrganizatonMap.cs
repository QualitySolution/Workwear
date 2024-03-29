﻿using FluentNHibernate.Mapping;
using Workwear.Domain.Company;

namespace Workwear.HibernateMapping.Company
{
	public class OrganizationMap : ClassMap<Organization>
	{
		public OrganizationMap()
		{
			Table ("organizations");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.Name).Column("name");
			Map(x => x.Address).Column("address");
		}
	}
}
