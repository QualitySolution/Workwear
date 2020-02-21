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
			Map(x => x.Size).Column("size");
			Map(x => x.SizeStd).Column("size_std");
			Map(x => x.WearGrowth).Column("growth");
			Map(x => x.WearGrowthStd).Column("growth_std");

			References(x => x.Document).Column("stock_income_id");
			References (x => x.Nomenclature).Column ("nomenclature_id");
			References (x => x.IssuedOn).Column ("stock_expense_detail_id");
			References(x => x.EmployeeIssueOperation).Column("employee_issue_operation_id").Cascade.All();
			References(x => x.WarehouseOperation).Column("warehouse_operation_id").Not.Nullable();
		}
	}
}