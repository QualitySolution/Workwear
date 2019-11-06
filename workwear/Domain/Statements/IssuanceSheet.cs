using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using workwear.Domain.Company;

namespace workwear.Domain.Statements
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "ведомости на выдачу",
		Nominative = "ведомость на выдачу")]
	public class IssuanceSheet : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private DateTime date;

		[Display(Name = "Дата составления")]
		public virtual DateTime Date {
			get { return date; }
			set { SetField(ref date, value); }
		}

		private Organization organization;

		[Display(Name = "Организация")]
		public virtual Organization Organization {
			get { return organization; }
			set { SetField(ref organization, value); }
		}

		private Facility subdivision;

		[Display(Name = "Подразделение")]
		public virtual Facility Subdivision {
			get { return subdivision; }
			set { SetField(ref subdivision, value); }
		}

		#region Подписи

		private Leader responsiblePerson;

		[Display(Name = "Материально ответственное лицо")]
		public virtual Leader ResponsiblePerson {
			get { return responsiblePerson; }
			set { SetField(ref responsiblePerson, value); }
		}

		private Leader headOfDivisionPerson;

		[Display(Name = "Руководитель подразделения")]
		public virtual Leader HeadOfDivisionPerson {
			get { return headOfDivisionPerson; }
			set { SetField(ref headOfDivisionPerson, value); }
		}

		#endregion

		IList<IssuanceSheetItem> items = new List<IssuanceSheetItem>();
		[Display(Name = "Строки")]
		public virtual IList<IssuanceSheetItem> Items {
		    get => items;
			set => SetField(ref items, value);
		}

		GenericObservableList<IssuanceSheetItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<IssuanceSheetItem> ObservableItems {
		    get {
		        if(observableItems == null)
		            observableItems = new GenericObservableList<IssuanceSheetItem>(Items);
		        return observableItems;
			}
		}

		#endregion

		public IssuanceSheet()
		{
		}
	}
}
