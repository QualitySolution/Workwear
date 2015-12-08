using FluentNHibernate.Mapping;
using workwear.Domain.Stock;

namespace workwear.HMap
{
	public class ExpenseMap : ClassMap<Expense>
	{
		public ExpenseMap ()
		{
			Table ("stock_expense");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Operation).Column ("operation").CustomType<ExpenseOperationsType> ();
			Map (x => x.Date).Column ("date");

			References (x => x.Facility).Column ("object_id");
			References (x => x.EmployeeCard).Column ("wear_card_id");
			References (x => x.CreatedbyUser).Column ("user_id");

				HasMany (x => x.Items)
				.KeyColumn ("stock_expense_id").Not.KeyNullable ()
				.Cascade.AllDeleteOrphan ()
				.LazyLoad ();
		}
	}
}