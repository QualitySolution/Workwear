using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using workwear.Domain.Operations;

namespace workwear.Domain.Stock
{
	[Appellative(Gender = GrammaticalGender.Feminine,
	NominativePlural = "строки перевода со склада на склад",
	Nominative = "строка перевода со склада на склад")]
	public class TransferItem : PropertyChangedBase, IDomainObject
	{

		#region Свойства

		public virtual int Id { get; set; }

		private Transfer document;

		[Display(Name = "Документ")]
		public virtual Transfer Document {
			get { return document; }
			set { SetField(ref document, value); }
		}

		Nomenclature nomenclature;

		[Display(Name = "Номеклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField(ref nomenclature, value, () => Nomenclature); }
		}

		int amount;

		[Display(Name = "Количество")]
		[PropertyChangedAlso("Total")]
		public virtual int Amount {
			get { return amount; }
			set { SetField(ref amount, value, () => Amount); }
		}

		private WarehouseOperation warehouseOperation = new WarehouseOperation();
		[Display(Name = "Операция на складе")]
		public virtual WarehouseOperation WarehouseOperation {
			get { return warehouseOperation; }
			set { SetField(ref warehouseOperation, value); }
		}

		private int amountInStock;
		[Display(Name = "Количество на складе")]
		public virtual int AmountInStock {
			get { return amountInStock; }
			set { SetField(ref amountInStock, value); }
		}


		#endregion

		#region Расчетные

		public virtual string Title => $"Перемещение {StockPosition.Title} x {Amount} со склада {document.WarehouseFrom.Name} на склад {document.WarehouseTo.Name}";

		public virtual StockPosition StockPosition => new StockPosition(Nomenclature, WarehouseOperation.Size, WarehouseOperation.Growth, WarehouseOperation.WearPercent);

		#endregion

		public TransferItem()
		{

		}

		public TransferItem(IUnitOfWork uow, Transfer transfer, StockPosition position, int amount)
		{
			this.document = transfer;
			this.warehouseOperation.Nomenclature = this.nomenclature = position.Nomenclature;
			this.warehouseOperation.Size = position.Size;
			this.warehouseOperation.Growth = position.Growth;
			this.warehouseOperation.WearPercent = position.WearPercent;
			this.warehouseOperation.Amount = this.amount = amount;
			this.warehouseOperation.ExpenseWarehouse = transfer.WarehouseFrom;
			this.warehouseOperation.OperationTime = transfer.Date;
		}

		#region Функции

		public virtual void UpdateOperations(IUnitOfWork uow, Func<string, bool> askUser)
		{
			WarehouseOperation.Update(uow, this);
			uow.Save(WarehouseOperation);
		}

		#endregion
	}
}
