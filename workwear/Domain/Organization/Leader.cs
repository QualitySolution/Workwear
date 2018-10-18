﻿using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace workwear.Domain.Organization
{
	[OrmSubject (Gender = GrammaticalGender.Feminine,
		NominativePlural = "руководители",
		Nominative = "руководитель")]
	public class Leader : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Display (Name = "Ф.И.О.")]
		[Required (ErrorMessage = "Ф.И.О. должно быть заполнено.")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		#endregion


		public Leader ()
		{
		}
	}
}

