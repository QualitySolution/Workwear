using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Workwear.Domain.Operations;

namespace Workwear.Domain.Stock.Documents
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "акты списания",
		Nominative = "акт списания",
		Genitive = "акта списания"
		)]
	[HistoryTrace]
	public class Writeoff : StockDocument, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region Свойства

		private IList<WriteoffItem> items = new List<WriteoffItem>();

		[Display (Name = "Строки документа")]
		public virtual IList<WriteoffItem> Items {
			get { return items; }
			set { SetField (ref items, value, () => Items); }
		}

		GenericObservableList<WriteoffItem> observableItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WriteoffItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<WriteoffItem> (Items);
				return observableItems;
			}
		}
			
		#endregion

		public virtual string Title{
			get{ return String.Format ("Акт списания №{0} от {1:d}", Id, Date);}
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (Date < new DateTime(2008, 1, 1))
				yield return new ValidationResult ("Дата должны указана (не ранее 2008-го)", 
					new[] { this.GetPropertyName (o => o.Date)});

			if(Items.Count == 0)
				yield return new ValidationResult ("Документ должен содержать хотя бы одну строку.", 
					new[] { this.GetPropertyName (o => o.Items)});

			if(Items.Any (i => i.Amount <= 0))
				yield return new ValidationResult ("Документ не должен содержать строк с нулевым количеством.", 
					new[] { this.GetPropertyName (o => o.Items)});
			
		}

		#endregion


		public Writeoff ()
		{
		}

		#region Обработка строк

		public virtual void AddItem(SubdivisionIssueOperation operation, int count)
		{
			if(operation.Issued == 0)
				throw new InvalidOperationException("Этот метод можно использовать только с операциями выдачи.");

			if(Items.Any(p => DomainHelper.EqualDomainObjects(p.SubdivisionWriteoffOperation?.IssuedOperation, operation))) {
				logger.Warn("Номенклатура из этой выдачи уже добавлена. Пропускаем...");
				return;
			}

			ObservableItems.Add(new WriteoffItem(this, operation, count));
		}

		public virtual WriteoffItem AddItem(EmployeeIssueOperation operation, int count)
		{
			if(operation.Issued == 0)
				throw new InvalidOperationException("Этот метод можно использовать только с операциями выдачи.");

			if(Items.Any(p => DomainHelper.EqualDomainObjects(p.EmployeeWriteoffOperation?.IssuedOperation, operation))) {
				logger.Warn("Номенклатура из этой выдачи уже добавлена. Пропускаем...");
				return null;
			}
			var item = new WriteoffItem(this, operation, count);
			ObservableItems.Add(item);
			return item;
		}

		public virtual void AddItem(StockPosition position, Warehouse warehouse, int count)
		{
			if(position == null)
				throw new ArgumentNullException(nameof(position));

			if(warehouse == null)
				throw new ArgumentNullException(nameof(warehouse));

			if(Items.Any(p => p.WarehouseOperation?.ExpenseWarehouse == warehouse && position.Equals(p.StockPosition))) {
				logger.Warn($"Позиция [{position}] для склада {warehouse.Name} уже добавлена. Пропускаем...");
				return;
			}

			ObservableItems.Add (new WriteoffItem(this, position, warehouse, count));
		}

		public virtual void RemoveItem(WriteoffItem item)
		{
			ObservableItems.Remove (item);
		}

		public virtual void UpdateOperations(IUnitOfWork uow)
		{
			Items.ToList().ForEach(x => x.UpdateOperations(uow));
		}

		#endregion
	}
}

