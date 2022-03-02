using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace workwear.Domain.Company
{

	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "организации",
		Nominative = "организация",
		Genitive = "организации"
		)]
	[HistoryTrace]
	public class Organization : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private string name;

		[Display(Name = "Название")]
		[Required(ErrorMessage = "Название должно быть заполнено")]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value); }
		}

		private string address;

		[Display(Name = "Адрес")]
		public virtual string Address {
			get { return address; }
			set { SetField(ref address, value); }
		}

		#endregion
	}
}
