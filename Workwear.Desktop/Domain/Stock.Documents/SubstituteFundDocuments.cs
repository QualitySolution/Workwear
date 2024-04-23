using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Workwear.Domain.Operations;

namespace Workwear.Domain.Stock.Documents 
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Подменные выдачи",
		Nominative = "Подменная выдача",
		Genitive = "Подменной выдачи"
	)]
	[HistoryTrace]
	public class SubstituteFundDocuments : StockDocument, IValidatableObject
	{
		#region Propertires
		private IObservableList<SubstituteFundDocumentItem> items = new ObservableList<SubstituteFundDocumentItem>();
		[Display (Name = "Строки документа")]
		public virtual IObservableList<SubstituteFundDocumentItem> Items 
		{
			get => items;
			set => SetField (ref items, value, () => Items);
		}
		
		private Warehouse warehouse;
		[Display(Name = "Склад")]
		public virtual Warehouse Warehouse 
		{
			get => warehouse;
			set => SetField(ref warehouse, value);
		}
		#endregion

		public virtual void AddItem(SubstituteFundOperation operation) 
		{
			Items.Add(new SubstituteFundDocumentItem(this, operation));
		}

		public virtual void DeleteItem(SubstituteFundDocumentItem item) 
		{
			item.Document = null;
			item.SubstituteFundOperation = null;
			items.Remove(item);
		}
		
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (Date < new DateTime(2008, 1, 1)) 
			{
				yield return new ValidationResult("Дата должна быть не ранее 2008-го года",
					new[] { this.GetPropertyName(o => o.Date) });
			}
			if (Items.Count == 0) 
			{
				yield return new ValidationResult("Документ должен содержать хотя бы одну строку.",
					new[] { this.GetPropertyName(o => o.Items) });
			}
		}
	}
}
