using FluentNHibernate.Mapping;
using workwear.Domain.Stock;

namespace workwear.HMap
{
	public class IncomeMap : ClassMap<Income>
	{
		public IncomeMap ()
		{
			Table ("stock_income");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Operation).Column ("operation").CustomType<IncomeOperationsType> ();
			Map (x => x.Date).Column ("date");
			Map (x => x.Number).Column ("number");
			Map(x => x.Comment).Column("comment");

			References (x => x.Facility).Column ("object_id");
			References (x => x.EmployeeCard).Column ("wear_card_id");
			References (x => x.CreatedbyUser).Column ("user_id");

				HasMany (x => x.Items)
				.KeyColumn ("stock_income_id").Not.KeyNullable ()
				.Cascade.AllDeleteOrphan ()
				.LazyLoad ();
		}
	}
}