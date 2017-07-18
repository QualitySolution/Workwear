using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;

namespace workwear.Domain.Stock
{
	public class StockDocument : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		DateTime date;

		[Display(Name = "Дата")]
		public virtual DateTime Date
		{
			get { return date; }
			set { SetField(ref date, value, () => Date); }
		}

		User createdbyUser;

		[Display(Name = "Документ создал")]
		public virtual User CreatedbyUser
		{
			get { return createdbyUser; }
			set { SetField(ref createdbyUser, value, () => CreatedbyUser); }
		}

		public StockDocument()
		{
		}

		public static Type GetDocClass(StokDocumentType docType)
		{
			switch (docType)
			{
				case StokDocumentType.IncomeDoc:
					return typeof(Income);
				case StokDocumentType.ExpenseDoc:
					return typeof(Expense);
				case StokDocumentType.WriteoffDoc:
					return typeof(Writeoff);
			}
			throw new NotSupportedException();
		}
	}

	public enum StokDocumentType
	{
		[Display(Name = "Приход на склад")]
		IncomeDoc,
		[Display(Name = "Расход со склада")]
		ExpenseDoc,
		[Display(Name = "Списание")]
		WriteoffDoc
	}
}
