using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NHibernate;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using workwear.Domain.Operations.Graph;
using workwear.Repository.Operations;

namespace workwear.Domain.Organization
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "отпуска сотрудника",
		Nominative = "отпуск сотрудника")]
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

		public virtual TimeSpan VacationTime => EndDate - BeginDate;

		public virtual string Title => $"Отпуск {Employee.ShortName} c {BeginDate:d} по {EndDate:d}";

		#endregion

		#region Публичные методы

		public virtual void UpdateRelatedOperations(IUnitOfWork uow, Func<string, bool> askUser)
		{
			var operations = EmployeeIssueRepository.GetOperationsTouchDates(uow, Employee, BeginDate, EndDate,
				q => q.Fetch(SelectMode.Fetch, o => o.Nomenclature.Type)
			);
			foreach(var typeGroup in operations.GroupBy(o => o.Nomenclature.Type)) {
				var graph = IssueGraph.MakeIssueGraph(uow, Employee, typeGroup.Key);
				foreach(var operation in typeGroup.OrderBy(o => o.OperationTime)) {
					operation.RecalculateDatesOfIssueOperation(graph, askUser);
					uow.Save(operation);
				}
			}
		}

		#endregion

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(BeginDate > EndDate)
				yield return new ValidationResult("Дата окончания отпуска не может быть меньше даты начала отпуска.", new[] { nameof(BeginDate), nameof(EndDate) });
		}

		public EmployeeVacation()
		{
		}
	}
}
