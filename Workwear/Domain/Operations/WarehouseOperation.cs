using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Utilities.Numeric;
using workwear.Domain.Stock;

namespace workwear.Domain.Operations
{
	public class WarehouseOperation : PropertyChangedBase, IDomainObject
	{

		public virtual int Id { get; set; }

		DateTime operationTime;


		public virtual DateTime OperationTime {
			get { return operationTime; }
			set { SetField(ref operationTime, value); }
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

		private decimal wearPercent;

		[Display(Name = "Процент износа")]
		public virtual decimal WearPercent {
			get { return wearPercent; }
			set { SetField(ref wearPercent, value.Clamp(0m, 9.99m)); }
		}

		private Expense employeeExpense;

		public virtual Expense EmployeeExpense {
			get { return employeeExpense; }
			set { SetField(ref employeeExpense, value); }
		}

		private WarehouseOperation incomeWarehouseOperation;

		public virtual WarehouseOperation IncomeWarehouseOperation {

			get { return incomeWarehouseOperation; }
			set { SetField(ref incomeWarehouseOperation, value); }
		}

	}
}
