using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using QS.Project.Domain;
using Workwear.Domain.Company;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Domain.Visits {
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "заявки на выдачу",
		Nominative = "заявка на выдачу",
		Genitive = "заявки на выдачу",
		GenitivePlural = "заявок на выдачу")]
	[HistoryTrace]
	public class IssuanceRequest: PropertyChangedBase, IDomainObject {
		#region Хранимые свойства

		public virtual int Id { get; set; }
		
		private DateTime receiptDate = DateTime.Now;
		[Display(Name = "Дата поступления заявки")]
		public virtual DateTime ReceiptDate {
			get => receiptDate;
			set => SetField(ref receiptDate, value);
		}

		private IssuanceRequestStatus status;
		[Display(Name = "Статус")]
		public virtual IssuanceRequestStatus Status {
			get => status;
			set => SetField(ref status, value);
		}

		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}
		private UserBase createdByUser;
		[Display(Name = "Пользователь, создавший заявку")]
		public virtual UserBase CreatedByUser {
			get => createdByUser;
			set => SetField(ref createdByUser, value);
		} 
		private DateTime creationDate;
		[Display(Name = "Дата создания заявки")]
		public virtual DateTime CreationDate {
			get => creationDate;
			set => SetField(ref creationDate, value);
		}
		#endregion

		#region Коллекции

		private IObservableList<EmployeeCard> employees = new ObservableList<EmployeeCard>();
		[Display(Name = "Сотрудники")]
		public virtual IObservableList<EmployeeCard> Employees {
			get => employees;
			set => SetField(ref employees, value);
		}

		private IObservableList<CollectiveExpense> collectiveExpenses = new ObservableList<CollectiveExpense>();
		[Display(Name = "Документы коллективной выдачи")]
		public virtual IObservableList<CollectiveExpense> CollectiveExpenses {
			get => collectiveExpenses;
			set => SetField(ref collectiveExpenses, value);
		}
		#endregion

		#region Генерируемые
		public virtual string Title => $"Заявка на выдачу №{Id} от {ReceiptDate:d}";
		#endregion
	}
	public enum IssuanceRequestStatus {
		[Display(Name = "Новая")]
		New,
		[Display(Name = "Выдано")]
		Issued,
		[Display(Name = "Частично выдано")]
		PartiallyIssued
	}
}
