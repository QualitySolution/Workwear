using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using workwear.Domain.Stock;

namespace workwear.Domain.Company
{
	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "подразделения",
		Nominative = "подразделение",
		Genitive ="подразделения"
		)]
	[HistoryTrace]
	public class Subdivision : PropertyChangedBase, IDomainObject
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

		private IList<SubdivisionPlace> places = new List<SubdivisionPlace>();

		[Display (Name = "Места размещения")]
		public virtual IList<SubdivisionPlace> Places {
			get { return places; }
			set { SetField (ref places, value, () => Places); }
		}

		Warehouse warehouse;

		public virtual Warehouse Warehouse {
			get { return warehouse; }
			set { SetField(ref warehouse, value); }
		}


		#endregion

		public Subdivision ()
		{
		}
	}
}

