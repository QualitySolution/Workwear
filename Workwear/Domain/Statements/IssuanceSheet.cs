using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using QS.Utilities.Dates;
using QSReport;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Stock;

namespace workwear.Domain.Statements
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "ведомости на выдачу",
		Nominative = "ведомость на выдачу",
		Genitive = "ведомости на выдачу"
		)]
	[HistoryTrace]
	public class IssuanceSheet : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private DateTime date = DateTime.Today;

		[Display(Name = "Дата составления")]
		public virtual DateTime Date {
			get { return date; }
			set { SetField(ref date, value); }
		}

		private Organization organization;

		[Display(Name = "Организация")]
		public virtual Organization Organization {
			get { return organization; }
			set { SetField(ref organization, value); }
		}

		private Subdivision subdivision;

		[Display(Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get { return subdivision; }
			set { SetField(ref subdivision, value); }
		}

		private Expense expense;
		[Display(Name = "Документ выдачи")]
		public virtual Expense Expense {
			get => expense;
			set => SetField(ref expense, value);
		}

		private MassExpense massExpense;
		[Display (Name = "Документ выдачи списком")]
		public virtual MassExpense MassExpense {
			get => massExpense;
			set => SetField(ref massExpense, value);
		}

		private CollectiveExpense collectiveExpense;
		[Display(Name = "Документ коллективной выдачи")]
		public virtual CollectiveExpense CollectiveExpense {
			get => collectiveExpense;
			set => SetField(ref collectiveExpense, value);
		}

		#region Подписи

		private Leader responsiblePerson;

		[Display(Name = "Материально ответственное лицо")]
		public virtual Leader ResponsiblePerson {
			get { return responsiblePerson; }
			set { SetField(ref responsiblePerson, value); }
		}

		private Leader headOfDivisionPerson;

		[Display(Name = "Руководитель подразделения")]
		public virtual Leader HeadOfDivisionPerson {
			get { return headOfDivisionPerson; }
			set { SetField(ref headOfDivisionPerson, value); }
		}

		#endregion

		IList<IssuanceSheetItem> items = new List<IssuanceSheetItem>();
		[Display(Name = "Строки")]
		public virtual IList<IssuanceSheetItem> Items {
		    get => items;
			set => SetField(ref items, value);
		}

		GenericObservableList<IssuanceSheetItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<IssuanceSheetItem> ObservableItems {
		    get {
		        if(observableItems == null)
		            observableItems = new GenericObservableList<IssuanceSheetItem>(Items);
		        return observableItems;
			}
		}

		#endregion

		#region Вычисляемые свойства

		public virtual string Title => $"Ведомость №{Id}";

		#endregion

		#region Добавление строк

		public virtual IssuanceSheetItem AddItem(ExpenseItem expenseItem)
		{
			var item = new IssuanceSheetItem {
				IssuanceSheet = this,
				ExpenseItem = expenseItem
			};
			ObservableItems.Add(item);
			item.UpdateFromExpense();
			return item;
		}

		public virtual IssuanceSheetItem AddItem(CollectiveExpenseItem expenseItemItem)
		{
			var item = new IssuanceSheetItem {
				IssuanceSheet = this,
				CollectiveExpenseItem = expenseItemItem
			};
			ObservableItems.Add(item);
			item.UpdateFromCollectiveExpense();
			return item;
		}

		public virtual IssuanceSheetItem AddItem(MassExpenseEmployee massExpenseEmployee,
			   MassExpenseNomenclature massExpenseNomenclature,
			EmployeeIssueOperation employeeIssueOperation)
		{
			var item = new IssuanceSheetItem {
				IssuanceSheet = this,
				Employee = massExpenseEmployee.EmployeeCard,
				Nomenclature = massExpenseNomenclature.Nomenclature,
				Amount = (uint)massExpenseNomenclature.Amount,
				IssueOperation = employeeIssueOperation
			};

			ObservableItems.Add(item);
			item.UpdateFromMassExpense(employeeIssueOperation);
			return item;
		}

		public virtual void CleanObservableItems()
		{
			observableItems = null;
			OnPropertyChanged(nameof(ObservableItems));
		}

		public virtual IssuanceSheetItem AddItem(EmployeeIssueOperation operation)
		{
			var item = new IssuanceSheetItem {
				IssuanceSheet = this,
				IssueOperation = operation,
				Amount = (uint)operation.Issued,
				Employee = operation.Employee,
				Lifetime = operation.StartOfUse == null || operation.ExpiryByNorm == null ? 0 
					: new DateRange(operation.StartOfUse.Value, operation.ExpiryByNorm.Value).Months,
				Nomenclature = operation.Nomenclature,
				StartOfUse = operation.StartOfUse ?? operation.OperationTime
			};
			ObservableItems.Add(item);
			return item;
		}

		public virtual IssuanceSheetItem AddItem(EmployeeCardItem employeeItem)
		{
			var item = new IssuanceSheetItem {
				IssuanceSheet = this,
				ProtectionTools = employeeItem.ProtectionTools,
				Amount = (uint)employeeItem.ActiveNormItem.Amount,
				Employee = employeeItem.EmployeeCard,
				Lifetime = employeeItem.ActiveNormItem.PeriodInMonths,
				StartOfUse = employeeItem.NextIssue ?? Date,
			};
			ObservableItems.Add(item);
			return item;
		}
		#endregion

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			foreach(var item in Items) {
				if(item.Employee == null)
					yield return new ValidationResult($"Отсутствует сотрудник в строке [{item.Title}].",
					new[] { nameof(Items) });
			}

			if(Items.Any(i => i.Amount <= 0))
				yield return new ValidationResult("Документ не должен содержать номенклатур с нулевым количеством.",
					new[] { nameof(Items) });
		}

		public IssuanceSheet()
		{
		}
	}

	public enum IssuedSheetPrint
	{
		[Display(Name = "Альбомная")]
		[ReportIdentifier("Statements.IssuanceSheet")]
		IssuanceSheet,
		[Display(Name = "Книжная")]
		[ReportIdentifier("Statements.IssuanceSheetVertical")]
		IssuanceSheetVertical,
		[Display(Name = "Задание на сборку")]
		[ReportIdentifier("Statements.AssemblyTask")]
		AssemblyTask,
	}
}
