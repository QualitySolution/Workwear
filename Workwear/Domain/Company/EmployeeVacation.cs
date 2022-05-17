using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using workwear.Repository.Operations;
using Workwear.Tools;

namespace workwear.Domain.Company
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "отпуска сотрудника",
		Nominative = "отпуск сотрудника",
		Genitive = "отпуска сотрудника"
		)]
	[HistoryTrace]
	public class EmployeeVacation : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private EmployeeCard employee;

		[Display(Name = "Сотрудник")]
		public virtual EmployeeCard Employee {
			get { return employee; }
			set { SetField(ref employee, value); }
		}

		private VacationType vacationType;

		[Display(Name = "Вид отпуска")]
		public virtual VacationType VacationType {
			get { return vacationType; }
			set { SetField(ref vacationType, value); }
		}

		private DateTime beginDate;

		[Display(Name = "Дата начала")]
		public virtual DateTime BeginDate {
			get { return beginDate; }
			set { SetField(ref beginDate, value); }
		}

		private DateTime endDate;

		[Display(Name = "Дата окончания")]
		public virtual DateTime EndDate {
			get { return endDate; }
			set { SetField(ref endDate, value); }
		}

		private string comments;

		[Display(Name = "Комментарий")]
		public virtual string Comments {
			get { return comments; }
			set { SetField(ref comments, value); }
		}

		#endregion

		#region Расчетные

		public virtual TimeSpan VacationTime => (EndDate.AddDays(1) - BeginDate);

		public virtual string Title => $"Отпуск {Employee.ShortName} c {BeginDate:d} по {EndDate:d}";

		#endregion

		#region Публичные методы

		public virtual void UpdateRelatedOperations(IUnitOfWork uow, EmployeeIssueRepository employeeIssueRepository, BaseParameters baseParameters, IInteractiveQuestion askUser)
		{
			Employee.RecalculateDatesOfIssueOperations(uow, employeeIssueRepository, baseParameters, askUser, BeginDate, EndDate);
		}

		#endregion

		#region IValidatableObject implementation
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(BeginDate > EndDate)
				yield return new ValidationResult("Дата окончания отпуска не может быть меньше даты начала отпуска.", new[] { nameof(BeginDate), nameof(EndDate) });
			if(VacationType == null)
				yield return new ValidationResult("Вид отпуска должен быть указан", new[] { nameof(VacationType) });
		}
		#endregion

		public EmployeeVacation()
		{
		}

	}
}