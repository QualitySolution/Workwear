using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QSOrmProject;

namespace workwear.Domain.Stock
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "выдачи",
		Nominative = "выдача")]
	public class Expense : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region Свойства

		public virtual int Id { get; set; }

		ExpenseOperations operation;

		[Display (Name = "Тип операции")]
		public virtual ExpenseOperations Operation {
			get { return operation; }
			set { SetField (ref operation, value, () => Operation); }
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

		DateTime date;

		[Display (Name = "Дата")]
		public virtual DateTime Date {
			get { return date; }
			set { SetField (ref date, value, () => Date); }
		}

		User createdbyUser;

		[Display (Name = "Карточку создал")]
		public virtual User CreatedbyUser {
			get { return createdbyUser; }
			set { SetField (ref createdbyUser, value, () => CreatedbyUser); }
		}

		private IList<ExpenseItem> items = new List<ExpenseItem>();

		[Display (Name = "Строки документа")]
		public virtual IList<ExpenseItem> Items {
			get { return items; }
			set { SetField (ref items, value, () => Items); }
		}

		GenericObservableList<ExpenseItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ExpenseItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<ExpenseItem> (Items);
				return observableItems;
			}
		}
			
		#endregion

		public virtual string Title{
			get{ return String.Format ("Выдача №{0}", Id);}
		}

		public Expense ()
		{
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (Date < new DateTime(2008, 1, 1))
				yield return new ValidationResult ("Дата должны указана (не ранее 2008-го)", 
					new[] { this.GetPropertyName (o => o.Date)});

			if(Operation == ExpenseOperations.Object && Facility == null)
				yield return new ValidationResult ("Объект должен быть указан", 
					new[] { this.GetPropertyName (o => o.Date)});

			if(Operation == ExpenseOperations.Employee && EmployeeCard == null)
				yield return new ValidationResult ("Сотрудник должен быть указан", 
					new[] { this.GetPropertyName (o => o.Date)});

			if(Items.Count == 0)
				yield return new ValidationResult ("Документ должен содержать хотя бы одну строку.", 
					new[] { this.GetPropertyName (o => o.Items)});

			if(Items.Any (i => i.Amount <= 0))
				yield return new ValidationResult ("Документ не должен содержать строк с нулевым количеством.", 
					new[] { this.GetPropertyName (o => o.Items)});
		}
		#endregion

		public virtual void AddItem(IncomeItem fromIncomeItem)
		{
			AddItem (fromIncomeItem, 1);
		}

		public virtual void AddItem(IncomeItem fromIncomeItem, int amount)
		{
			if(Items.Any (p => DomainHelper.EqualDomainObjects (p.IncomeOn, fromIncomeItem)))
			{
				logger.Warn ("Номенклатура из этого поступления уже добавлена. Пропускаем...");
				return;
			}
			var newItem = new ExpenseItem () {
				ExpenseDoc = this,
				Amount = amount,
				Nomenclature = fromIncomeItem.Nomenclature,
				IncomeOn = fromIncomeItem
			};

			ObservableItems.Add (newItem);
		}

		public virtual void RemoveItem(ExpenseItem item)
		{
			ObservableItems.Remove (item);
		}

	}

	public enum ExpenseOperations {
		[Display(Name = "Выдача сотруднику")]
		Employee,
		[Display(Name = "Выдача на объект")]
		Object
	}

	public class ExpenseOperationsType : NHibernate.Type.EnumStringType
	{
		public ExpenseOperationsType () : base (typeof(ExpenseOperations))
		{
		}
	}
}

