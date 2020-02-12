using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Project.Domain;
using workwear.Domain.Company;

namespace workwear.Domain.Stock
{
	public class MassExpense : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }
		`
		DateTime date;

		public virtual DateTime Date {
			get { return date; }
			set { SetField(ref date, value); }
		}

		UserBase createdbyUser;

		[Display(Name = "Документ создал")]
		public virtual UserBase User {
			get { return createdbyUser; }
			set { SetField(ref createdbyUser, value); }
		}

		Warehouse warehouseoperation;

		[Display(Name = "Склад выдачи")]
		public virtual Warehouse Warehouseoperation {
			get { return warehouseoperation; }
			set { SetField(ref warehouseoperation, value); }
		}

		private string comment;

		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField(ref comment, value); }
		}

		private IList<EmployeeCard> employees = new List<EmployeeCard>();

		[Display(Name = "Сотрудники")]
		public virtual IList<EmployeeCard> Employees {
			get { return employees; }
			set { SetField(ref employees, value); }
		}

		private IList<MassExpenseIssuing> massExpenseIssuing;

		public virtual IList<MassExpenseIssuing> MassExpenseIssuing {
			get { return massExpenseIssuing; }
			set { SetField(ref massExpenseIssuing, value); }
		}


		private IList<MassExpenseOperation> massExpenseOperation;

		public virtual IList<MassExpenseOperation> MassExpenseOperation {
			get { return massExpenseOperation; }
			set { SetField(ref massExpenseOperation, value); }
		}
	}
}
