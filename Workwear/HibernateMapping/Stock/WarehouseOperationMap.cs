using System;
using FluentNHibernate.Mapping;
using workwear.Domain.Stock;

namespace workwear.HibernateMapping.Stock
{
	public class WarehouseOperationMap : ClassMap<WarehouseOperation>
	{
		public WarehouseOperationMap()
		{

			Table("operation_warehouse");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.OperationTime).Column("operation_time");
			Map(x => x.Size).Column("size");
			Map(x => x.Growth).Column("growth");
			Map(x => x.Amount).Column("amount");

			References(x => x.ReceiptWarehouse).Column("warehouse_receipt_id");
			References(x => x.ExpenseWarehouse).Column("warehouse_expense_id");
			References(x => x.Nomenclature).Column("nomenclature_id").Not.Nullable();

		}
	}
}
