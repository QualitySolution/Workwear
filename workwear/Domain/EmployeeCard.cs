using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using QSProjectsLib;

namespace workwear.Domain
{

	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "карточки сотрудников",
		Nominative = "карточка сотрудника")]
	public class EmployeeCard: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string cardNumber;

		[Display (Name = "Номер карточки")]
		public virtual string CardNumber {
			get { return cardNumber; }
			set { SetField (ref cardNumber, value, () => CardNumber); }
		}

		string name;

		[Display (Name = "Имя")]
		public virtual string FirstName {
			get { return name; }
			set { SetField (ref name, value, () => FirstName); }
		}

		string lastName;

		[Display (Name = "Фамилия")]
		public virtual string LastName {
			get { return lastName; }
			set { SetField (ref lastName, value, () => LastName); }
		}

		string patronymic;

		[Display (Name = "Отчество")]
		public virtual string Patronymic {
			get { return patronymic; }
			set { SetField (ref patronymic, value, () => Patronymic); }
		}

		Post post;

		[Display (Name = "Должность")]
		public virtual Post Post {
			get { return post; }
			set { SetField (ref post, value, () => Post); }
		}

		Leader leader;

		[Display (Name = "Руководитель")]
		public virtual Leader Leader {
			get { return leader; }
			set { SetField (ref leader, value, () => Leader); }
		}

		DateTime? hireDate;

		[Display (Name = "Дата поступления")]
		public virtual DateTime? HireDate {
			get { return hireDate; }
			set { SetField (ref hireDate, value, () => HireDate); }
		}

		DateTime? dismissDate;

		[Display (Name = "Дата увольнения")]
		public virtual DateTime? DismissDate {
			get { return dismissDate; }
			set { SetField (ref dismissDate, value, () => DismissDate); }
		}

		Sex sex;

		[Display (Name = "Пол")]
		public virtual Sex Sex {
			get { return sex; }
			set { SetField (ref sex, value, () => Sex); }
		}

		User createdbyUser;

		[Display (Name = "Карточку создал")]
		public virtual User CreatedbyUser {
			get { return createdbyUser; }
			set { SetField (ref createdbyUser, value, () => CreatedbyUser); }
		}
			
		Facility facility;

		[Display (Name = "Объект")]
		public virtual Facility Facility {
			get { return facility; }
			set { SetField (ref facility, value, () => Facility); }
		}

		User user;

		[Display (Name = "Пользователь")]
		public virtual User User {
			get { return user; }
			set { SetField (ref user, value, () => User); }
		}

		byte[] photo;

		[Display (Name = "Фотография")]
		public virtual byte[] Photo {
			get { return photo; }
			set { SetField (ref photo, value, () => Photo); }
		}
			
		#endregion

		#region Размеры одежды

		string wearGrowth;

		[Display (Name = "Рост одежды")]
		public virtual string WearGrowth { 
			get { return wearGrowth; } 
			set	{ SetField (ref wearGrowth, value, () => WearGrowth); }
		}

		string wearSizeStd;

		[Display (Name = "Стандарт размера одежды")]
		public virtual string WearSizeStd {
			get { return wearSizeStd; }
			set { SetField (ref wearSizeStd, value, () => WearSizeStd); }
		}

		string wearSize;

		[Display (Name = "Размер одежды")]
		public virtual string WearSize { 
			get { return wearSize; } 
			set	{ SetField (ref wearSize, value, () => WearSize); }
		}

		string shoesSizeStd;

		[Display (Name = "Стандарт размера обуви")]
		public virtual string ShoesSizeStd {
			get { return shoesSizeStd; }
			set { SetField (ref shoesSizeStd, value, () => ShoesSizeStd); }
		}

		string shoesSize;

		[Display (Name = "Размер обуви")]
		public virtual string ShoesSize { 
			get { return shoesSize; } 
			set	{ SetField (ref shoesSize, value, () => ShoesSize); }
		}

		string headdressSizeStd;

		[Display (Name = "Стандарт размера головного убора")]
		public virtual string HeaddressSizeStd {
			get { return headdressSizeStd; }
			set { SetField (ref headdressSizeStd, value, () => HeaddressSizeStd); }
		}

		string headdressSize;

		[Display (Name = "Размер головного убора")]
		public virtual string HeaddressSize { 
			get { return headdressSize; } 
			set	{ SetField (ref headdressSize, value, () => HeaddressSize); }
		}

		string glovesSizeStd;

		[Display (Name = "Стандарт размера одежды")]
		public virtual string GlovesSizeStd {
			get { return glovesSizeStd; }
			set { SetField (ref glovesSizeStd, value, () => GlovesSizeStd); }
		}

		string glovesSize;

		[Display (Name = "Размер одежды")]
		public virtual string GlovesSize { 
			get { return glovesSize; } 
			set	{ SetField (ref glovesSize, value, () => GlovesSize); }
		}

		#endregion

		public virtual string Title {
			get{ return StringWorks.PersonNameWithInitials (LastName, FirstName, Patronymic);
			}
		}

		public virtual string FullName {
			get { return String.Format ("{0} {1} {2}", LastName, FirstName, Patronymic).Trim (); }
		}

		public virtual string ShortName {
			get { return StringWorks.PersonNameWithInitials (LastName, FirstName, Patronymic); }
		}

		public EmployeeCard ()
		{
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (String.IsNullOrEmpty (FirstName) && String.IsNullOrEmpty (LastName) && String.IsNullOrEmpty (Patronymic))
				yield return new ValidationResult ("Должно быть заполнено хотя бы одно из следующих полей: " +
					"Фамилия, Имя, Отчество)", 
					new[] { this.GetPropertyName (o => o.FirstName), this.GetPropertyName (o => o.LastName), this.GetPropertyName (o => o.Patronymic) });

			if (Sex == Sex.None)
				yield return new ValidationResult ("Пол должен быть указан.", new[] { this.GetPropertyName (o => o.Sex) });
			
		}

		#endregion
	}

	public enum Sex{
		[Display(Name = "Нет")]
		None,
		[Display(Name = "Мужской")]
		M,
		[Display(Name = "Женский")]
		F
	}

	public class SexStringType : NHibernate.Type.EnumStringType
	{
		public SexStringType () : base (typeof(Sex))
		{
		}
	}

}

