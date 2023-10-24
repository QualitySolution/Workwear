using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Workwear.Domain.Company {
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "члены группы",
		Nominative = "член группы",
		Genitive = "члена группы"
	)]
	[HistoryTrace]
	public class EmployeeGroupItem : PropertyChangedBase, IDomainObject{
		
		public EmployeeGroupItem(){}
		
		#region Генерируемые Свойства
		public virtual string Title => $"{Employee.FullName} в группе {Group.Name}";
		public virtual string FullName => $"{Employee?.LastName} {Employee?.FirstName} {Employee?.Patronymic}".Trim();
		public virtual string EmployeePersonnelNumber => Employee?.PersonnelNumber;
		public virtual string CardNumberText => Employee?.CardNumber ?? Employee?.Id.ToString();
		public virtual bool Dismiss => Employee?.DismissDate != null;
		#endregion
		
		#region Хранимые Свойства
		public virtual int Id { get; set; }

		private EmployeeGroup group;
		[Display (Name = "Группа")]
		public virtual EmployeeGroup Group {
			get { return group; }
			set { SetField(ref group, value); }
		}
		
		private EmployeeCard employee;
		[Display (Name = "Член группы")]
		public virtual EmployeeCard Employee {
			get { return employee; }
			set { SetField(ref employee, value); }
		}
		
		private string comment;
		[Display(Name = "Коментарий")]
		public virtual string Comment {
			get => String.IsNullOrWhiteSpace(comment) ? null : comment;  //Чтобы в базе хранить null, а не пустую строку. 
			set => SetField(ref comment, value);
		}
		#endregion
	}
}
