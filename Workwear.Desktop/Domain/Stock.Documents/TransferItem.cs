﻿using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Workwear.Domain.Operations;
using Workwear.Models.Operations;

namespace Workwear.Domain.Stock.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения",
		Nominative = "строка перемещения",
		Genitive = "строки перемещения"
		)]
	[HistoryTrace]
	public class TransferItem : PropertyChangedBase, IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }
		private Transfer document;
		[Display(Name = "Документ")]
		[IgnoreHistoryTrace]
		public virtual Transfer Document {
			get => document;
			set => SetField(ref document, value);
		}

		Nomenclature nomenclature;
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set => SetField(ref nomenclature, value);
		}
		private int amount;
		[Display(Name = "Количество")]
		[PropertyChangedAlso("Total")]
		public virtual int Amount {
			get => amount;
			set => SetField(ref amount, value);
		}
		private WarehouseOperation warehouseOperation = new WarehouseOperation();
		[Display(Name = "Операция на складе")]
		[IgnoreHistoryTrace]
		public virtual WarehouseOperation WarehouseOperation {
			get => warehouseOperation;
			set => SetField(ref warehouseOperation, value);
		}
		
		[Display(Name = "Собственник имущества")]
		public virtual Owner Owner {
			get => WarehouseOperation.Owner;
			set {
				if(WarehouseOperation.Owner != value) {
					WarehouseOperation.Owner = value;
					OnPropertyChanged();
				}
			}
		}
		
		#endregion
		#region Расчетные
		public virtual string Title => 
			$"Перемещение {StockPosition.Title} x {Amount} со склада {document.WarehouseFrom.Name} на склад {document.WarehouseTo.Name}";
		public virtual StockPosition StockPosition => new StockPosition(
			Nomenclature, 
			WarehouseOperation.WearPercent, 
			warehouseOperation.WearSize, 
			warehouseOperation.Height, 
			warehouseOperation.Owner);
		public virtual int AmountInStock => StockBalanceModel.GetAmount(StockPosition);
		#endregion

		#region Служебные
		public virtual StockBalanceModel StockBalanceModel { get; set; }
		#endregion
		public TransferItem() { }
		public TransferItem(IUnitOfWork uow, Transfer transfer, StockPosition position, int amount) {
			document = transfer;
			warehouseOperation.Nomenclature = nomenclature = position.Nomenclature;
			warehouseOperation.WearSize = position.WearSize;
			warehouseOperation.Height = position.Height;
			warehouseOperation.WearPercent = position.WearPercent;
			warehouseOperation.Owner = position.Owner;
			warehouseOperation.Amount = this.amount = amount;
			warehouseOperation.ExpenseWarehouse = transfer.WarehouseFrom;
			warehouseOperation.OperationTime = transfer.Date;
		}
		#region Функции
		public virtual void UpdateOperations(IUnitOfWork uow, Func<string, bool> askUser) {
			WarehouseOperation.Update(uow, this);
			uow.Save(WarehouseOperation);
		}
		#endregion
	}
}
