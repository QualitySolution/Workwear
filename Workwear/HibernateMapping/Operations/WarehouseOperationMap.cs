using FluentNHibernate.Mapping;
using workwear.Domain.Operations;

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
			Map(x => x.WearPercent).Column("wear_percent");
			Map(x => x.Cost).Column("cost");

			References(x => x.ReceiptWarehouse).Column("warehouse_receipt_id");
			References(x => x.ExpenseWarehouse).Column("warehouse_expense_id");
			References(x => x.Nomenclature).Column("nomenclature_id").Not.Nullable();
		}
	}
}
