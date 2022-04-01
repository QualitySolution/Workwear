﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using workwear.Repository.Stock;
using workwear.Tools;

namespace workwear.Domain.Stock
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы перемещения",
		Nominative = "документ перемещения",
		Genitive = "документа перемещения"
		)]
	[HistoryTrace]
	public class Transfer : StockDocument, IValidatableObject
	{
		public Transfer() { }
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		#region Свойства
		private Warehouse warehouseFrom;
		[Display(Name = "Склад отправитель")]
		[Required(ErrorMessage = "Склад отправитель должен быть указан.")]
		public virtual Warehouse WarehouseFrom {
			get => warehouseFrom;
			set => SetField(ref warehouseFrom, value);
		}
		private Warehouse warehouseTo;
		[Display(Name = "Склад получатель")]
		[Required(ErrorMessage = "Склад получатель должен быть указан.")]
		public virtual Warehouse WarehouseTo {
			get => warehouseTo;
			set => SetField(ref warehouseTo, value);
		}
		private IList<TransferItem> items = new List<TransferItem>();
		[Display(Name = "Строки документа")]
		public virtual IList<TransferItem> Items {
			get => items;
			set => SetField(ref items, value);
		}
		private GenericObservableList<TransferItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<TransferItem> ObservableItems => 
			observableItems ?? (observableItems = new GenericObservableList<TransferItem>(Items));
		#endregion
		#region Расчетные
		public virtual string Title => $"Перемещение №{Id} от {Date:d}";
		#endregion
		#region IValidatableObject implementation
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
			if(Date < new DateTime(1990, 1, 1))
				yield return new ValidationResult("Дата должна быть указана",
					new[] { nameof(Date) });
			if(Items.Count == 0)
				yield return new ValidationResult("Документ должен содержать хотя бы одну строку.",
					new[] { nameof(Items) });
			if(Items.Any(i => i.Amount <= 0))
				yield return new ValidationResult("Документ не должен содержать строк с нулевым количеством.",
					new[] { nameof(Items) });
			if (warehouseTo == null)
				yield return new ValidationResult("Склад добавления должен быть указан",
				new[] { nameof(Items) });
			if(warehouseFrom == null)
				yield return new ValidationResult("Склад списания должен быть указан",
				new[] { nameof(Items) });
			if (WarehouseTo == WarehouseFrom)
				yield return new ValidationResult("Склад добавления должен отличаться от склада списания",
				new[] { nameof(Items) });
			var baseParameters = (BaseParameters)validationContext.Items[nameof(BaseParameters)];
			if (!baseParameters.CheckBalances) yield break;
			var strNom = items
				.Where(transferItem => transferItem.Amount > transferItem.AmountInStock)
				.Aggregate("", (current, transferItem) => current + $"\"{transferItem.Nomenclature.Name}\"\n");
			if(strNom.Length > 0)
				yield return new ValidationResult($"Количество у номенклатур:\n{strNom}больше, чем доступно на складе",
					new[] { nameof(Items) });
		}
		#endregion
		public virtual TransferItem AddItem(StockPosition position, int amount) {
			if(Items.Any(p => position.Equals(p.StockPosition))) {
				logger.Warn($"Складская позици {position.Title} из уже добавлена. Пропускаем...");
				return null;
			}
			var newItem = new TransferItem(UoW, this, position, amount);
			ObservableItems.Add(newItem);
			SetAmountInStock(newItem);
			return newItem;
		}
		public virtual void RemoveItem(TransferItem item) {
			ObservableItems.Remove(item);
		}
		public virtual void UpdateOperations(IUnitOfWork uow, Func<string, bool> askUser) {
			Items.ToList().ForEach(x => x.UpdateOperations(uow, askUser));
		}
		public virtual void SetAmountInStock(TransferItem item = null) {
			IList<Nomenclature> nomenclatures;
			IList<TransferItem> currentItems;

			if(item != null) {
				nomenclatures = new List<Nomenclature> { item.Nomenclature };
				currentItems = items.Where(x => x == item).ToList();
			}
			else {
				nomenclatures = items.Select(x => x.Nomenclature).ToList();
				currentItems = items;
			}
			var stock = new StockRepository().StockBalances(UoW, WarehouseFrom, nomenclatures, Date);
			foreach(var currentItem in currentItems) {
				var currentNomeclature = currentItem.Nomenclature;

				var stockBalanceDTO = stock
					.FirstOrDefault(x => x.Nomenclature == currentNomeclature && 
					                     x.WearSize == currentItem.WarehouseOperation.WearSize && 
					                     x.Height == currentItem.WarehouseOperation?.Height);

				if(currentItem.WarehouseOperation.Id > 0 && stockBalanceDTO != null)
					stockBalanceDTO.Amount += currentItem.WarehouseOperation.Amount;

				if (currentItem.WarehouseOperation.Id > 0 && stockBalanceDTO == null) 
					currentItem.AmountInStock = currentItem.Amount;
				else 
					currentItem.AmountInStock = stockBalanceDTO.Amount;
			}
		}
	}

}
