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
			Map(x => x.Comment).Column("comment");

			References (x => x.Subdivision).Column ("object_id");
			References (x => x.Employee).Column ("wear_card_id");
			References (x => x.CreatedbyUser).Column ("user_id");

			HasOne(x => x.IssuanceSheet)
				.Cascade.All()
				.PropertyRef(x => x.Expense);

			HasMany (x => x.Items)
				.Inverse()
				.KeyColumn ("stock_expense_id").Not.KeyNullable ()
				.Cascade.AllDeleteOrphan ().Inverse ()
				.LazyLoad ();
		}
	}
}