using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Workwear.Domain.Stock
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "склады",
		Nominative = "склад",
		Genitive = "склада"
		)]
	[HistoryTrace]
	public class Warehouse: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		string name;

		[Display(Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value); }

		}

	}
}
