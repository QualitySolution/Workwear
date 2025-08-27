using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.Project.Domain;
using Workwear.Domain.Company;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Domain.Visits {
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "заявки на выдачу",
		Nominative = "заявка на выдачу",
		Genitive = "заявки на выдачу",
		GenitivePlural = "заявок на выдачу")]
	public class IssuanceRequest: PropertyChangedBase, IDomainObject {
		#region Хранимые свойства

		private int id;
		public virtual int Id {
			get => id;
			set => SetField(ref id, value);
		}
		private DateTime receiptDate;
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
		public virtual IObservableList<EmployeeCard> Employees { get; set; }
		public virtual IObservableList<CollectiveExpense> CollectiveExpenses { get; set; }

		public enum IssuanceRequestStatus {
			[Display(Name = "Новая")]
			New,
			[Display(Name = "Выдано")]
			Issued,
			[Display(Name = "Частично выдано")]
			PartiallyIssued
		}
		#endregion
	}
}
