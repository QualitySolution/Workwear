using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QSOrmProject;

namespace workwear.Domain.Regulations
{
	[OrmSubject(Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "нормативные документы",
		Nominative = "нормативный документ"
	)]
	public class RegulationDoc : PropertyChangedBase
	{
		#region Свойства

		public virtual int Id { get; set; }

		private string name;

		[Display(Name = "Название документа")]
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

		private IList<RegulationDocAnnex> annexes = new List<RegulationDocAnnex>();

		[Display(Name = "Приложения")]
		public virtual IList<RegulationDocAnnex> Annexess
		{
			get { return annexes; }
			set { SetField(ref annexes, value, () => Annexess); }
		}

		GenericObservableList<RegulationDocAnnex> observableAnnexes;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
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

		public RegulationDoc()
		{
		}
	}
}
