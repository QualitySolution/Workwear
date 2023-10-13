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
		
		#region Свойства

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
		[Display(Name = "Коментарийц")]
		public virtual string Comment {
			get { return comment; }
			set { SetField(ref comment, value); }
		}
		#endregion
	}
}
