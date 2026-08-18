using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
		
		[IgnoreHistoryTrace]
		public virtual IList<BarcodeOperation> WarehouseBarcodeOperations { get; set; } = new List<BarcodeOperation>();

		/// <summary>
		/// Название метки. Пока строка не сохранена в базу - редактируется и при сохранении перезаписывает Barcode.Label.
		/// Для уже сохранённой строки показывает текущее название метки, изменить отсюда уже нельзя.
		/// </summary>
		private string label;
		[Display(Name = "Название")]
		[IgnoreHistoryTrace]
		public virtual string Label {
			get => label ?? Barcodes.FirstOrDefault()?.Label;
			set => SetField(ref label, value);
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

		public virtual IEnumerable<Barcode> Barcodes => WarehouseBarcodeOperations.Select(x => x.Barcode);

		public virtual string BarcodesString => string.Join(
			"\n",
			WarehouseBarcodeOperations
				.Select(x => x.Barcode?.Title)
				.Where(x => !string.IsNullOrWhiteSpace(x)));

		public virtual bool CanEditAmount => !WarehouseBarcodeOperations.Any();
		#endregion

		#region Служебные
		public virtual StockBalanceModel StockBalanceModel { get; set; }
		#endregion
		public TransferItem() { }
		public TransferItem(Transfer transfer, StockPosition position, int amount, IEnumerable<Barcode> barcodes = null) {
			document = transfer;
			warehouseOperation.Nomenclature = nomenclature = position.Nomenclature;
			warehouseOperation.WearSize = position.WearSize;
			warehouseOperation.Height = position.Height;
			warehouseOperation.WearPercent = position.WearPercent;
			warehouseOperation.Owner = position.Owner;
			warehouseOperation.Amount = this.amount = amount;
			warehouseOperation.ExpenseWarehouse = transfer.WarehouseFrom;
			warehouseOperation.OperationTime = transfer.Date;
			AddBarcodes(barcodes);
		}

		private void AddBarcodes(IEnumerable<Barcode> barcodes) {
			var selectedBarcodes = barcodes?.ToList();
			if(selectedBarcodes == null || !selectedBarcodes.Any())
				return;

			foreach(var barcode in selectedBarcodes) {
				var newBarcodeOperation = new BarcodeOperation {
					Barcode = barcode,
					WarehouseOperation = warehouseOperation,
					KitNumber = barcode.LastOperation?.KitNumber,
				};
				barcode.BarcodeOperations.Add(newBarcodeOperation);
				WarehouseBarcodeOperations.Add(newBarcodeOperation);
			}
		}

		#region Функции
		public virtual void UpdateOperations(IUnitOfWork uow, Func<string, bool> askUser) {
			var isNew = Id == 0;
			WarehouseOperation.Update(uow, this);
			uow.Save(WarehouseOperation);
			foreach(var barcodeOperation in WarehouseBarcodeOperations) {
				uow.Save(barcodeOperation);
				if(isNew && !String.IsNullOrWhiteSpace(label) && barcodeOperation.Barcode != null) {
					barcodeOperation.Barcode.Label = label;
					uow.Save(barcodeOperation.Barcode);
				}
			}
		}

		/// <summary>
		/// Добавляет ещё один штрихкод в уже существующую строку перемещения (сканирование).
		/// </summary>
		public virtual void AddBarcode(Barcode barcode) {
			if(WarehouseBarcodeOperations.Any(x => DomainHelper.EqualDomainObjects(x.Barcode, barcode)))
				return; //уже добавлен этим документом

			AddBarcodes(new[] { barcode });
			Amount++;
		}
		#endregion
	}
}
