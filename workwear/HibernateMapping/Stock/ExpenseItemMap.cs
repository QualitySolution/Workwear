using FluentNHibernate.Mapping;
using workwear.Domain.Stock;

namespace workwear.HMap
{
	public class ExpenseItemMap : ClassMap<ExpenseItem>
	{
		public ExpenseItemMap ()
		{
			Table ("stock_expense_detail");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Amount).Column ("quantity");

			References (x => x.Nomenclature).Column ("nomenclature_id");
			References (x => x.IncomeOn).Column ("stock_income_detail_id");
		}
	}
}