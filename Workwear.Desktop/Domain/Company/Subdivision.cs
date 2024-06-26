﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Workwear.Domain.Stock;

namespace Workwear.Domain.Company
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
		
		private IObservableList<Subdivision> childSubdivisions = new ObservableList<Subdivision>();
		[Display (Name = "Дочерние подразделения")]
		public virtual IObservableList<Subdivision> ChildSubdivisions {
			get => childSubdivisions;
			set => SetField (ref childSubdivisions, value);
		}
		#endregion
		#region Методы
		public virtual IEnumerable<Subdivision> AllGenerationsSubdivisions {
			get {
				yield return this;
				foreach (var child in ChildSubdivisions.SelectMany(x => x.AllGenerationsSubdivisions))
					yield return child;
			}
		}
		public virtual IEnumerable<Subdivision> AllParents {
			get {
				yield return ParentSubdivision;
				if(ParentSubdivision != null)
					foreach (var parent in ParentSubdivision.AllParents)
						yield return parent;
			}
		}
		#endregion
		#region IValidatableObject implementation
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
			if(ParentSubdivision != null)
				if (ParentSubdivision.AllParents.Take(500).Contains(this))
					yield return new ValidationResult($"Родительское подразделение: " +
				                                  $"{ParentSubdivision.Name} уже значится дочерней к текущему!");
		}
		#endregion
		public Subdivision () { }
	}
}

