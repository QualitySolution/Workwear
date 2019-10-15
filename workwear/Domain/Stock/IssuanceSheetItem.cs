﻿using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Regulations;

namespace workwear.Domain.Stock
{
	[Appellative(Gender = GrammaticalGender.Feminine,
	NominativePlural = "строки ведомости",
	Nominative = "строка ведомости")]
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

		private decimal periodCount;

		[Display(Name = "Срок службы в периодах")]
		public virtual decimal PeriodCount {
			get { return periodCount; }
			set { SetField(ref periodCount, value); }
		}

		private NormPeriodType periodType;

		[Display(Name = "Тип периода")]
		public virtual NormPeriodType PeriodType {
			get { return periodType; }
			set { SetField(ref periodType, value); }
		}

		#endregion

		public IssuanceSheetItem()
		{
		}
	}
}
