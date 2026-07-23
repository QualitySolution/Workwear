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

		public EmployeeCostCenter(EmployeeCard employee, CostCenter costCenter, decimal percent) {
			this.employee = employee;
			this.costCenter = costCenter;
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
		
		private CostCenter costCenter;
		[Display(Name = "МВЗ")]
		public virtual CostCenter CostCenter {
			get { return costCenter; }
			set { SetField(ref costCenter, value); }
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
