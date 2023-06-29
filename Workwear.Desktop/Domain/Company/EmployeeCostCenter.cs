using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using QS.Utilities.Numeric;

namespace Workwear.Domain.Company {
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "МВЗ сотрудника",
		Nominative = "МВЗ сотрудника",
		Genitive = "МВЗ сотрудника"
	)]
	
	[HistoryTrace]
	public class EmployeeCostCenter  : PropertyChangedBase, IDomainObject{
		#region Свойства

		public EmployeeCostCenter() {}

		public EmployeeCostCenter(EmployeeCard employee, CostCenter сostCenter, decimal percent) {
			this.employee = employee;
			this.сostCenter = сostCenter;
			this.percent = percent;
		}
		
		public virtual int Id { get; set; }

		public virtual string Title => $"МВЗ сотрудника {Employee.ShortName} - {CostCenter.Name}";
		
		private EmployeeCard employee;
		[Display(Name = "Сотрудник")]
		public virtual EmployeeCard Employee {
			get { return employee; }
			set { SetField(ref employee, value); }
		}
		
		private CostCenter сostCenter;
		[Display(Name = "МВЗ")]
		public virtual CostCenter CostCenter {
			get { return сostCenter; }
			set { SetField(ref сostCenter, value); }
		}
		private decimal percent;
		[Display(Name = "Доля затрат")]
		public virtual decimal Percent {
			get => percent;
			set => SetField(ref percent, value.Clamp(0m, 1m));
		}

		#endregion
	}
}
