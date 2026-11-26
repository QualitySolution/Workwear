using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using Workwear.Domain.Operations;

namespace Workwear.Domain.Stock.Documents {
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы маркировки",
		Nominative = "документ маркировки",
		Genitive = "документа маркировки",
		PrepositionalPlural = "документах маркировки"
	)]
	
	public class Barcoding : StockDocument {
		
		#region Свойства
		public virtual string Title => $"Документ маркировки №{DocNumberText} от {Date:d}";

		private Warehouse warehouse;
		[Display(Name = "Склад")]
		[Required(ErrorMessage = "Склад должен быть указан.")]
		public virtual Warehouse Warehouse {
			get => warehouse;
			set { SetField(ref warehouse, value, () => Warehouse); }
		}
		
		private IObservableList<BarcodingItem> items = new ObservableList<BarcodingItem>();
		[Display (Name = "Строки документа")]
		public virtual IObservableList<BarcodingItem> Items {
			get => items;
			set { SetField (ref items, value, () => Items); }
		}
		
		#endregion
		
		#region Методы
		public virtual void RemoveItem(BarcodingItem item) {
			Items.Remove (item);
		}
		#endregion

		public virtual BarcodingItem AddItem(WarehouseOperation operationExpense, WarehouseOperation operationReceipt, IList<Barcode> barcodes) {
			var item = (new BarcodingItem() {
				Document = this,
				OperationExpense = operationExpense,
				OperationReceipt = operationReceipt,
				Barcodes = barcodes
			});
			Items.Add(item);
			return item;
		}
	}
}
