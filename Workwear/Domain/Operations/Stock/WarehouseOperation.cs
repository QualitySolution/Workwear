using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace workwear.Domain.Stock
{
	public class WarehouseOperation : PropertyChangedBase, IDomainObject
	{

		public virtual int Id { get; set; }

		DateTime? wareOperationTime;

	
		public virtual DateTime? WareOperationTime {
			get { return wareOperationTime; }
			set { SetField(ref wareOperationTime, value); }
		}

		Warehouse receiptWarehouse;

		[Display(Name = "Склад прихода")]
		public virtual Warehouse ReceiptWarehouse {
			get { return receiptWarehouse; }
			set { SetField(ref receiptWarehouse, value); }
		}

		Warehouse expenseWarehouse;

		[Display(Name = "Склад расхода")]
		public virtual Warehouse ExpenseWarehouse {
			get { return expenseWarehouse; }
			set { SetField(ref expenseWarehouse, value); }
		}


		Nomenclature nomenclature;

		[Display(Name = "Номеклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField(ref nomenclature, value); }
		}


		int size;

		[Display(Name ="Размер")]
		public virtual int Size {
			get { return size; }
			set { SetField(ref size, value); }
		}


		int growth;

		[Display(Name = "Рост")]
		public virtual int Growth {
			get { return growth; }
			set { SetField(ref growth, value); }
		}


		int amount;

		[Display(Name = "Количество")]
		public virtual int Amount {
			get { return amount; }
			set { SetField(ref amount, value); }
		}


	

	}
}
