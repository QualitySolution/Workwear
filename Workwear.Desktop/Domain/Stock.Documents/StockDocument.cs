using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Project.Domain;

namespace Workwear.Domain.Stock.Documents
{
	public class StockDocument : BusinessObjectBase<StockDocument>, IDomainObject
	{
		public virtual int Id { get; set; }

		private string docNumber;
		[StringLength(15)]
		[Display (Name = "Пользовательский номер документа")]
		public virtual string DocNumber {
			get => docNumber;
			set => SetField (ref docNumber, value);
		}
		
		[StringLength(15)]
		[Display (Name = "Номер документа")]
		public virtual string DocNumberText {
			get => String.IsNullOrWhiteSpace(DocNumber) ? Id.ToString() : DocNumber;
			set => DocNumber = value == Id.ToString() ? docNumber : value; 
		}
		
		DateTime date = DateTime.Now;
		[Display(Name = "Дата")]
		public virtual DateTime Date {
			get { return date; }
			set { SetField(ref date, value, () => Date); }
		}

		UserBase createdbyUser;
		[Display(Name = "Документ создал")]
		public virtual UserBase CreatedbyUser {
			get { return createdbyUser; }
			set { SetField(ref createdbyUser, value, () => CreatedbyUser); }
		}

		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField(ref comment, value, () => Comment); }
		}
		public StockDocument() { }

		private DateTime? creationDate = DateTime.Now;
		[Display(Name = "Дата создания")]
		public virtual DateTime? CreationDate {
			get => creationDate;
			set => SetField(ref creationDate, value);
		}

		public static Type GetDocClass(StockDocumentType docType)
		{
			switch (docType)
			{
				case StockDocumentType.Income:
					return typeof(Income);
				case StockDocumentType.Return:
					return typeof(Return);
				case StockDocumentType.ExpenseEmployeeDoc:
					return typeof(Expense);
				case StockDocumentType.ExpenseDutyNormDoc:
					return typeof(ExpenseDutyNorm);
				case StockDocumentType.CollectiveExpense:
					return typeof(CollectiveExpense);
				case StockDocumentType.WriteoffDoc:
					return typeof(Writeoff);
				case StockDocumentType.TransferDoc:
					return typeof(Transfer);
				case StockDocumentType.Completion:
					return typeof(Completion);
				case StockDocumentType.InspectionDoc:
					return typeof(Inspection);
			}
			throw new NotSupportedException();
		}
	}

	public enum StockDocumentType
	{
		[Display(Name = "Поступление на склад")]
		Income,
		[Display(Name = "Выдача сотруднику")]
		ExpenseEmployeeDoc,
		[Display(Name = "Коллективная выдача")]
		CollectiveExpense,
		[Display(Name = "Выдача по дежурной норме")]
		ExpenseDutyNormDoc,
		[Display(Name = "Возврат от сотрудника")]
		Return,
		[Display(Name = "Перемещение")]
		TransferDoc,
		[Display(Name = "Списание")]
		WriteoffDoc,
		[Display(Name = "Комплектация")]
		Completion,
		[Display(Name = "Оценка")]
		InspectionDoc
	}
}
