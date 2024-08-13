using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Workwear.Tools;

namespace Workwear.Domain.Stock.Documents
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
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
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
		private IObservableList<TransferItem> items = new ObservableList<TransferItem>();
		[Display(Name = "Строки документа")]
		public virtual IObservableList<TransferItem> Items {
			get => items;
			set => SetField(ref items, value);
		} 
		#endregion
		#region Расчетные
		public virtual string Title => $"Перемещение №{DocNumberText} от {Date:d}";
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
				yield return new ValidationResult("Склад получатель должен быть указан",
				new[] { nameof(Items) });
			if(warehouseFrom == null)
				yield return new ValidationResult("Склад отправитель должен быть указан",
				new[] { nameof(Items) });
			if (WarehouseTo == WarehouseFrom)
				yield return new ValidationResult("Склад получатель должен отличаться от склада отправителя",
				new[] { nameof(Items) });
			var baseParameters = (BaseParameters)validationContext.Items[nameof(BaseParameters)];
			if (baseParameters.CheckBalances) {
				var strNom = items
					.Where(transferItem => transferItem.Amount > transferItem.AmountInStock)
					.Aggregate("", (current, transferItem) => current + $"\"{transferItem.Nomenclature.Name}\"\n");
				if (strNom.Length > 0)
					yield return new ValidationResult(
						$"Количество у номенклатур:\n{strNom}больше, чем доступно на складе",
						new[] {nameof(Items)});
			}
		}
		#endregion
		public virtual TransferItem AddItem(StockPosition position, int amount) {
			if(Items.Any(p => position.Equals(p.StockPosition))) {
				logger.Warn($"Складская позици {position.Title} из уже добавлена. Пропускаем...");
				return null;
			}
			var newItem = new TransferItem(UoW, this, position, amount);
			Items.Add(newItem);
			return newItem;
		}
		public virtual void RemoveItem(TransferItem item) {
			Items.Remove(item);
		}
		public virtual void UpdateOperations(IUnitOfWork uow, Func<string, bool> askUser) {
			Items.ToList().ForEach(x => x.UpdateOperations(uow, askUser));
		}
	}

}
