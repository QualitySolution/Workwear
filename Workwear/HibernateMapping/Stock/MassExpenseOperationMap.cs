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

			if(workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();

			References(x => x.WarehouseOperationExpense).Column("operation_warehouse_id").Not.Nullable();
			References(x => x.EmployeeIssueOperation).Column("operation_issued_by_employee").Cascade.All().Not.Nullable();
			References(x => x.MassExpenseDoc).Column("stock_mass_expense_id").Not.Nullable();


		}
	}
}
