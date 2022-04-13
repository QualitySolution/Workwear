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
			get => code;
			set => SetField(ref code, value);
		}

		private string name;
		[Display (Name = "Название")]
		[StringLength(240)]
		[Required (ErrorMessage = "Название должно быть заполнено.")]
		public virtual string Name {
			get => name;
			set => SetField (ref name, value);
		}

		private string address;
		[Display (Name = "Адрес")]
		[StringLength(65536)]
		public virtual string Address {
			get => address;
			set => SetField (ref address, value);
		}

		private IList<SubdivisionPlace> places = new List<SubdivisionPlace>();
		[Display (Name = "Места размещения")]
		public virtual IList<SubdivisionPlace> Places {
			get => places;
			set => SetField (ref places, value);
		}

		private Warehouse warehouse;
		[Display(Name = "Склад подразделения")]
		public virtual Warehouse Warehouse {
			get => warehouse;
			set => SetField(ref warehouse, value);
		}

		private Subdivision parentSubdivision;
		[Display(Name = "Головное подразделение")]
		public virtual Subdivision ParentSubdivision {
			get => parentSubdivision;
			set => SetField(ref parentSubdivision, value);
		}

		#endregion
		public Subdivision () { }
	}
}

