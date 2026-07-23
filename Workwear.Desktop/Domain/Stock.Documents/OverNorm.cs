using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Workwear.Domain.Operations;
using Workwear.Repository.Stock;
using Workwear.Tools;

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
		#region Maped Propertires
		private IObservableList<OverNormItem> items = new ObservableList<OverNormItem>();
		[Display (Name = "Строки документа")]
		public virtual IObservableList<OverNormItem> Items {
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

		public virtual string Title => $"Выдача вне нормы ({Type.GetEnumTitle()}) №{(string.IsNullOrEmpty(DocNumber) ? Id.ToString() : DocNumber)} ({Type.GetEnumTitle()}) от {Date:d}";
		
		public virtual void AddItem(OverNormOperation operation)
		{
			if (operation == null) 
				throw new ArgumentNullException(nameof(operation));
			Items.Add(new OverNormItem(this, operation));
		}


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
		
			if (Items.Any(x => x.OverNormOperation?.WarehouseOperation == null))
				yield return new ValidationResult("Строки документа должны быть заполнены",
					new[] { nameof(Items) });

			var duplicateBarcodeTitles = Items
				.SelectMany(x => x.OverNormOperation?.BarcodeOperations ?? Enumerable.Empty<BarcodeOperation>())
				.Where(x => x.Barcode != null)
				.GroupBy(x => x.Barcode.Id > 0 ? (object)x.Barcode.Id : x.Barcode)
				.Where(x => x.Count() > 1)
				.Select(x => x.First().Barcode.Title)
				.ToList();
			if(duplicateBarcodeTitles.Any())
				yield return new ValidationResult(
					$"В документе несколько раз добавлены одни и те же штрихкоды:\n{string.Join("\n", duplicateBarcodeTitles)}",
					new[] { nameof(Items) });

			if(validationContext.Items.TryGetValue(nameof(BaseParameters), out var baseParametersObject)
			   && baseParametersObject is BaseParameters baseParameters
			   && validationContext.Items.TryGetValue(nameof(StockRepository), out var stockRepositoryObject)
			   && stockRepositoryObject is StockRepository stockRepository
			   && Warehouse != null
			   && baseParameters.CheckBalances) {
				var filledItems = Items
					.Where(x => x.OverNormOperation?.WarehouseOperation?.Nomenclature != null)
					.ToList();
				if(!filledItems.Any())
					yield break;

				var nomenclatures = filledItems
					.Select(x => x.OverNormOperation.WarehouseOperation.Nomenclature)
					.Distinct()
					.ToList();
				var excludeOperations = filledItems
					.Select(x => x.OverNormOperation.WarehouseOperation)
					.Where(x => x.Id > 0)
					.ToList();
				var balance = stockRepository.StockBalances(Warehouse, nomenclatures, Date, excludeOperations);

				foreach(var positionGroup in filledItems.GroupBy(x => x.OverNormOperation.WarehouseOperation.StockPosition)) {
					var amount = positionGroup.Sum(x => x.OverNormOperation.WarehouseOperation.Amount);
					if(amount == 0)
						continue;

					var stockExist = balance.FirstOrDefault(x => x.StockPosition.Equals(positionGroup.Key));
					if(stockExist == null) {
						yield return new ValidationResult($"На складе отсутствует - {positionGroup.Key.Title}", new[] { nameof(Items) });
						continue;
					}

					if(stockExist.Amount < amount)
						yield return new ValidationResult(
							$"Недостаточное количество - {positionGroup.Key.Title}, Необходимо: {amount} На складе: {stockExist.Amount}",
							new[] { nameof(Items) });
				}
			}
		}
	}
}
