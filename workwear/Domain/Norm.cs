using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QSOrmProject;

namespace workwear.Domain
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "нормы выдачи",
		Nominative = "норма выдачи")]
	public class Norm : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string tonNumber;

		[Display (Name = "№ ТОН")]
		[StringLength (10)]
		public virtual string TONNumber {
			get { return tonNumber; }
			set { SetField (ref tonNumber, value, () => TONNumber); }
		}

		string tonAttachment;

		[Display (Name = "№ приложения ТОН")]
		[StringLength (10)]
		public virtual string TONAttachment {
			get { return tonAttachment; }
			set { SetField (ref tonAttachment, value, () => TONAttachment); }
		}

		string tonParagraph;

		[Display (Name = "№ пункта приложения ТОН")]
		[StringLength (10)]
		public virtual string TONParagraph {
			get { return tonParagraph; }
			set { SetField (ref tonParagraph, value, () => TONParagraph); }
		}

		private IList<Post> professions;

		[Display (Name = "Профессии")]
		public virtual IList<Post> Professions {
			get { return professions; }
			set { SetField (ref professions, value, () => Professions); }
		}

		GenericObservableList<Post> observableProfessions;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Post> ObservableProfessions {
			get {
				if (observableProfessions == null)
					observableProfessions = new GenericObservableList<Post> (Professions);
				return observableProfessions;
			}
		}

		private IList<NormItem> items;

		[Display (Name = "Строки норм")]
		public virtual IList<NormItem> Items {
			get { return items; }
			set { SetField (ref items, value, () => Items); }
		}

		GenericObservableList<NormItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<NormItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<NormItem> (Items);
				return observableItems;
			}
		}

		#endregion

		public Norm ()
		{
		}
	}
}

