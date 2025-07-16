using System;
using System.Collections.Generic;
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

		public virtual IObservableList<Expense> ExpenseDocuments { get; set; }
		public virtual IObservableList<Writeoff> WriteoffDocuments { get; set; } 
		public virtual IObservableList<Return> ReturnDocuments { get; set; }

		#endregion

		#region Расчётные
		
        public virtual IEnumerable<StockDocument> Documents {
	        get => ExpenseDocuments
			        .Concat<StockDocument>(WriteoffDocuments)
			        .Concat(ReturnDocuments);
        }

		#endregion
	}
}
