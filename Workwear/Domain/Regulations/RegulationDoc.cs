using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using QS.Utilities.Text;

namespace workwear.Domain.Regulations
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
		
		
		[Display(Name = "Обрезанное название документа")]
		[StringLength(101)]
		public virtual string CroppedName {
			get { return StringManipulationHelper.EllipsizeMiddle(name, 101); }
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

		private IList<RegulationDocAnnex> annexes = new List<RegulationDocAnnex>();

		[Display(Name = "Приложения")]
		public virtual IList<RegulationDocAnnex> Annexess
		{
			get { return annexes; }
			set { SetField(ref annexes, value, () => Annexess); }
		}

		GenericObservableList<RegulationDocAnnex> observableAnnexes;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<RegulationDocAnnex> ObservableAnnexes
		{
			get
			{
				if (observableAnnexes == null)
					observableAnnexes = new GenericObservableList<RegulationDocAnnex>(Annexess);
				return observableAnnexes;
			}
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
			if(Annexess.Any(x => x.Number < 0))
				yield return new ValidationResult("Номер приложения должен быть положительным числом.",
												  new[] { nameof(Annexess) });
			if(Annexess.Any(x => x.Number > 127))
				yield return new ValidationResult("Номер приложения не может превышать 127.",
												  new[] { nameof(Annexess) });

			if(Annexess.Any(x => !String.IsNullOrEmpty(x.Name) && x.Name.Length > 255))
				yield return new ValidationResult("Название приложения не может превышать 255 символов.",
												  new[] { nameof(Annexess) });
		}

		#region Методы

		public virtual RegulationDocAnnex AddAnnex()
		{
			var annex = new RegulationDocAnnex();
			annex.Document = this;
			if (Annexess.Count == 0)
				annex.Number = 1;
			else
				annex.Number = Annexess.Max(x => x.Number) + 1;

			ObservableAnnexes.Add(annex);
			return annex;
		}

		public virtual void RemoveAnnex(RegulationDocAnnex annex){
			ObservableAnnexes.Remove(annex);
		}

  		#endregion
	}
}
