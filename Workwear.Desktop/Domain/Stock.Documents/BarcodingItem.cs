using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.BusinessCommon.Domain;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;

namespace Workwear.Domain.Stock.Documents {
		[Appellative(Gender = GrammaticalGender.Feminine,
			NominativePlural = "строки маркировки",
			Nominative = "строка маркировки",
			Genitive = "строки маркировки",
			PrepositionalPlural = "строках маркировки"
		)]
		
	public class BarcodingItem : PropertyChangedBase, IDomainObject {
		
		#region Хранимые Свойства

		public virtual int Id { get; }

		public virtual string Title => $"Маркировка №{Document.DocNumberText}, {Amount} {Nomenclature?.Type?.Units?.Name} {StockPosition.Title}";
		
		private Barcoding document;
		[Display(Name = "Документ маркировки")]
		[IgnoreHistoryTrace]
		public virtual Barcoding Document {
			get => document;
			set => SetField(ref document, value);
		}

		private WarehouseOperation operationReceipt = new WarehouseOperation();
		[Display(Name = "Операция поступления с маркировкой")]
		[IgnoreHistoryTrace]
		public virtual WarehouseOperation OperationReceipt {
			get => operationReceipt;
			set => SetField(ref operationReceipt, value);
		}
		
		private WarehouseOperation operationExpence = new WarehouseOperation();
		[Display(Name = "Операция списания со склада")]
		[IgnoreHistoryTrace]
		public virtual WarehouseOperation OperationExpence {
			get => operationExpence;
			set => SetField(ref operationExpence, value);
		}
		#endregion

		#region Динамические свойства

		[Display(Name = "Номенклатура")] public virtual Nomenclature Nomenclature => OperationExpence?.Nomenclature;
		[Display(Name = "Количество")] public virtual int Amount => OperationExpence?.Amount ?? 0;
		[Display(Name = "Рост одежды")] public virtual Size Size => OperationExpence?.WearSize;
		[Display(Name = "Рост одежды")] public virtual Size Height  => OperationExpence?.Height;
		[Display(Name = "Собственник имущества")] public virtual Owner Owner => OperationExpence?.Owner;
		[Display(Name = "Количество")] public virtual decimal WearPercent => OperationExpence?.WearPercent ?? 0;
		[Display(Name = "Единица измерения")] public virtual MeasurementUnits Units => Nomenclature?.Type.Units;
		public virtual string SizeName => Size?.Name ?? String.Empty;
		public virtual string HeightName  => Height?.Name ?? String.Empty;
		public virtual string OwnerName => Owner?.Name ?? String.Empty;
		public virtual string UnitsName => Units?.Name ?? String.Empty;
		[Display(Name = "Складская позиция")] public virtual StockPosition StockPosition => new StockPosition(Nomenclature, WearPercent, Size, Height, Owner);
		[Display(Name = "Штрихкоды")] public virtual IEnumerable<Barcode> Barcodes  { get; set; }
		[Display(Name = "Операции штрихкодов")] public virtual IList<BarcodeOperation> BarcodeOperations { get; set; }
		
	    #endregion
	}
}
