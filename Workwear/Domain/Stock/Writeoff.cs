using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using workwear.Domain.Operations;
using workwear.Repository.Operations;

namespace workwear.Domain.Stock
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "акты списания",
		Nominative = "акт списания")]
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
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
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

		public virtual void AddItem(ExpenseItem expenseFromItem, int count)
		{
			if(expenseFromItem.ExpenseDoc.Operation == ExpenseOperations.Employee)
				throw new InvalidOperationException("Этот метод нельзя использовать для выдачи сотрудникам. Используйте метод с операцией EmployeeIssueOperation.");

			if(Items.Any (p => DomainHelper.EqualDomainObjects (p.IssuedOn, expenseFromItem)))
			{
				logger.Warn ("Номенклатура из этой выдачи уже добавлена. Пропускаем...");
				return;
			}

			var newItem = new WriteoffItem (this) {
				Amount = count,
				Nomenclature = expenseFromItem.Nomenclature,
				IssuedOn = expenseFromItem,
			};
				
			ObservableItems.Add (newItem);
		}

		public virtual void AddItem(IUnitOfWork uow, EmployeeIssueOperation operation, int count)
		{
			if(operation.Issued == 0)
				throw new InvalidOperationException("Этот метод можно использовать только с операциями выдачи.");

			ExpenseItem expenseFromItem = EmployeeIssueRepository.GetExpenseItemForOperation(uow, operation);

			if(Items.Any(p => DomainHelper.EqualDomainObjects(p.IssuedOn, expenseFromItem))) {
				logger.Warn("Номенклатура из этой выдачи уже добавлена. Пропускаем...");
				return;
			}

			var newItem = new WriteoffItem(this) {
				Amount = count,
				Nomenclature = operation.Nomenclature,
				IssuedOn = expenseFromItem,
			};

			ObservableItems.Add(newItem);
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

			var newItem = new WriteoffItem (this) {
				Amount = count,
				Nomenclature = position.Nomenclature,
				Size = position.Size,
				WearGrowth = position.Growth,
				Warehouse = warehouse,
				WarehouseOperation = new WarehouseOperation() {
					Amount = count,
					Size = position.Size,
					Growth = position.Growth,
					Nomenclature = position.Nomenclature,
					OperationTime = Date,
					WearPercent = position.WearPercent,
					ExpenseWarehouse = warehouse
				}
			};

			ObservableItems.Add (newItem);
		}

		public virtual void RemoveItem(WriteoffItem item)
		{
			ObservableItems.Remove (item);
		}

		public virtual void UpdateOperations(IUnitOfWork uow, Func<string, bool> askUser)
		{
			Items.ToList().ForEach(x => x.UpdateOperations(uow, askUser));
		}
	}
}

