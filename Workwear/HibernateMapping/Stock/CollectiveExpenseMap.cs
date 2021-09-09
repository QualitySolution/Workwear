using FluentNHibernate.Mapping;
using workwear.Domain.Stock;

namespace workwear.HMap
{
	public class CollectiveExpenseMap : ClassMap<CollectiveExpense>
	{
		public CollectiveExpenseMap()
		{
			Table ("stock_collective_expense");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Date).Column ("date");
			Map(x => x.Comment).Column("comment");

			References (x => x.CreatedbyUser).Column ("user_id");
			References(x => x.Warehouse).Column("warehouse_id").Not.Nullable();

			HasOne(x => x.IssuanceSheet)
				.Cascade.All()
				.PropertyRef(x => x.CollectiveExpense);

			HasMany (x => x.Items)
				.Inverse()
				.KeyColumn ("stock_collective_expense_id").Not.KeyNullable ()
				.Cascade.AllDeleteOrphan ()
				.LazyLoad ();
		}
	}
}