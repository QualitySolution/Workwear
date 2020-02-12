using FluentNHibernate.Mapping;
using workwear.Domain.Stock;

namespace workwear.HMap
{
	public class IncomeItemMap : ClassMap<IncomeItem>
	{
		public IncomeItemMap ()
		{
			Table ("stock_income_detail");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Amount).Column ("quantity");
			Map (x => x.LifePercent).Column ("life_percent");
			Map (x => x.Cost).Column ("cost");
			Map(x => x.Certificate).Column("certificate");

			References(x => x.Document).Column("stock_income_id");
			References (x => x.Nomenclature).Column ("nomenclature_id");
			References (x => x.IssuedOn).Column ("stock_expense_detail_id");
			References(x => x.EmployeeIssueOperation).Column("employee_issue_operation_id").Cascade.All();
			References(x => x.WarehouseOperation).Column("warehouse_operation_id").Not.Nullable();
		}
	}
}