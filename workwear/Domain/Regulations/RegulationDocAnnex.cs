using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;

namespace workwear.Domain.Regulations
{
	[OrmSubject(Gender = QSProjectsLib.GrammaticalGender.Neuter,
	NominativePlural = "приложения к нормативному документу",
	            Nominative = "приложение к нормативному документу"
	)]
	public class RegulationDocAnnex : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private RegulationDoc document;

		[Display(Name = "Нормативный документ")]
		public virtual RegulationDoc Document
		{
			get { return document; }
			set { SetField(ref document, value, () => Document); }
		}

		private string name;

		[Display(Name = "Название приложения")]
		public virtual string Name
		{
			get { return name; }
			set { SetField(ref name, value, () => Name); }
		}

		private int number;

		[Display(Name = "Номер")]
		public virtual int Number
		{
			get { return number; }
			set { SetField(ref number, value, () => Number); }
		}

		#endregion

		public virtual string Title => $"Приложение №{Number}"; 

		public RegulationDocAnnex()
		{
		}
	}
}
