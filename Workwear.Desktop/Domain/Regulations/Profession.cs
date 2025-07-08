﻿using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Workwear.Domain.Regulations
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "профессии",
		Nominative = "профессия",
		Genitive ="профессии",
		GenitivePlural = "профессий"
		)]
	[HistoryTrace]
	public class Profession : PropertyChangedBase, IDomainObject
	{
		public Profession()
		{
		}

		#region Свойства
		public virtual int Id { get; set; }

		string name;

		[Display(Name = "Название")]
		[Required(ErrorMessage = "Название должно быть заполнено.")]
		[StringLength(200)]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value, () => Name); }
		}

		private uint? code;
		[Display(Name = "Код")]
		public virtual uint? Code {
			get => code;
			set => SetField(ref code, value);
		}

		#endregion
	}
}
