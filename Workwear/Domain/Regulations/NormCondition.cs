using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using QS.Utilities.Dates;
using workwear.Domain.Company;

namespace workwear.Domain.Regulations
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "условия нормы",
		Nominative = "условие нормы",
		Genitive = "условия нормы"
		)]
	[HistoryTrace]
	public class NormCondition : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private string name;
		[Display(Name = "Название")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value);
		}

		private SexNormCondition sex;
		public virtual SexNormCondition SexNormCondition {
			get => sex;
			set => SetField(ref sex, value);
		}

		private DateTime? issuanceStart;
		public virtual DateTime? IssuanceStart {
			get => issuanceStart;
			set => SetField(ref issuanceStart, value);
		}

		private DateTime? issuanceEnd;
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
				default:
					return false;
			}
		}

		 public DateRange CalculateDatesNextPeriod() {
		 	if (IssuanceStart is null || IssuanceEnd is null)
		 		throw new ArgumentException("Даты периода не заданы");
		 	var today = DateTime.Today;
		    var start = new DateTime(today.Year, IssuanceStart.Value.Month, IssuanceStart.Value.Day);
		    if (today > start)
			    start = start.AddYears(1);
		    var end = new DateTime(start.Year, IssuanceEnd.Value.Month, IssuanceEnd.Value.Day);
		    if (start > end)
			    end = end.AddYears(1);
		    return new DateRange(start, end);
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