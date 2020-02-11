using System;
using FluentNHibernate.Mapping;
using workwear.Domain.Stock;

namespace workwear.HibernateMapping.Stock
{
	public class MassExpenseOperationMap : ClassMap<MassExpenseOperation>
	{
		public MassExpenseOperationMap()
		{
			Table("stock_mass_expense_operation");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.WarehouseOperationExpense).Column("warehouse_expense_id").Not.Nullable();
			References(x => x.EmployeeIssueOperation).Column("stock_expense_id").Not.Nullable();
			References(x => x.MassExpense).Column("doc_id").Not.Nullable();

		}
	}
}
