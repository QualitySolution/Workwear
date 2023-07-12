using FluentNHibernate.Mapping;
using Workwear.Domain.Stock.Documents;

namespace Workwear.HibernateMapping.Stock.Documents
{
	public class CollectiveExpenseMap : ClassMap<CollectiveExpense>
	{
		public CollectiveExpenseMap()
		{
			Table ("stock_collective_expense");

			if(Workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map (x => x.Date).Column ("date");
			Map(x => x.Comment).Column("comment");
			Map(x => x.CreationDate).Column("creation_date");

			References (x => x.CreatedbyUser).Column ("user_id");
			References(x => x.Warehouse).Column("warehouse_id").Not.Nullable();
			References (x => x.InCharge).Column ("in_charge_id");

			HasOne(x => x.IssuanceSheet)
				.PropertyRef(x => x.CollectiveExpense);

			HasMany (x => x.Items)
				.Inverse()
				.KeyColumn ("stock_collective_expense_id").Not.KeyNullable ()
				.Cascade.AllDeleteOrphan ()
				.LazyLoad ();
		}
	}
}
