﻿using FluentNHibernate.Mapping;
using Workwear.Domain.Company;

namespace Workwear.HibernateMapping.Company
{
	public class PostMap : ClassMap<Post>
	{
		public PostMap ()
		{
			Table ("posts");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map (x => x.Name).Column ("name").Not.Nullable ();
			Map(x => x.Code).Column("code");
			Map(x => x.Comments).Column("comments");

			References(x => x.Subdivision).Column("subdivision_id");
			References(x => x.Department).Column("department_id");
			References(x => x.Profession).Column("profession_id");
			References(x => x.CostCenter).Column("cost_center_id");
		}
	}
}
