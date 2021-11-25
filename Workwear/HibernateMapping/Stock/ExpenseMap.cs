using FluentNHibernate.Mapping;
using workwear.Domain.Stock;

namespace workwear.HMap
{
	public class ExpenseMap : ClassMap<Expense>
	{
		public ExpenseMap ()
		{
			Table ("stock_expense");
			if(workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map (x => x.Operation).Column ("operation").CustomType<ExpenseOperationsType> ();
			Map (x => x.Date).Column ("date");
			Map(x => x.Comment).Column("comment");

			References (x => x.Subdivision).Column ("object_id");
			References (x => x.Employee).Column ("wear_card_id");
			References (x => x.CreatedbyUser).Column ("user_id");
			References(x => x.Warehouse).Column("warehouse_id").Not.Nullable();
			References(x => x.WriteOffDoc).Column("write_off_doc");

			HasOne(x => x.IssuanceSheet)
				.PropertyRef(x => x.Expense);

			HasMany (x => x.Items)
				.Inverse()
				.KeyColumn ("stock_expense_id").Not.KeyNullable ()
				.Cascade.AllDeleteOrphan ()
				.LazyLoad ();
		}
	}
}