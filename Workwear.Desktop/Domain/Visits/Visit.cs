using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using Workwear.Domain.Company;
using Workwear.Domain.Stock.Documents;

namespace Workwear.Domain.Visits {
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "записи на посещение",
		Nominative = "запись на посищение",
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

		private DateTime visitDate;
		public virtual DateTime VisitDate {
			get => visitDate;
			set => SetField(ref visitDate, value);
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

		public virtual List<Expense> ExpenseDocuments { get; set; } = new List<Expense>();
		public virtual List<Writeoff> WriteoffDocuments { get; set; } = new List<Writeoff>();
		public virtual List<Return> ReturnDocuments { get; set; } = new List<Return>();

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
