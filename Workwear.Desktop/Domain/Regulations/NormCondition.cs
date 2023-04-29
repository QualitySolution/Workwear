using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using QS.Utilities.Dates;
using Workwear.Domain.Company;

namespace Workwear.Domain.Regulations
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "условия нормы",
		Nominative = "условие нормы",
		Genitive = "условия нормы"
		)]
	[HistoryTrace]
	public class NormCondition : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		private string name;
		[Required(ErrorMessage = "Название является обязательным")]
		[Display(Name = "Название")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value);
		}

		private SexNormCondition sex;
		[Display(Name = "Ограничение по полу")]
		public virtual SexNormCondition SexNormCondition {
			get => sex;
			set => SetField(ref sex, value);
		}

		private DateTime? issuanceStart;
		[Display(Name = "Начало выдачи")]
		public virtual DateTime? IssuanceStart {
			get => issuanceStart;
			set => SetField(ref issuanceStart, value);
		}

		private DateTime? issuanceEnd;
		[Display(Name = "Окончание выдачи")]
		public virtual DateTime? IssuanceEnd {
			get => issuanceEnd;
			set => SetField(ref issuanceEnd, value);
		}

		#region Методы 

		public virtual bool MatchesForEmployee(EmployeeCard employee)
		{
			switch(employee.Sex) {
				case Workwear.Domain.Company.Sex.F:
					return SexNormCondition == SexNormCondition.OnlyWomen || SexNormCondition == SexNormCondition.ForAll;
				case Workwear.Domain.Company.Sex.M:
					return SexNormCondition == SexNormCondition.OnlyMen || SexNormCondition == SexNormCondition.ForAll;
				case Workwear.Domain.Company.Sex.None:
					return SexNormCondition == SexNormCondition.ForAll;
				default:
					throw new NotSupportedException("Unknown sex value.");
			}
		}

		public virtual DateRange CalculateCurrentPeriod(DateTime dateFrom) {
			if (IssuanceStart is null || IssuanceEnd is null)
		 		throw new ArgumentException("В условиях нормы не заданы даты");
		    
		    var end = new DateTime(dateFrom.Year, IssuanceEnd.Value.Month, IssuanceEnd.Value.Day);
		    if (end < dateFrom)
			    end = end.AddYears(1);
		    var start = new DateTime(end.Year, IssuanceStart.Value.Month, IssuanceStart.Value.Day);
		    if (start > end)
			      start = start.AddYears(-1);
		    return new DateRange(start, end);
		}
		#endregion
		#region IValidatableObject implementation
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(IssuanceStart != null && IssuanceEnd is null)
				yield return new ValidationResult ("Если указана дата начала периода, то должна быть указана и дата окончания");
			if(IssuanceStart is null && IssuanceEnd != null)
				yield return new ValidationResult ("Если указана дата окончания периода, то должна быть указана и дата начала");
		}
		#endregion
	}

	public enum SexNormCondition
	{
		[Display(Name = "Для всех")]
		ForAll,
		[Display(Name = "Только мужчинам")]
		OnlyMen,
		[Display(Name = "Только женщинам")]
		OnlyWomen
	}
}
