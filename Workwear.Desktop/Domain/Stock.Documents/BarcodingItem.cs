using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.Measurement.Domain;
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
		
		private WarehouseOperation operationExpense = new WarehouseOperation();
		[Display(Name = "Операция списания со склада")]
		[IgnoreHistoryTrace]
		public virtual WarehouseOperation OperationExpense {
			get => operationExpense;
			set => SetField(ref operationExpense, value);
		}
		#endregion

		#region Динамические свойства

		[Display(Name = "Номенклатура")] public virtual Nomenclature Nomenclature => OperationExpense?.Nomenclature;
		[Display(Name = "Количество")] public virtual int Amount => OperationExpense?.Amount ?? 0;
		[Display(Name = "Размер одежды")] public virtual Size Size => OperationExpense?.WearSize;
		[Display(Name = "Рост одежды")] public virtual Size Height  => OperationExpense?.Height;
		[Display(Name = "Собственник имущества")] public virtual Owner Owner => OperationExpense?.Owner;
		[Display(Name = "Количество")] public virtual decimal WearPercent => OperationExpense?.WearPercent ?? 0;
		[Display(Name = "Единица измерения")] public virtual MeasurementUnit Unit => Nomenclature?.Type.Units;
		public virtual string SizeName => Size?.Name ?? String.Empty;
		public virtual string HeightName  => Height?.Name ?? String.Empty;
		public virtual string OwnerName => Owner?.Name ?? String.Empty;
		public virtual string UnitsName => Unit?.Name ?? String.Empty;
		[Display(Name = "Складская позиция")] public virtual StockPosition StockPosition => new StockPosition(Nomenclature, WearPercent, Size, Height, Owner);
		[Display(Name = "Штрихкоды")] public virtual IList<Barcode> Barcodes  { get; set; }
		#endregion
	}
}
