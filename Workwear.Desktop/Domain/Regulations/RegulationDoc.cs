using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;

namespace Workwear.Domain.Regulations
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "нормативные документы",
		Nominative = "нормативный документ",
		Genitive = "нормативного документа"
	)]
	[HistoryTrace]
	public class RegulationDoc : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private string name;

		[Display(Name = "Название документа")]
		[StringLength(255)]
		public virtual string Name
		{
			get { return name; }
			set { SetField(ref name, value, () => Name); }
		}

		private string number;

		[Display(Name = "Номер")]
		public virtual string Number
		{
			get { return number; }
			set { SetField(ref number, value, () => Number); }
		}

		private DateTime? docDate;

		[Display(Name = "Дата документа")]
		public virtual DateTime? DocDate
		{
			get { return docDate; }
			set { SetField(ref docDate, value, () => DocDate); }
		}

		private IObservableList<RegulationDocAnnex> annexes = new ObservableList<RegulationDocAnnex>();

		[Display(Name = "Приложения")]
		public virtual IObservableList<RegulationDocAnnex> Annexes
		{
			get { return annexes; }
			set { SetField(ref annexes, value, () => Annexes); }
		}
		#endregion

		#region Расчетные

		public virtual string Title
		{
			get
			{
				return Name + (DocDate.HasValue ? String.Format(" от {0:d}", DocDate.Value) : "") + (String.IsNullOrWhiteSpace(Number) ? "" : String.Format(" №{0}", Number));
			}
		}
		#endregion

		public RegulationDoc()
		{
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (String.IsNullOrWhiteSpace(Name))
				yield return new ValidationResult("Название документа должно быть заполнено.",
												  new[] { this.GetPropertyName(o => o.Name) });
			if(Annexes.Any(x => x.Number < 0))
				yield return new ValidationResult("Номер приложения должен быть положительным числом.",
												  new[] { nameof(Annexes) });
			if(Annexes.Any(x => x.Number > 127))
				yield return new ValidationResult("Номер приложения не может превышать 127.",
												  new[] { nameof(Annexes) });

			if(Annexes.Any(x => !String.IsNullOrEmpty(x.Name) && x.Name.Length > 255))
				yield return new ValidationResult("Название приложения не может превышать 255 символов.",
												  new[] { nameof(Annexes) });
		}

		#region Методы

		public virtual RegulationDocAnnex AddAnnex()
		{
			var annex = new RegulationDocAnnex();
			annex.Document = this;
			if (Annexes.Count == 0)
				annex.Number = 1;
			else
				annex.Number = Annexes.Max(x => x.Number) + 1;

			Annexes.Add(annex);
			return annex;
		}

		public virtual void RemoveAnnex(RegulationDocAnnex annex){
			Annexes.Remove(annex);
		}

  		#endregion
	}
}
