using FluentNHibernate.Mapping;
using Workwear.Domain.Stock.Documents;

namespace Workwear.HibernateMapping.Stock.Documents
{
	public class ExpenseDutyNormMap : ClassMap<ExpenseDutyNorm>
	{
		public ExpenseDutyNormMap ()
		{
			Table ("stock_expense_duty_norm");
			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.DocNumber).Column("doc_number");
			Map (x => x.Date).Column ("date");
			Map(x => x.Comment).Column("comment");
			Map(x => x.CreationDate).Column("creation_date");

			References (x => x.CreatedbyUser).Column ("user_id");
			References (x => x.DutyNorm).Column ("duty_norm_id").Not.Nullable();
			References(x => x.Warehouse).Column("warehouse_id").Not.Nullable();
			References (x => x.ResponsibleEmployee).Column ("responsible_employee_id");

			HasOne(x => x.IssuanceSheet)
				.PropertyRef(x => x.ExpenseDutyNorm);

			HasMany (x => x.Items)
				.Inverse()
				.KeyColumn ("stock_expense_duty_norm_id").Not.KeyNullable ()
				.Cascade.AllDeleteOrphan ()
				.LazyLoad ();
		}
	}
}
