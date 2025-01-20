﻿using FluentNHibernate.Mapping;
using Workwear.Domain.Regulations;

namespace Workwear.HibernateMapping.Regulations
{
	public class NormItemMap : ClassMap<NormItem>
	{
		public NormItemMap ()
		{
			Table ("norms_item");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else {
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
				Version(x=>x.LastUpdate).Column("last_update").Generated.Always();
			}
				
			
			Map (x => x.Amount).Column ("amount");
			Map (x => x.NormPeriod).Column ("period_type");
			Map (x => x.PeriodCount).Column ("period_count");
			Map(x => x.NormParagraph).Column("norm_paragraph");
			Map(x => x.Comment).Column("comment");

			References (x => x.Norm).Column ("norm_id").Not.Nullable ();
			References (x => x.ProtectionTools).Column ("protection_tools_id").Not.Nullable();
			References(x => x.NormCondition).Column("condition_id").Nullable();
		}
	}
}
