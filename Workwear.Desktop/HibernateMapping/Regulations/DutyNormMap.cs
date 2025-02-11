using FluentNHibernate.Mapping;
using Workwear.Domain.Regulations;

namespace Workwear.HibernateMapping.Regulations
{
	public class DutyNormMap : ClassMap<DutyNorm>
	{
		public DutyNormMap ()
		{
			Table ("duty_norms");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.Name).Column("name");
			Map(x => x.NormParagraph).Column("norm_paragraph");
			Map(x => x.DateFrom).Column("datefrom");
			Map(x => x.DateTo).Column("dateto");
			Map(x => x.Comment).Column("comment");

			References(x => x.ResponsibleLeader).Column("responsible_leder_id");
			References(x => x.ResponsibleEmployee).Column("responsible_employee_id");
			References(x => x.Subdivision).Column("subdivision_id");

			HasMany (x => x.Items).KeyColumn ("duty_norm_id").Not.KeyNullable ().Inverse ().Cascade.AllDeleteOrphan ()
				.LazyLoad ();
		}
	}
}
