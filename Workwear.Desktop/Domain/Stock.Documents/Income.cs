using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Repository.Operations;

namespace Workwear.Domain.Stock.Documents
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "приходные документы",
		Nominative = "приходный документ",
		Genitive = "приходного документа"
		)]
	[HistoryTrace]
	public class Income : StockDocument, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		#region Свойства

		private IncomeOperations operation;
		[Display (Name = "Тип операции")]
		public virtual IncomeOperations Operation {
			get => operation;
			set { SetField (ref operation, value, () => Operation); }
		}

		private Warehouse warehouse;
		[Display(Name = "Склад")]
		[Required(ErrorMessage = "Склад должен быть указан.")]
		public virtual Warehouse Warehouse {
			get => warehouse;
			set { SetField(ref warehouse, value, () => Warehouse); }
		}

		private string number;
		[Display (Name = "Вх. номер")]
		public virtual string Number {
			get => number;
			set { SetField (ref number, value, () => Number); }
		}

		private EmployeeCard employeeCard;
		[Display (Name = "Сотрудник")]
		public virtual EmployeeCard EmployeeCard {
			get => employeeCard;
			set { SetField (ref employeeCard, value, () => EmployeeCard); }
		}

		private Subdivision subdivision;
		[Display (Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get => subdivision;
			set { SetField (ref subdivision, value, () => Subdivision); }
		}

		private IList<IncomeItem> items = new List<IncomeItem>();
		[Display (Name = "Строки документа")]
		public virtual IList<IncomeItem> Items {
			get => items;
			set { SetField (ref items, value, () => Items); }
		}

		private GenericObservableList<IncomeItem> observableItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<IncomeItem> ObservableItems => 
			observableItems ?? (observableItems = new GenericObservableList<IncomeItem>(Items));

		#endregion
		public virtual string Title{
			get{
				switch (Operation) {
				case IncomeOperations.Enter:
					return $"Приходная накладная №{Id} от {Date:d}";
				case IncomeOperations.Return:
					return $"Возврат от работника №{Id} от {Date:d}";
				case IncomeOperations.Object:
					return $"Возврат c подразделения №{Id} от {Date:d}";
				default:
					return null;
				}
			}
		}
		#region IValidatableObject implementation
		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext) {
			if (Date < new DateTime(2008, 1, 1))
				yield return new ValidationResult ("Дата должны указана (не ранее 2008-го)", 
					new[] { this.GetPropertyName (o => o.Date)});

			if(Operation == IncomeOperations.Object && Subdivision == null)
				yield return new ValidationResult ("Подразделение должно быть указано", 
					new[] { this.GetPropertyName (o => o.Subdivision)});

			if(Operation == IncomeOperations.Return && EmployeeCard == null)
				yield return new ValidationResult ("Сотрудник должен быть указан", 
					new[] { this.GetPropertyName (o => o.EmployeeCard)});

			if(Items.Count == 0)
				yield return new ValidationResult ("Документ должен содержать хотя бы одну строку.", 
					new[] { this.GetPropertyName (o => o.Items)});

			if(Items.Any (i => i.Amount <= 0))
				yield return new ValidationResult ("Документ не должен содержать строк с нулевым количеством.", 
					new[] { this.GetPropertyName (o => o.Items)});

			if (Items.Any(i => i.Certificate != null && i.Certificate.Length > 40))
				yield return new ValidationResult("Длина номера сертификата не может быть больше 40 символов.",
					new[] { this.GetPropertyName(o => o.Items) });
			
			if(Operation == IncomeOperations.Return && EmployeeCard != null)
				foreach (var item in items) {
					if(item.IssuedEmployeeOnOperation == null || item.IssuedEmployeeOnOperation.Employee != EmployeeCard)
						yield return new ValidationResult(
							$"{item.Nomenclature.Name}: номенклатура добавлена не из числящегося за данным сотрудником", 
							new[] { nameof(Items) });
				}
			
			if(Operation == IncomeOperations.Object && Subdivision != null)
				foreach (var item in items) {
					if(item.IssuedSubdivisionOnOperation == null || item.IssuedSubdivisionOnOperation.Subdivision != Subdivision)
						yield return new ValidationResult(
							$"{item.Nomenclature.Name}: номенклатура добавлена не из числящегося за данным подразделением", 
							new[] { nameof(Items) });
				}
			if(Operation == IncomeOperations.Return)
				foreach (var item in items) {
					if(item.Nomenclature == null)
						yield return new ValidationResult(
							$"Во всех строках должна быть выбрана номенклатура.", 
							new[] { nameof(Items) });
				}
		}

		#endregion
		public Income () { }

		#region Строки документа
		public virtual void AddItem(SubdivisionIssueOperation issuedOperation, int count) {
			if(issuedOperation.Issued == 0)
				throw new InvalidOperationException("Этот метод можно использовать только с операциями выдачи.");

			if(Items.Any (p => DomainHelper.EqualDomainObjects (p.IssuedSubdivisionOnOperation, issuedOperation))) {
				logger.Warn ("Номенклатура из этой выдачи уже добавлена. Пропускаем...");
				return;
			}

			var newItem = new IncomeItem(this) {
				Amount = count,
				Nomenclature = issuedOperation.Nomenclature,
				IssuedSubdivisionOnOperation= issuedOperation,
				Cost = issuedOperation.CalculatePercentWear(Date),
				WearPercent = issuedOperation.CalculateDepreciationCost(Date)
			};
			ObservableItems.Add (newItem);
		}
		public virtual IncomeItem AddItem(EmployeeIssueOperation issuedOperation, int count) {
			if(issuedOperation.Issued == 0)
				throw new InvalidOperationException("Этот метод можно использовать только с операциями выдачи.");

			if(Items.Any(p => DomainHelper.EqualDomainObjects(p.IssuedEmployeeOnOperation, issuedOperation))) {
				logger.Warn("Номенклатура из этой выдачи уже добавлена. Пропускаем...");
				return null;
			}
			var newItem = new IncomeItem(this) {
				Amount = count,
				Nomenclature = issuedOperation.Nomenclature,
				WearSize = issuedOperation.WearSize,
				Height = issuedOperation.Height,
				IssuedEmployeeOnOperation = issuedOperation,
				Cost = issuedOperation.CalculateDepreciationCost(Date),
				WearPercent = issuedOperation.CalculatePercentWear(Date),
			};

			ObservableItems.Add(newItem);
			return newItem;
		}
		public virtual IncomeItem AddItem(Nomenclature nomenclature, IInteractiveMessage message) {
			if (Operation != IncomeOperations.Enter)
				throw new InvalidOperationException ("Добавление номенклатуры возможно только во входящую накладную. " +
				                                     "Возвраты должны добавляться с указанием строки выдачи.");

			if(nomenclature.Type == null) {
				//Такого в принципе быть не должно. Но бывают поломанные базы, поэтому лучше сообщить пользователю причину.
				message.ShowMessage(ImportanceLevel.Error, "У добавляемой номенклатуры обязательно должен быть указан тип.");
				return null;
			}
			
			var newItem = new IncomeItem (this) {
				Amount = 1,
				Nomenclature = nomenclature,
				Cost = 0,
			};

			ObservableItems.Add (newItem);
			return newItem;
		}
		public virtual IncomeItem AddItem(
			Nomenclature nomenclature, 
			Size size, Size height, int amount = 0, 
			string certificate = null, decimal price = 0m, Owner owner = null)
		{
			if(Operation != IncomeOperations.Enter)
				throw new InvalidOperationException("Добавление номенклатуры возможно только во входящую накладную. " +
				                                    "Возвраты должны добавляться с указанием строки выдачи.");
			var item = FindItem(nomenclature, size, height, owner);
			if(item == null) {
				item = new IncomeItem(this) {
					Amount = amount,
					Nomenclature = nomenclature,
					WearSize = size,
					Height = height,
					Cost = price,
					Certificate = certificate,
					Owner = owner
				};
				ObservableItems.Add(item);
			}
			else {
				item.Amount+= amount;
			}
			return item;
		}
		public virtual void RemoveItem(IncomeItem item) {
			ObservableItems.Remove (item);
		}

		public virtual IncomeItem FindItem(Nomenclature nomenclature, Size size, Size height, Owner owner) => Items
			.FirstOrDefault(i => i.Nomenclature.Id == nomenclature.Id
			                     && i.Height == height && i.WearSize == size && i.Owner == owner);
		#endregion

		public virtual void UpdateOperations(IUnitOfWork uow, IInteractiveQuestion askUser) {
			Items.ToList().ForEach(x => x.UpdateOperations(uow, askUser));
		}

		public virtual void UpdateEmployeeWearItems() {
			EmployeeCard.FillWearReceivedInfo(new EmployeeIssueRepository(UoW));
			EmployeeCard.UpdateNextIssue(Items
				.Select(x => x.IssuedEmployeeOnOperation.ProtectionTools)
				.Where(x => x != null).Distinct().ToArray());
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
		/// Возврат с подразделения
		/// </summary>
		[Display(Name = "Возврат с подразделения")]
		Object
	}
}

