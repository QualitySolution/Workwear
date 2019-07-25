using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using workwear.Domain.Operations;
using workwear.Domain.Organization;
using workwear.Repository.Operations;

namespace workwear.Domain.Stock
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "приходные документы",
		Nominative = "приходный документ")]
	public class Income : StockDocument, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region Свойства

		IncomeOperations operation;

		[Display (Name = "Тип операции")]
		public virtual IncomeOperations Operation {
			get { return operation; }
			set { SetField (ref operation, value, () => Operation); }
		}

		string number;

		[Display (Name = "Вх. номер")]
		public virtual string Number {
			get { return number; }
			set { SetField (ref number, value, () => Number); }
		}

		EmployeeCard employeeCard;

		[Display (Name = "Сотрудник")]
		public virtual EmployeeCard EmployeeCard {
			get { return employeeCard; }
			set { SetField (ref employeeCard, value, () => EmployeeCard); }
		}

		Facility facility;

		[Display (Name = "Объект")]
		public virtual Facility Facility {
			get { return facility; }
			set { SetField (ref facility, value, () => Facility); }
		}

		private IList<IncomeItem> items = new List<IncomeItem>();

		[Display (Name = "Строки документа")]
		public virtual IList<IncomeItem> Items {
			get { return items; }
			set { SetField (ref items, value, () => Items); }
		}

		GenericObservableList<IncomeItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<IncomeItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<IncomeItem> (Items);
				return observableItems;
			}
		}
			
		#endregion

		public virtual string Title{
			get{
				switch (Operation) {
				case IncomeOperations.Enter:
					return String.Format ("Приходная накладная №{0} от {1:d}", Id, Date);
				case IncomeOperations.Return:
					return String.Format ("Возврат от работника №{0} от {1:d}", Id, Date);
				case IncomeOperations.Object:
					return String.Format ("Возврат c объекта №{0} от {1:d}", Id, Date);
				default:
					return null;
				}
			}
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (Date < new DateTime(2008, 1, 1))
				yield return new ValidationResult ("Дата должны указана (не ранее 2008-го)", 
					new[] { this.GetPropertyName (o => o.Date)});

			if(Operation == IncomeOperations.Object && Facility == null)
				yield return new ValidationResult ("Объект должен быть указан", 
					new[] { this.GetPropertyName (o => o.Date)});

			if(Operation == IncomeOperations.Return && EmployeeCard == null)
				yield return new ValidationResult ("Сотрудник должен быть указан", 
					new[] { this.GetPropertyName (o => o.Date)});

			if(Items.Count == 0)
				yield return new ValidationResult ("Документ должен содержать хотя бы одну строку.", 
					new[] { this.GetPropertyName (o => o.Items)});

			if(Items.Any (i => i.Amount <= 0))
				yield return new ValidationResult ("Документ не должен содержать строк с нулевым количеством.", 
					new[] { this.GetPropertyName (o => o.Items)});

			if (Items.Any(i => i.Certificate != null && i.Certificate.Length > 40))
				yield return new ValidationResult("Длина номера сертификата не может быть больше 40 символов.",
					new[] { this.GetPropertyName(o => o.Items) });

		}

		#endregion


		public Income ()
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
			decimal life = expenseFromItem.IncomeOn.LifePercent;
			decimal cost = expenseFromItem.IncomeOn.Cost;
			if(expenseFromItem.AutoWriteoffDate.HasValue)
			{
				double multiplier = (expenseFromItem.AutoWriteoffDate.Value - DateTime.Today).TotalDays / (expenseFromItem.AutoWriteoffDate.Value - expenseFromItem.ExpenseDoc.Date).TotalDays;
				life = (life * (decimal)multiplier);
				cost = (cost * (decimal)multiplier);
			}

			var newItem = new IncomeItem(this)
			{
				Amount = count,
				Nomenclature = expenseFromItem.Nomenclature,
				IssuedOn = expenseFromItem,
				Cost = cost,
				LifePercent = life,
				Certificate = expenseFromItem.IncomeOn?.Certificate
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
			decimal life = 1 - operation.WearPercent;
			decimal cost = operation.IncomeOnStock.Cost;
			if(operation.ExpiryByNorm.HasValue) {
				decimal wearPercent = operation.CalculatePercentWear(DateTime.Today);
				life = 1 - wearPercent;
				cost = Math.Max(cost - cost * wearPercent, 0);
			}

			var newItem = new IncomeItem(this) {
				Amount = count,
				Nomenclature = operation.Nomenclature,
				IssuedOn = expenseFromItem,
				Cost = cost,
				LifePercent = life,
				Certificate = expenseFromItem.IncomeOn?.Certificate
			};

			ObservableItems.Add(newItem);
		}

		public virtual IncomeItem AddItem(Nomenclature nomenclature)
		{
			if (Operation != IncomeOperations.Enter)
				throw new InvalidOperationException ("Добавление номенклатуры возможно только во входящую накладную. Возвраты должны добавляться с указанием строки выдачи.");

			if(Items.Any (p => DomainHelper.EqualDomainObjects (p.Nomenclature, nomenclature)))
			{
				logger.Warn ("Номенклатура из уже добавлена. Пропускаем...");
				return null;
			}

			var newItem = new IncomeItem (this) {
				Amount = 1,
				Nomenclature = nomenclature,
				Cost = 0,
				LifePercent = 1,
			};

			ObservableItems.Add (newItem);
			return newItem;
		}

		public virtual void RemoveItem(IncomeItem item)
		{
			ObservableItems.Remove (item);
		}

		public virtual void UpdateOperations(IUnitOfWork uow, Func<string, bool> askUser)
		{
			Items.ToList().ForEach(x => x.UpdateOperations(uow, askUser));
		}
	}

	public enum IncomeOperations {
		[Display(Name = "Приходная накладная")]
		Enter,
		[Display(Name = "Возврат от работника")]
		Return,
		[Display(Name = "Возврат с объекта")]
		Object
	}

	public class IncomeOperationsType : NHibernate.Type.EnumStringType
	{
		public IncomeOperationsType () : base (typeof(IncomeOperations))
		{
		}
	}
}

