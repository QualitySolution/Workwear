using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace workwear.Domain.Stock
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "строки выдачи",
		Nominative = "строка выдачи")]
	public class ExpenseItem : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		Expense expenseDoc;

		[Display (Name = "Документ")]
		public virtual Expense ExpenseDoc {
			get { return expenseDoc; }
			set { SetField (ref expenseDoc, value, () => ExpenseDoc); }
		}

		Nomenclature nomenclature;

		[Display (Name = "Номеклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField (ref nomenclature, value, () => Nomenclature); }
		}

		IncomeItem incomeOn;

		[Display (Name = "Операция прихода")]
		public virtual IncomeItem IncomeOn {
			get { return incomeOn; }
			set { SetField (ref incomeOn, value, () => IncomeOn); }
		}
			
		int amount;

		[Display (Name = "Количество")]
		public virtual int Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount); }
		}

		DateTime? autoWriteoffDate;

		[Display (Name = "День автосписания")]
		public virtual DateTime? AutoWriteoffDate {
			get { return autoWriteoffDate; }
			set { SetField (ref autoWriteoffDate, value, () => AutoWriteoffDate); }
		}

		#endregion


		public ExpenseItem ()
		{
		}

	}
}

