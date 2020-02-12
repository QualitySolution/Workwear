using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using workwear.Domain.Company;

namespace workwear.Domain.Stock
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "склады",
		Nominative = "склад")]
	public class Warehouse: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		string name;

		[Display(Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value); }

		}

		Subdivision subdivision;
		[Display(Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get { return subdivision; }
			set { SetField(ref subdivision, value); }
		}

	}
}
