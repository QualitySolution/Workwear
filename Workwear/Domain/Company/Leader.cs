using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Utilities.Text;

namespace workwear.Domain.Company
{
	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "должностные лица",
		Nominative = "должностное лицо")]
	public class Leader : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private string surname;

		[Display(Name = "Фамилия")]
		[StringLength(50)]
		public virtual string Surname {
			get { return surname; }
			set { SetField(ref surname, value); }
		}

		string name;

		[Display (Name = "Имя")]
		[StringLength(50)]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		private string patronymic;

		[Display(Name = "Отчество")]
		[StringLength(50)]
		public virtual string Patronymic {
			get { return patronymic; }
			set { SetField(ref patronymic, value); }
		}

		private string position;

		[Display(Name = "Должность")]
		[StringLength(150)]
		public virtual string Position {
			get { return position; }
			set { SetField(ref position, value); }
		}

		private EmployeeCard employee;
		[Display(Name = "Карточка сотрудника")]
		public virtual EmployeeCard Employee {
			get => employee;
			set => SetField(ref employee, value);
		}

		#endregion

		public virtual string Title => PersonHelper.PersonNameWithInitials(Surname, Name, Patronymic);
		public Leader ()
		{
		}
	}
}

