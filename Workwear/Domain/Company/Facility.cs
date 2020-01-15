using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace workwear.Domain.Company
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "подразделения",
		Nominative = "подразделение")]
	
	public class Facility : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private string code;

		[Display(Name = "Код подразделения")]
		[StringLength(20)]
		public virtual string Code {
			get { return code; }
			set { SetField(ref code, value); }
		}

		string name;

		[Display (Name = "Название")]
		[StringLength(240)]
		[Required (ErrorMessage = "Название должно быть заполнено.")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		string address;

		[Display (Name = "Адрес")]
		[StringLength(65536)]
		public virtual string Address {
			get { return address; }
			set { SetField (ref address, value, () => Address); }
		}

		private IList<FacilityPlace> places = new List<FacilityPlace>();

		[Display (Name = "Места размещения")]
		public virtual IList<FacilityPlace> Places {
			get { return places; }
			set { SetField (ref places, value, () => Places); }
		}

		#endregion

		public Facility ()
		{
		}
	}
}

