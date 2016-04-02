using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;

namespace workwear.Domain
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "объекты",
		Nominative = "объект")]
	
	public class Facility : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Display (Name = "Название")]
		[Required (ErrorMessage = "Название должно быть заполнено.")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		string address;

		[Display (Name = "Адрес")]
		public virtual string Address {
			get { return address; }
			set { SetField (ref address, value, () => Address); }
		}

		private IList<EmployeeCardItem> places = new List<EmployeeCardItem>();

		[Display (Name = "Места размещения")]
		public virtual IList<EmployeeCardItem> Places {
			get { return places; }
			set { SetField (ref places, value, () => Places); }
		}

		#endregion

		public Facility ()
		{
		}
	}
}

