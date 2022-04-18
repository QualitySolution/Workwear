using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
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
	public class Subdivision : PropertyChangedBase, IDomainObject, IValidatableObject
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
		
		private IList<Subdivision> childSubdivisions = new List<Subdivision>();
		[Display (Name = "Дочерние подразделения")]
		public virtual IList<Subdivision> ChildSubdivisions {
			get => childSubdivisions;
			set => SetField (ref childSubdivisions, value);
		}

		GenericObservableList<Subdivision> observableChildSubdivisions;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Subdivision> ObservableChildSubdivisions =>
			observableChildSubdivisions ?? (observableChildSubdivisions =
				new GenericObservableList<Subdivision>(ChildSubdivisions));

		#endregion
		#region Методы
		public virtual IEnumerable<Subdivision> AllGenerationsSubdivisions {
			get {
				yield return this;
				foreach (var child in ChildSubdivisions.SelectMany(x => x.AllGenerationsSubdivisions))
					yield return child;
			}
		}
		#endregion
		#region IValidatableObject implementation
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
			if (AllGenerationsSubdivisions.Take(500).Contains(ParentSubdivision))
				yield return new ValidationResult($"Родительское подразделение: " +
				                                  $"{ParentSubdivision.Name} уже значится в дочерних!");
		}
		#endregion
		public Subdivision () { }
	}
}

