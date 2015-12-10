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
			Map (x => x.AutoWriteoffDate).Column ("auto_writeoff_date");

			References (x => x.ExpenseDoc).Column ("stock_expense_id").Not.Nullable ();
			References (x => x.Nomenclature).Column ("nomenclature_id").Not.Nullable ();
			References (x => x.IncomeOn).Column ("stock_income_detail_id").Not.Nullable ();
		}
	}
}