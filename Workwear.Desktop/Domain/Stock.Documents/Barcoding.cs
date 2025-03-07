﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;

namespace Workwear.Domain.Stock.Documents {
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы маркировки",
		Nominative = "документ маркировки",
		Genitive = "документа маркировки",
		PrepositionalPlural = "документах маркировки"
	)]
	
	public class Barcoding : StockDocument, IValidatableObject {
		
		#region Свойства
		public virtual string Title{
			get{ return String.Format ("Акт оценки износа №{0} от {1:d}", DocNumberText, Date);}
		}

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
			get { return items; }
			set { SetField (ref items, value, () => Items); }
		}
		
		#endregion
		
		#region Методы
		public virtual void RemoveItem(BarcodingItem item) {
			Items.Remove (item);
		}
		public virtual void AddItem() {
			var item = (new BarcodingItem() {
				Document = this
			});
			Items.Add(item);
		}
		
		#endregion
		
		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
			throw new System.NotImplementedException();
		}
	}
}
