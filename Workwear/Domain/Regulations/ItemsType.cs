using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using NLog;
using QS.BusinessCommon.Domain;
using QS.DomainModel.Entity;
using workwear.Domain.Stock;
using workwear.Measurements;



namespace workwear.Domain.Regulations
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "типы номенклатуры",
		Nominative = "тип номенклатуры")]
	public class ItemsType : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public virtual int Id { get; set; }

		string name;

		[Display (Name = "Название")]
		[Required(ErrorMessage = "Имя типа номенклатуры не должно быть пустым.")]
		[StringLength(240)]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		MeasurementUnits units;

		[Display (Name = "Единица измерения")]
		[Required(ErrorMessage = "Единица измерения должна быть указана.")]
		public virtual MeasurementUnits Units {
			get { return units; }
			set { SetField (ref units, value, () => Units); }
		}

		ItemTypeCategory category;

		[Display (Name = "Категория")]
		public virtual ItemTypeCategory Category {
			get { return category; }
			set { if(SetField (ref category, value, () => Category))
				{
					if (Category != ItemTypeCategory.wear)
						WearCategory = null;
					if (Category != ItemTypeCategory.property)
						LifeMonths = null;
				}
			}
		}

		СlothesType? wearCategory;

		[Display (Name = "Вид одежды")]
		public virtual СlothesType? WearCategory {
			get { return wearCategory; }
			set { SetField (ref wearCategory, value, () => WearCategory); }
		}

		int? lifeMonths;

		[Display (Name = "Срок службы")]
		public virtual int? LifeMonths {
			get { return lifeMonths; }
			set { SetField (ref lifeMonths, value, () => LifeMonths); }
		}

		private string comment;

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get { return comment; }
			set { SetField(ref comment, value, () => Comment); }
		}

		private IList<Nomenclature> norms = new List<Nomenclature>();

		[Display(Name = "Строки норм")]
		public virtual IList<Nomenclature> Norms {
			get { return norms; }
			set { SetField(ref norms, value, () => Norms); }
		}

		GenericObservableList<Nomenclature> observableNorms;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Nomenclature> ObservableItems {
			get {
				if(observableNorms == null)
					observableNorms = new GenericObservableList<Nomenclature>(Norms);
				return observableNorms;
			}
		}

		#endregion

		private IList<ItemsType> itemsTypesAnalogs = new List<ItemsType>();

		[Display(Name = "Аналоги")]
		public virtual IList<ItemsType> ItemsTypesAnalogs {
			get { return itemsTypesAnalogs; }
			set { SetField(ref itemsTypesAnalogs, value, () => ItemsTypesAnalogs); }
		}

		GenericObservableList<ItemsType> observableItemsTypesAnalogs;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ItemsType> ObservableItemsTypeAnalog{
			get {
				if(observableItemsTypesAnalogs == null)
					observableItemsTypesAnalogs = new GenericObservableList<ItemsType>(ItemsTypesAnalogs);
				return observableItemsTypesAnalogs;
			}
		}

		public virtual void AddAnalog(ItemsType Analog)
		{
			if(ItemsTypesAnalogs.Any(p => DomainHelper.EqualDomainObjects(p, Analog))) {
				logger.Warn("Такой аналог уже добавлен. Пропускаем...");
				return;
			}
			ObservableItemsTypeAnalog.Add(Analog);
		}

		public virtual void RemoveAnalog(ItemsType Analog)
		{
			ObservableItemsTypeAnalog.Remove(Analog);
		}

		public ItemsType ()
		{
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (Category == ItemTypeCategory.wear && WearCategory == null)
				yield return new ValidationResult ("Вид одежды должен быть указан.", 
					new[] { this.GetPropertyName (o => o.WearCategory)});
		}

		#endregion

	}

	public enum ItemTypeCategory{
		[Display(Name = "Спецодежда")]
		wear,
		[Display(Name = "Имущество")]
		property
	}

	public class ItemTypeCategoryType : NHibernate.Type.EnumStringType
	{
		public ItemTypeCategoryType () : base (typeof(ItemTypeCategory))
		{
		}
	}
}

