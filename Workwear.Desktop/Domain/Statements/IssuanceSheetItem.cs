﻿using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using QS.Utilities.Numeric;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Stock;

namespace Workwear.Domain.Statements
{
	[Appellative(Gender = GrammaticalGender.Feminine,
	NominativePlural = "строки ведомости",
	Nominative = "строка ведомости",
		Genitive = "строки ведомости"
		)]
	[HistoryTrace]
	public class IssuanceSheetItem : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private IssuanceSheet issuanceSheet;

		[Display(Name = "Ведомость")]
		public virtual IssuanceSheet IssuanceSheet {
			get { return issuanceSheet; }
			set { SetField(ref issuanceSheet, value); }
		}

		private EmployeeCard employee;

		[Display(Name = "Сотрудник")]
		public virtual EmployeeCard Employee {
			get { return employee; }
			set { SetField(ref employee, value); }
		}

		private Nomenclature nomenclature;

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField(ref nomenclature, value); }
		}

		private ProtectionTools protectionTools;
		[Display(Name = "Номенклатура нормы")]
		public virtual ProtectionTools ProtectionTools {
			get => protectionTools;
			set => SetField(ref protectionTools, value);
		}

		private ExpenseItem expenseItem;
		[Display(Name = "Строка выдачи")]
		public virtual ExpenseItem ExpenseItem {
			get => expenseItem;
			set => SetField(ref expenseItem, value);
		}

		private CollectiveExpenseItem collectiveExpenseItem;
		[Display(Name = "Строка коллективной выдачи")]
		public virtual CollectiveExpenseItem CollectiveExpenseItem {
			get => collectiveExpenseItem;
			set => SetField(ref collectiveExpenseItem, value);
		}

		private EmployeeIssueOperation issueOperation;

		[Display(Name = "Операция выдачи")]
		public virtual EmployeeIssueOperation IssueOperation {
			get { return issueOperation; }
			set { SetField(ref issueOperation, value); }
		}

		private uint amount;

		[Display(Name = "Количество")]
		public virtual uint Amount {
			get { return amount; }
			set { SetField(ref amount, value); }
		}

		private DateTime startOfUse;

		[Display(Name = "Дата поступления в эксплуатацию")]
		public virtual DateTime StartOfUse {
			get { return startOfUse; }
			set { SetField(ref startOfUse, value); }
		}

		private decimal lifetime;

		[Display(Name = "Срок службы")]
		public virtual decimal Lifetime {
			get { return lifetime; }
			set { SetField(ref lifetime, value.Clamp(0m, 999.99m)); }
		}
		private Size wearSize;
		[Display(Name = "Размер")]
		public virtual Size WearSize {
			get => wearSize;
			set => SetField(ref wearSize, value);
		}
		private Size height;
		[Display(Name = "Рост одежды")]
		public virtual Size Height {
			get => height;
			set => SetField(ref height, value);
		}

		#endregion

		#region Расчетные

		public virtual string ItemName => nomenclature?.Name ?? ProtectionTools?.Name;

		public virtual string Title {
			get {
				return String.Format("Строка ведомости {0} - {1} в количестве {2} {3}",
					Employee?.ShortName,
			  		ItemName,
			  		Amount,
			  		Nomenclature?.Type?.Units?.Name
		 		);
			}
		}

		#endregion

		public IssuanceSheetItem()
		{
		}

		#region Методы

		public virtual void UpdateFromExpense()
		{
			if(ExpenseItem == null)
				return;

			Employee = ExpenseItem.ExpenseDoc.Employee;
			Nomenclature = ExpenseItem.Nomenclature;
			Amount = (uint)ExpenseItem.Amount;
			WearSize = ExpenseItem.WearSize;
			Height = ExpenseItem.Height;
			StartOfUse = ExpenseItem.EmployeeIssueOperation?.StartOfUse ?? IssuanceSheet.Date;
			Lifetime = ExpenseItem.EmployeeIssueOperation?.LifetimeMonth ?? 0;
			IssueOperation = ExpenseItem.EmployeeIssueOperation;
		}

		public virtual void UpdateFromCollectiveExpense()
		{
			if(CollectiveExpenseItem == null)
				return;

			Employee = CollectiveExpenseItem.Employee;
			Nomenclature = CollectiveExpenseItem.Nomenclature;
			Amount = (uint)CollectiveExpenseItem.Amount;
			WearSize = CollectiveExpenseItem.WearSize;
			Height = CollectiveExpenseItem.Height;
			StartOfUse = CollectiveExpenseItem.EmployeeIssueOperation?.StartOfUse ?? IssuanceSheet.Date;
			Lifetime = CollectiveExpenseItem.EmployeeIssueOperation?.LifetimeMonth ?? 0;
			IssueOperation = CollectiveExpenseItem.EmployeeIssueOperation;
		}
		#endregion

	}
}
