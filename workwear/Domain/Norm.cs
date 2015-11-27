using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QSOrmProject;

namespace workwear.Domain
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "нормы выдачи",
		Nominative = "норма выдачи")]
	public class Norm : PropertyChangedBase, IDomainObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

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

		private IList<Post> professions = new List<Post>();

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

		private IList<NormItem> items = new List<NormItem>();

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

		public virtual string ProfessionsText {
			get{ return String.Join ("; ", Professions.Select (p => p.Name));
			}
		}

		public Norm ()
		{
		}

		public virtual void AddProfession(Post prof)
		{
			if(Professions.Any (p => DomainHelper.EqualDomainObjects (p, prof)))
			{
				logger.Warn ("Такая профессия уже добавлена. Пропускаем...");
				return;
			}
			ObservableProfessions.Add (prof);
		}

		public virtual void RemoveProfession(Post prof)
		{
			ObservableProfessions.Remove (prof);
		}

		public virtual void AddItem(ItemsType itemtype)
		{
			if(Items.Any (i => DomainHelper.EqualDomainObjects (i.Item, itemtype)))
			{
				logger.Warn ("Такое наименование уже добавлено. Пропускаем...");
				return;
			}

			var item = new NormItem () {
				Item = itemtype,
				Amount = 1,
				NormPeriod = NormPeriodType.Year,
				PeriodCount = 1
			};

			ObservableItems.Add (item);
		}

		public virtual void RemoveItem(NormItem item)
		{
			ObservableItems.Remove (item);
		}

	}
}

