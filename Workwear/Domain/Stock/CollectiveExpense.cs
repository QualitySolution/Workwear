using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using workwear.Domain.Statements;
using workwear.Repository.Company;
using workwear.Repository.Operations;
using workwear.Tools;

namespace workwear.Domain.Stock
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "коллективные выдачи",
		Nominative = "коллективная выдача")]
	public class CollectiveExpense : StockDocument, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region Свойства

		private Warehouse warehouse;

		[Display(Name = "Склад")]
		[Required(ErrorMessage = "Склад должен быть указан.")]
		public virtual Warehouse Warehouse {
			get { return warehouse; }
			set { SetField(ref warehouse, value, () => Warehouse); }
		}

		private IList<CollectiveExpenseItem> items = new List<CollectiveExpenseItem>();

		[Display (Name = "Строки документа")]
		public virtual IList<CollectiveExpenseItem> Items {
			get { return items; }
			set { SetField (ref items, value, () => Items); }
		}

		System.Data.Bindings.Collections.Generic.GenericObservableList<CollectiveExpenseItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual System.Data.Bindings.Collections.Generic.GenericObservableList<CollectiveExpenseItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new System.Data.Bindings.Collections.Generic.GenericObservableList<CollectiveExpenseItem> (Items);
				return observableItems;
			}
		}

		private IssuanceSheet issuanceSheet;
		[Display(Name = "Связанная ведомость")]
		public virtual IssuanceSheet IssuanceSheet {
			get => issuanceSheet;
			set => SetField(ref issuanceSheet, value);
		}

		#endregion

		#region Расчетные
		public virtual string Title => $"Коллективная выдача №{Id} от {Date:d}";
		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (Date < new DateTime(2008, 1, 1))
				yield return new ValidationResult ("Дата должны указана (не ранее 2008-го)", 
					new[] { this.GetPropertyName (o => o.Date)});
					
			if(Items.All(i => i.Amount <= 0))
				yield return new ValidationResult ("Документ должен содержать хотя бы одну строку с количеством больше 0.", 
					new[] { this.GetPropertyName (o => o.Items)});

			if(Items.Any (i => i.Amount > 0 && i.Nomenclature == null))
				yield return new ValidationResult ("Документ не должен содержать строки без выбранной номенклатуры и с указанным количеством.", 
					new[] { this.GetPropertyName (o => o.Items)});
		}
		#endregion

		#region Добавление удаление строк

		public virtual CollectiveExpenseItem AddItem(StockPosition position, int amount = 1)
		{
			var newItem = new CollectiveExpenseItem() {
				Document = this,
				Amount = amount,
				Nomenclature = position.Nomenclature,
				Size = position.Size,
				WearGrowth = position.Growth,
				WearPercent = position.WearPercent
			};

			ObservableItems.Add(newItem);
			return newItem;
		}

		public virtual CollectiveExpenseItem AddItem(EmployeeCardItem employeeCardItem, BaseParameters baseParameters)
		{
			if(employeeCardItem == null)
				throw new ArgumentNullException(nameof(employeeCardItem));

			CollectiveExpenseItem newItem;
			if(employeeCardItem.BestChoiceInStock.Any())
				newItem = AddItem(employeeCardItem.BestChoiceInStock.First().StockPosition);
			else { 
				newItem = new CollectiveExpenseItem() {
					Document = this,
				};
				ObservableItems.Add(newItem);
			}

			newItem.EmployeeCardItem = employeeCardItem;
			newItem.ProtectionTools = employeeCardItem.ProtectionTools;

			if(Employee.GetUnderreceivedItems(baseParameters).Contains(employeeCardItem) && newItem.Nomenclature != null) 
				newItem.Amount = employeeCardItem.CalculateRequiredIssue(baseParameters);
			else newItem.Amount = 0;

			return newItem;
		}

		public virtual void RemoveItem(CollectiveExpenseItem item)
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

		public virtual void FillCanWriteoffInfo(IUnitOfWork uow)
		{
			var itemsBalance = EmployeeRepository.ItemsBalance(uow, Employee, Date);
			foreach(var item in Items) {
				item.IsWriteOff = item.EmployeeIssueOperation?.EmployeeOperationIssueOnWriteOff != null;
				item.IsEnableWriteOff = item.IsWriteOff || itemsBalance.Where(x => x.ProtectionToolsId == item.ProtectionTools?.Id).Sum(x => x.Amount) > 0;
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
			Employee.UpdateNextIssue(Items.Select(x => x.ProtectionTools).ToArray());
			Employee.FillWearRecivedInfo(new EmployeeIssueRepository(UoW));
			UoW.Save(Employee);
		}

		public virtual void CreateIssuanceSheet()
		{
			if(IssuanceSheet != null)
				return;

			IssuanceSheet = new IssuanceSheet {
				Expense = this
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

		#endregion
	}
}

