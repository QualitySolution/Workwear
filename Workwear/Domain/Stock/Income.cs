using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Utilities;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Measurements;

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

		private Warehouse warehouse;

		[Display(Name = "Склад")]
		[Required(ErrorMessage = "Склад должен быть указан.")]
		public virtual Warehouse Warehouse {
			get { return warehouse; }
			set { SetField(ref warehouse, value, () => Warehouse); }
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

		Subdivision subdivision;

		[Display (Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get { return subdivision; }
			set { SetField (ref subdivision, value, () => Subdivision); }
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

			if(Operation == IncomeOperations.Object && Subdivision == null)
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

			foreach(var duplicate in Items.GroupBy(x => x.StockPosition).Where(x => x.Count() > 1)) {
				var caseCountText = NumberToTextRus.FormatCase(duplicate.Count(), "{0} раз", "{0} раза", "{0} раз");
				yield return new ValidationResult($"Складская позиция {duplicate.First().Title} указана в документе {caseCountText}.",
					new[] { this.GetPropertyName(o => o.Items) });
			}
		}

		#endregion


		public Income ()
		{
		}

		public virtual void AddItem(SubdivisionIssueOperation issuedOperation, int count)
		{
			if(issuedOperation.Issued == 0)
				throw new InvalidOperationException("Этот метод можно использовать только с операциями выдачи.");

			if(Items.Any (p => DomainHelper.EqualDomainObjects (p.IssuedSubdivisionOnOperation, issuedOperation)))
			{
				logger.Warn ("Номенклатура из этой выдачи уже добавлена. Пропускаем...");
				return;
			}

			var newItem = new IncomeItem(this)
			{
				Amount = count,
				Nomenclature = issuedOperation.Nomenclature,
				IssuedSubdivisionOnOperation= issuedOperation,
				Cost = issuedOperation.CalculatePercentWear(Date),
				WearPercent = issuedOperation.CalculateDepreciationCost(Date)
			};

			ObservableItems.Add (newItem);
		}

		public virtual void AddItem(EmployeeIssueOperation issuedOperation, int count)
		{
			if(issuedOperation.Issued == 0)
				throw new InvalidOperationException("Этот метод можно использовать только с операциями выдачи.");

			if(Items.Any(p => DomainHelper.EqualDomainObjects(p.IssuedEmployeeOnOperation, issuedOperation))) {
				logger.Warn("Номенклатура из этой выдачи уже добавлена. Пропускаем...");
				return;
			}

			var newItem = new IncomeItem(this) {
				Amount = count,
				Nomenclature = issuedOperation.Nomenclature,
				IssuedEmployeeOnOperation = issuedOperation,
				Cost = issuedOperation.CalculateDepreciationCost(Date),
				WearPercent = issuedOperation.CalculatePercentWear(Date),
			};

			ObservableItems.Add(newItem);
		}

		public virtual IncomeItem AddItem(Nomenclature nomenclature)
		{
			if (Operation != IncomeOperations.Enter)
				throw new InvalidOperationException ("Добавление номенклатуры возможно только во входящую накладную. Возвраты должны добавляться с указанием строки выдачи.");
				
			var newItem = new IncomeItem (this) {
				Amount = 1,
				Nomenclature = nomenclature,
				Cost = 0,
			};

			ObservableItems.Add (newItem);
			return newItem;
		}

		public virtual IncomeItem AddItem(Nomenclature nomenclature, string growth, string size, int amount = 0)
		{
			if(Operation != IncomeOperations.Enter)
				throw new InvalidOperationException("Добавление номенклатуры возможно только во входящую накладную. Возвраты должны добавляться с указанием строки выдачи.");
			var item = ObservableItems.FirstOrDefault(i => i.Nomenclature.Id == nomenclature.Id && i.WearGrowth == growth && i.Size == size);
			if(item == null) {
				item = new IncomeItem(this) {
					Amount = amount,
					Nomenclature = nomenclature,
					WearGrowth = growth,
					Size = size,
					Cost = 0,
				};
				ObservableItems.Add(item);
			}
			else {
				item.Amount++;
			}
			return item;
		}


		public virtual void RemoveItem(IncomeItem item)
		{
			ObservableItems.Remove(item);
		}

		public virtual void UpdateOperations(IUnitOfWork uow, IInteractiveQuestion askUser)
		{
			Items.ToList().ForEach(x => x.UpdateOperations(uow, askUser));
		}

		public virtual void UpdateEmployeeWearItems()
		{
			EmployeeCard.UpdateNextIssue(Items.Select(x => x.Nomenclature.Type).ToArray());
			EmployeeCard.FillWearRecivedInfo(UoW);
			UoW.Save(EmployeeCard);
		}
	}

	public enum IncomeOperations {
		/// <summary>
		/// Приходная накладная
		/// </summary>
		[Display(Name = "Приходная накладная")]
		Enter,
		/// <summary>
		/// Возврат от работника
		/// </summary>
		[Display(Name = "Возврат от работника")]
		Return,
		/// <summary>
		/// Возврат с объекта
		/// </summary>
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

