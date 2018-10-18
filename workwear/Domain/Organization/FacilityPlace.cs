﻿using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QSOrmProject;

namespace workwear.Domain.Organization
{
	[OrmSubject (Gender = GrammaticalGender.Neuter,
		NominativePlural = "размещения в объекте",
		Nominative = "размещение в объекте")]
	
	public class FacilityPlace : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Display (Name = "Название")]
		[StringLength(45)]
		[Required (ErrorMessage = "Название должно быть заполнено.")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		Facility facility;

		[Display (Name = "Объект")]
		public virtual Facility Facility {
			get { return facility; }
			set { SetField (ref facility, value, () => Facility); }
		}


		#endregion

		public FacilityPlace ()
		{
		}
	}
}

