﻿using FluentNHibernate.Mapping;
using workwear.Domain.Operations;

namespace workwear.HibernateMapping.Stock
{
	public class WarehouseOperationMap : ClassMap<WarehouseOperation>
	{
		public WarehouseOperationMap()
		{
			Table("operation_warehouse");

			if(workwear.HibernateMapping.MappingParams.UseIdsForTest)
				Id (x => x.Id).Column ("id").GeneratedBy.HiLo("0");
			else 
				Id (x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.OperationTime).Column("operation_time");
			Map(x => x.Amount).Column("amount");
			Map(x => x.WearPercent).Column("wear_percent");
			Map(x => x.Cost).Column("cost");

			References(x => x.ReceiptWarehouse).Column("warehouse_receipt_id");
			References(x => x.ExpenseWarehouse).Column("warehouse_expense_id");
			References(x => x.Nomenclature).Column("nomenclature_id").Not.Nullable();
			References(x => x.SizeType).Column("size_id");
			References(x => x.Height).Column("height_id");
		}
	}
}
