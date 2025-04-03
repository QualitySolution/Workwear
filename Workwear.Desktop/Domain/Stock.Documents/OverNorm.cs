using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Workwear.Domain.Operations;
using Workwear.Tools.OverNorms;
using Workwear.Tools.OverNorms.Impl;

namespace Workwear.Domain.Stock.Documents 
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "выдачи вне нормы",
		Nominative = "выдача вне нормы",
		Genitive = "выдачи вне нормы"
	)]
	[HistoryTrace]
	public class OverNorm : StockDocument, IValidatableObject
	{
		#region Propertires
		private IObservableList<OverNormItem> items = new ObservableList<OverNormItem>();
		[Display (Name = "Строки документа")]
		public virtual IObservableList<OverNormItem>Items {
			get => items;
			set => SetField (ref items, value);
		}

		private OverNormType type;
		[Display(Name = "Тип операции выдачи вне нормы")]
		public virtual OverNormType Type {
			get => type;
			set => SetField(ref type, value);
		}
		
		private Warehouse warehouse;
		[Display(Name = "Склад")]
		public virtual Warehouse Warehouse {
			get => warehouse;
			set => SetField(ref warehouse, value);
		}
		#endregion

		#region Not Mapped Propertis
		public virtual string Title => $"Выдача вне нормы ({Type.GetAttribute<DisplayAttribute>().Name}) №{(string.IsNullOrEmpty(DocNumber) ? Id.ToString() : DocNumber)} ({Type.GetAttribute<DisplayAttribute>().Name}) от {Date:d}";
		
		public virtual void AddItem(OverNormOperation operation, OverNormParam param = null)
		{
			if (operation == null) 
				throw new ArgumentNullException(nameof(operation));
			Items.Add(new OverNormItem(this, operation) { Param = param });
		}
		#endregion

		public virtual void DeleteItem(OverNormItem item) 
		{
			Items.Remove(item);
		}
		
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (Warehouse == null) 
				yield return new ValidationResult("Склад должен быть указан",
					new[] { nameof(Warehouse) });
			
			if (Date < new DateTime(2008, 1, 1)) 
				yield return new ValidationResult("Дата должна быть не ранее 2008-го года",
					new[] { nameof(Date) });
			
			if (!Items.Any()) 
				yield return new ValidationResult("Документ должен содержать хотя бы одну строку",
					new[] { nameof(Items) });
		
			if (Items.Any(x => x.Param == null)) 
				yield return new ValidationResult("Строки документа должны быть заполнены",
					new[] { nameof(Items) });
		}
	}
}
