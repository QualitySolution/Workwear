using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using workwear.Domain.Company;

namespace workwear.Domain.Regulations
{
	[Appellative (Gender = GrammaticalGender.Feminine,
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

		string tonParagraph;

		[Display(Name = "№ пункта приложения ТОН")]
		[StringLength(15)]
		public virtual string TONParagraph {
			get { return tonParagraph; }
			set { SetField(ref tonParagraph, value, () => TONParagraph); }
		}

		private string name;
		[Display(Name = "Название")]
		[StringLength(200)]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value);
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

		DateTime? dateFrom;
		public virtual DateTime? DateFrom 
		{
			get { return dateFrom; }
			set { SetField(ref dateFrom, value); }
		}

		DateTime? dateTo;
		public virtual DateTime? DateTo {
			get { return dateTo; }
			set { SetField(ref dateTo, value); }
		}

		#endregion

		#region Генерируемые

		public virtual string ProfessionsText {
			get{ return String.Join ("; ", Professions.Select (p => p.Name));
			}
		}

		public virtual string DocumentNumberText => document?.Number;

		public virtual string AnnexNumberText => Annex?.Number.ToString();

		public virtual bool IsActive => (DateFrom == null || DateFrom.Value <= DateTime.Today)
			&& (DateTo == null || DateTo >= DateTime.Today);

		# endregion

		public Norm ()
		{
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
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

		public virtual NormItem AddItem(ProtectionTools tools)
		{
			if(Items.Any (i => DomainHelper.EqualDomainObjects (i.ProtectionTools, tools)))
			{
				logger.Warn ("Такое наименование уже добавлено. Пропускаем...");
				return null;
			}

			var item = new NormItem () {
				Norm = this,
				ProtectionTools = tools,
				Amount = 1,
				NormPeriod = NormPeriodType.Year,
				PeriodCount = 1
			};

			ObservableItems.Add (item);
			return item;
		}

		public virtual void RemoveItem(NormItem item)
		{
			ObservableItems.Remove (item);
		}

	}
}

