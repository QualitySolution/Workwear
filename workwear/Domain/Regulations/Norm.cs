using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QSOrmProject;
using Gamma.Utilities;
using workwear.Domain.Regulations;

namespace workwear.Domain.Regulations
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "нормы выдачи",
		Nominative = "норма выдачи",
		PrepositionalPlural = "нормах выдачи"
	)]
	public class Norm : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region Свойства

		public virtual int Id { get; set; }

		private RegulationDoc document;

		[Display(Name = "Нормативный документ")]
		public virtual RegulationDoc Document
		{
			get { return document; }
			set { SetField(ref document, value, () => Document); }
		}

		private RegulationDocAnnex annex;

		[Display(Name = "Приложение нормативного документа")]
		public virtual RegulationDocAnnex Annex
		{
			get { return annex; }
			set { SetField(ref annex, value, () => Annex); }
		}

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

		public virtual string Title{
			get{
				string str = string.Empty;
				if(!String.IsNullOrWhiteSpace (TONNumber) || !String.IsNullOrWhiteSpace (TONAttachment))
					str += "ТОН ";
				if (!String.IsNullOrWhiteSpace (TONNumber))
					str += TONNumber;
				if(!String.IsNullOrWhiteSpace (TONAttachment))
					str += "-" + TONAttachment;

				if (!String.IsNullOrWhiteSpace (TONParagraph))
					str += String.Format (" п. {0}", TONParagraph);
				return str.Trim ();
			}
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

		private string comment;

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get { return comment; }
			set { SetField(ref comment, value, () => Comment); }
		}

		#endregion

#region Генерируемые

		public virtual string ProfessionsText {
			get{ return String.Join ("; ", Professions.Select (p => p.Name));
			}
		}

		public virtual string DocumentNumberText => document?.Number;

		public virtual string AnnexNumberText => Annex?.Number.ToString();

# endregion

		public Norm ()
		{
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (Professions.Count == 0)
				yield return new ValidationResult ("Норма должна содержать хотя бы одну профессию.", 
					new[] { this.GetPropertyName (o => o.Professions) });

			if (Items.Count == 0)
				yield return new ValidationResult ("Норма должна содержать хотя бы одну номенклатуру.", 
					new[] { this.GetPropertyName (o => o.Items) });
		}

		#endregion


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
				Norm = this,
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

