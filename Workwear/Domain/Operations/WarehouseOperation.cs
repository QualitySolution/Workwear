using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Utilities.Numeric;
using workwear.Domain.Sizes;
using workwear.Domain.Stock;

namespace workwear.Domain.Operations
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "складские операции",
		Nominative = "складская операция",
		Genitive = "складской операции"
	)]
	[HistoryTrace]
	public class WarehouseOperation : PropertyChangedBase, IDomainObject
	{

		public virtual int Id { get; set; }

		DateTime operationTime;
		[Display(Name = "Время операции")]
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

		string size;
		[Obsolete("Работа с размерами перенесена в классы Size, SizeType и SizeService")]
		[Display(Name ="Размер")]
		public virtual string Size {
			get { return String.IsNullOrWhiteSpace(size) ? null : size; }
			set { SetField(ref size, value); }
		}

		string growth;
		[Obsolete("Работа с размерами перенесена в классы Size, SizeType и SizeService")]
		[Display(Name = "Рост")]
		public virtual string Growth {
			get { return String.IsNullOrWhiteSpace(growth) ? null : growth; }
			set { SetField(ref growth, value); }
		}

		int amount;

		[Display(Name = "Количество")]
		[PropertyChangedAlso("Total")]
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

		decimal cost;

		[Display(Name = "Цена")]
		[PropertyChangedAlso("Total")]
		public virtual decimal Cost {
			get { return cost; }
			set { SetField(ref cost, value, () => Cost); }
		}
		[Display(Name ="Размер")]
		public virtual Size SizeType { get; set; }
		[Display(Name = "Рост")]
		public virtual Size Height { get; set; }

		#region Расчетные

		public virtual decimal Total => Cost * Amount;

		public virtual string Title => ReceiptWarehouse != null && ExpenseWarehouse != null
			? $"Перемещение {Amount} х {Nomenclature?.Name} из {ExpenseWarehouse.Name} в {ReceiptWarehouse.Name}"
			: ReceiptWarehouse != null 
				? $"Поступление {Amount} х {Nomenclature?.Name} в {ReceiptWarehouse.Name}"
				: ExpenseWarehouse != null 
					? $"Списание {Amount} х {Nomenclature?.Name} из {ExpenseWarehouse.Name}"
					: $"Перемещение {Amount} х {Nomenclature?.Name} из пустого в порожнее(оба склада не указаны)";

		#endregion

		#region Методы обновления операций

		public virtual void Update(IUnitOfWork uow, ExpenseItem item)
		{
			//Внимание здесь сравниваются даты без времени.
			if(item.ExpenseDoc.Date.Date != OperationTime.Date)
				OperationTime = item.ExpenseDoc.Date;

			ExpenseWarehouse = item.ExpenseDoc.Warehouse;
			ReceiptWarehouse = null;
			Nomenclature = item.Nomenclature;
			Size = item.Size;
			Growth = item.WearGrowth;
			Amount = item.Amount;
		}

		public virtual void Update(IUnitOfWork uow, CollectiveExpenseItem item)
		{
			//Внимание здесь сравниваются даты без времени.
			if(item.Document.Date.Date != OperationTime.Date)
				OperationTime = item.Document.Date;

			ExpenseWarehouse = item.Document.Warehouse;
			ReceiptWarehouse = null;
			Nomenclature = item.Nomenclature;
			Size = item.Size;
			Growth = item.WearGrowth;
			Amount = item.Amount;
		}

		public virtual void Update(IUnitOfWork uow, IncomeItem item)
		{
			//Внимание здесь сравниваются даты без времени.
			if(item.Document.Date.Date != OperationTime.Date)
				OperationTime = item.Document.Date;

			ReceiptWarehouse = item.Document.Warehouse;
			Nomenclature = item.Nomenclature;
			Size = item.Size;
			Growth =item.WearGrowth;
			Amount = item.Amount;
		}

		public virtual void Update(IUnitOfWork uow, WriteoffItem item)
		{
			//Внимание здесь сравниваются даты без времени.
			if(item.Document.Date.Date != OperationTime.Date)
				OperationTime = item.Document.Date;

			ExpenseWarehouse = item.Warehouse;
			ReceiptWarehouse = null;
			Nomenclature = item.Nomenclature;
			Size = item.Size;
			Growth = item.WearGrowth;
			Amount = item.Amount;
		}

		public virtual void Update(IUnitOfWork uow, TransferItem item)
		{
			//Внимание здесь сравниваются даты без времени.
			if(item.Document.Date.Date != OperationTime.Date)
				OperationTime = item.Document.Date;

			ReceiptWarehouse = item.Document.WarehouseTo;
			ExpenseWarehouse = item.Document.WarehouseFrom;
			Nomenclature = item.Nomenclature;
			amount = item.Amount;
		}

		#endregion
	}
}
