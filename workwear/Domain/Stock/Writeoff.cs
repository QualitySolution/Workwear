using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QSOrmProject;
using System.Linq;

namespace workwear.Domain.Stock
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "акты списания",
		Nominative = "акт списания")]
	public class Writeoff : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region Свойства

		public virtual int Id { get; set; }

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
			get{ return String.Format ("Акт списания №{0}", Id);}
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (Date < new DateTime(2008, 1, 1))
				yield return new ValidationResult ("Дата должны указана (не ранее 2008-го)", 
					new[] { this.GetPropertyName (o => o.Date)});
		}

		#endregion


		public Writeoff ()
		{
		}

		public virtual void AddItem(ExpenseItem expenseFromItem, int count)
		{
			if(Items.Any (p => DomainHelper.EqualDomainObjects (p.IssuedOn, expenseFromItem)))
			{
				logger.Warn ("Номенклатура из этой выдачи уже добавлена. Пропускаем...");
				return;
			}

			var newItem = new WriteoffItem () {
				Amount = count,
				Nomenclature = expenseFromItem.Nomenclature,
				IssuedOn = expenseFromItem,
			};
				
			ObservableItems.Add (newItem);
		}

		public virtual void AddItem(IncomeItem incomeFromItem, int count)
		{
			if(Items.Any (p => DomainHelper.EqualDomainObjects (p.IncomeOn, incomeFromItem)))
			{
				logger.Warn ("Номенклатура из этого прихода уже добавлена. Пропускаем...");
				return;
			}

			var newItem = new WriteoffItem () {
				Amount = count,
				Nomenclature = incomeFromItem.Nomenclature,
				IncomeOn = incomeFromItem,
			};

			ObservableItems.Add (newItem);
		}

		public virtual void RemoveItem(WriteoffItem item)
		{
			ObservableItems.Remove (item);
		}

	}
}

