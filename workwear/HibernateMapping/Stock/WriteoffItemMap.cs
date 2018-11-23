using FluentNHibernate.Mapping;
using workwear.Domain.Stock;

namespace workwear.HMap
{
	public class WriteoffItemMap : ClassMap<WriteoffItem>
	{
		public WriteoffItemMap ()
		{
			Table ("stock_write_off_detail");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Amount).Column ("quantity");

			References(x => x.Document).Column("stock_write_off_id");
			References (x => x.Nomenclature).Column ("nomenclature_id");
			References (x => x.IssuedOn).Column ("stock_expense_detail_id");
			References (x => x.IncomeOn).Column ("stock_income_detail_id");
			References(x => x.EmployeeIssueOperation).Column("employee_issue_operation_id").Cascade.All();
		}
	}
}