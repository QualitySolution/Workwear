using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using workwear.Domain.Company;
using workwear.Domain.Statements;
using workwear.Domain.Users;
using workwear.Repository.Company;
using workwear.Repository.Operations;
using workwear.Repository.Stock;
using workwear.Tools;

namespace workwear.Domain.Stock
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "расходные документы",
		Nominative = "расходный документ",
		Genitive = "расходного документа"
		)]
	[HistoryTrace]
	public class Expense : StockDocument, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region Свойства

		ExpenseOperations operation;

		[Display (Name = "Тип операции")]
		public virtual ExpenseOperations Operation {
			get { return operation; }
			set { SetField (ref operation, value, () => Operation); }
		}

		EmployeeCard employee;

		[Display (Name = "Сотрудник")]
		public virtual EmployeeCard Employee {
			get { return employee; }
			set { SetField (ref employee, value, () => Employee); }
		}

		Subdivision subdivision;

		[Display (Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get { return subdivision; }
			set { SetField (ref subdivision, value, () => Subdivision); }
		}

		private IList<ExpenseItem> items = new List<ExpenseItem>();

		[Display (Name = "Строки документа")]
		public virtual IList<ExpenseItem> Items {
			get { return items; }
			set { SetField (ref items, value, () => Items); }
		}

		private Warehouse warehouse;

		[Display(Name = "Склад")]
		[Required(ErrorMessage = "Склад должен быть указан.")]
		public virtual Warehouse Warehouse {
			get { return warehouse; }
			set { SetField(ref warehouse, value, () => Warehouse); }
		}

		System.Data.Bindings.Collections.Generic.GenericObservableList<ExpenseItem> observableItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual System.Data.Bindings.Collections.Generic.GenericObservableList<ExpenseItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new System.Data.Bindings.Collections.Generic.GenericObservableList<ExpenseItem> (Items);
				return observableItems;
			}
		}

		private IssuanceSheet issuanceSheet;
		[Display(Name = "Связанная ведомость")]
		public virtual IssuanceSheet IssuanceSheet {
			get => issuanceSheet;
			set => SetField(ref issuanceSheet, value);
		}

		private Writeoff writeOffDoc;

		[Display(Name = "Связанный документ списания")]
		public virtual Writeoff WriteOffDoc {
			get { return writeOffDoc; }
			set { SetField(ref writeOffDoc, value); }
		}

		#endregion

		public virtual string Title{
			get{ return String.Format ("{0} №{1} от {2:d}", Operation.GetEnumTitle (), Id, Date);}
		}

		public Expense ()
		{
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (Date < new DateTime(2008, 1, 1))
				yield return new ValidationResult ("Дата должны указана (не ранее 2008-го)", 
					new[] { nameof(Date)});

			if(Operation == ExpenseOperations.Object && Subdivision == null)
				yield return new ValidationResult ("Подразделение должно быть указано", 
					new[] { this.GetPropertyName (o => o.Subdivision)});

			if(Operation == ExpenseOperations.Employee && Employee == null)
				yield return new ValidationResult ("Сотрудник должен быть указан", 
					new[] { this.GetPropertyName (o => o.Employee)});

			if(Items.All(i => i.Amount <= 0))
				yield return new ValidationResult ("Документ должен содержать хотя бы одну строку с количеством больше 0.", 
					new[] { nameof (Items)});

			if(Items.Any (i => i.Amount > 0 && i.Nomenclature == null))
				yield return new ValidationResult ("Документ не должен содержать строки без выбранной номенклатуры и с указанным количеством.", 
					new[] { nameof (Items)});

			//Проверка наличия на складе
			var baseParameters = (BaseParameters)validationContext.Items[nameof(BaseParameters)];
			if(UoW != null && baseParameters.CheckBalances) {
				var repository = new StockRepository();
				var nomenclatures = Items.Where(x => x.Nomenclature != null).Select(x => x.Nomenclature).Distinct().ToList();
				var excludeOperations = Items.Where(x => x.WarehouseOperation?.Id > 0).Select(x => x.WarehouseOperation).ToList();
				var balance = repository.StockBalances(UoW, Warehouse, nomenclatures, Date, excludeOperations);

				var positionGoups = Items.Where(x => x.Nomenclature != null).GroupBy(x => x.StockPosition);
				foreach(var position in positionGoups) {
					var amount = position.Sum(x => x.Amount);
					if(amount == 0)
						continue;

					var stockExist = balance.FirstOrDefault(x => x.StockPosition.Equals(position.Key));

					if(stockExist == null) {
						yield return new ValidationResult($"На складе отсутствует - {position.Key.Title}", new[] { nameof(Items) });
						continue;
					}

					if(stockExist.Amount < amount) {
						yield return new ValidationResult($"Недостаточное количество - {position.Key.Title}, Необходимо: {amount} На складе: {stockExist.Amount}", new[] { nameof(Items) });
						continue;
					}
				}
			}
		}
		#endregion

		#region Добавление удаление строк

		public virtual ExpenseItem AddItem(StockPosition position, int amount = 1)
		{
			var newItem = new ExpenseItem() {
				ExpenseDoc = this,
				Amount = amount,
				Nomenclature = position.Nomenclature,
				WearSize = position.WearSize,
				Height = position.Height,
				WearPercent = position.WearPercent
			};

			ObservableItems.Add(newItem);
			return newItem;
		}

		public virtual ExpenseItem AddItem(EmployeeCardItem employeeCardItem, BaseParameters baseParameters)
		{
			if(employeeCardItem == null)
				throw new ArgumentNullException(nameof(employeeCardItem));

			ExpenseItem newItem;
			if(employeeCardItem.BestChoiceInStock.Any())
				newItem = AddItem(employeeCardItem.BestChoiceInStock.First().StockPosition);
			else { 
				newItem = new ExpenseItem() {
					ExpenseDoc = this,
				};
				ObservableItems.Add(newItem);
			}

			newItem.EmployeeCardItem = employeeCardItem;
			newItem.ProtectionTools = employeeCardItem.ProtectionTools;

			if(Employee.GetUnderreceivedItems(baseParameters).Contains(employeeCardItem) && newItem.Nomenclature != null && newItem.ProtectionTools?.Type.IssueType == IssueType.Personal) 
				newItem.Amount = employeeCardItem.CalculateRequiredIssue(baseParameters);
			else newItem.Amount = 0;

			return newItem;
		}

		public virtual void RemoveItem(ExpenseItem item)
		{
			ObservableItems.Remove (item);
		}

		public virtual void CleanupItems()
		{
			foreach(var item in Items.Where(x => x.Amount <= 0).ToList()) {
				RemoveItem(item);
			}
		}

		#endregion

		#region Методы
		public virtual void CleanupItemsWriteOff()
		{
			if(this.WriteOffDoc == null) return;

			var toRemove = WriteOffDoc.Items
				.Where(writeoffItem => !Items.Any(expenseItem => DomainHelper.EqualDomainObjects(writeoffItem.EmployeeWriteoffOperation, expenseItem.EmployeeIssueOperation.EmployeeOperationIssueOnWriteOff))).ToList();
			foreach(var item in toRemove) {
				this.WriteOffDoc.RemoveItem(item);
			}
		}

		public virtual void FillCanWriteoffInfo(IUnitOfWork uow)
		{
			var itemsBalance = EmployeeRepository.ItemsBalance(uow, Employee, Date);
			foreach(var item in Items) {
				item.IsWriteOff = item.EmployeeIssueOperation?.EmployeeOperationIssueOnWriteOff != null;
				item.IsEnableWriteOff = itemsBalance.Where(x => x.ProtectionToolsId == item.ProtectionTools?.Id).Any(x => x.NomenclatureId != null);
				if(item.IsEnableWriteOff)
					item.IsEnableWriteOff = item.IsWriteOff || 
				                        itemsBalance.Where(x => x.ProtectionToolsId == item.ProtectionTools?.Id).Sum(x => x.Amount) > 0;
				if(WriteOffDoc != null) {
					var relatedWriteoffItem = WriteOffDoc.Items
					.FirstOrDefault(x => item.EmployeeIssueOperation.EmployeeOperationIssueOnWriteOff.IsSame(x.EmployeeWriteoffOperation));
					if(relatedWriteoffItem != null)
						item.AktNumber = relatedWriteoffItem.AktNumber;
				}

			}
		}

		public virtual void UpdateOperations(IUnitOfWork uow, BaseParameters baseParameters, IInteractiveQuestion askUser, string signCardUid = null)
		{
			Items.ToList().ForEach(x => x.UpdateOperations(uow, baseParameters, askUser, signCardUid));
		}

		public virtual void UpdateEmployeeWearItems()
		{
			Employee.UpdateNextIssue(Items.Where(x => x.ProtectionTools != null).Select(x => x.ProtectionTools).ToArray());
			Employee.FillWearRecivedInfo(new EmployeeIssueRepository(UoW));
			UoW.Save(Employee);
		}

		public virtual void CreateIssuanceSheet(UserSettings userSettings)
		{
			if(IssuanceSheet != null)
				return;

			IssuanceSheet = new IssuanceSheet {
				Expense = this,
				Organization = userSettings?.DefaultOrganization,
				HeadOfDivisionPerson = userSettings?.DefaultLeader,
				ResponsiblePerson = userSettings?.DefaultResponsiblePerson,
			};
			UpdateIssuanceSheet();
		}

		public virtual void UpdateIssuanceSheet()
		{
			if(IssuanceSheet == null)
				return;

			if(Employee == null)
				throw new NullReferenceException("Для обновления ведомости сотрудник должен быть указан.");

			IssuanceSheet.Date = Date;
			if(Employee.Subdivision != null)
				IssuanceSheet.Subdivision = Employee.Subdivision;

			foreach(var item in Items.ToList()) {
				if(item.IssuanceSheetItem == null && item.Amount > 0) 
					item.IssuanceSheetItem = IssuanceSheet.AddItem(item);

				if(item.IssuanceSheetItem != null)
					item.IssuanceSheetItem.UpdateFromExpense();

				if(item.IssuanceSheetItem != null && item.Amount == 0)
					IssuanceSheet.Items.Remove(item.IssuanceSheetItem);
			}
		}

		public virtual void UpdateIssuedWriteOffOperation()
		{
			if(WriteOffDoc == null)
				return;

			WriteOffDoc.Items.ToList().ForEach(x => x.UpdateOperations(UoW));

			if(WriteOffDoc.Items.Count < 1) {
				UoW.Delete(WriteOffDoc);
				WriteOffDoc = null;

			}

		}

		#endregion
	}

	public enum ExpenseOperations {
		[Display(Name = "Выдача сотруднику")]
		Employee,
		[Display(Name = "Выдача на подразделение")]
		Object
	}

	public class ExpenseOperationsType : NHibernate.Type.EnumStringType
	{
		public ExpenseOperationsType () : base (typeof(ExpenseOperations))
		{
		}
	}
}

