using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using Workwear.Domain.Company;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Domain.Visits {
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "записи на посещение",
		Nominative = "запись на посещение",
		Genitive = "записи на посещение",
		GenitivePlural = "записей на посещение"
	)]
	
	public class Visit : PropertyChangedBase, IDomainObject {
		
		#region Хранимые свойства
		public virtual int Id { get; set; }
		
		private DateTime createDate = DateTime.Now;
		public virtual DateTime CreateDate {
			get => createDate;
			set => SetField(ref createDate, value);
		}

		private DateTime visitTime;
		public virtual DateTime VisitTime {
			get => visitTime;
			set => SetField(ref visitTime, value);
		}

		private EmployeeCard employee;
		public virtual EmployeeCard Employee {
			get => employee;
			set => SetField(ref employee, value);
		}

		private bool employeeCreate;
		public virtual bool EmployeeCreate {
			get => employeeCreate;
			set => SetField(ref employeeCreate, value);
		}

		private bool done;
		public virtual bool Done {
			get => done;
			set => SetField(ref done, value);
		}
		
		private bool cancelled;
		public virtual bool Cancelled {
			get => cancelled;
			set => SetField(ref cancelled, value);
		}
		
		private string comment;
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}

		private DateTime? timeStart;
		public virtual DateTime? TimeStart {
			get => timeStart;
			set => SetField(ref timeStart, value);
		}

		private DateTime? timeFinish;
		public virtual DateTime? TimeFinish {
			get => timeFinish;
			set => SetField(ref timeFinish, value);
		}

		private Status status;
		public virtual Status Status {
			get => status;
			set => SetField(ref status, value);
		}

		public virtual IObservableList<Expense> ExpenseDocuments { get; set; }
		public virtual IObservableList<Writeoff> WriteoffDocuments { get; set; } 
		public virtual IObservableList<Return> ReturnDocuments { get; set; }

		#endregion

		#region Sensitive
		public virtual bool SensitiveActionButtons => Status != Status.Done && Status != Status.Canceled && Status != Status.Missing;
		#endregion
		#region Расчётные
		public virtual string Title => $"Посещение {Employee?.ShortName} ({VisitTime.ToShortDateString()})";
		
        public virtual IEnumerable<StockDocument> Documents {
	        get => ExpenseDocuments
			        .Concat<StockDocument>(WriteoffDocuments)
			        .Concat(ReturnDocuments);
        }

		#endregion
	}

	public enum Status {
		[Display(Name ="Новая")]
		New,
		[Display(Name ="В очереди")]
		Queued,
		[Display(Name ="В обслуживании")]
		Serviced,
		[Display(Name ="Завершена")]
		Done,
		[Display(Name ="Отменена")]
		Canceled,
		[Display(Name ="Пропущена")]
		Missing
	}
}
