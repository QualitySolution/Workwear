using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using Gamma.Utilities;
using QS.BusinessCommon.Domain;
using QS.DomainModel.Entity;
using Workwear.Measurements;

namespace workwear.Domain.Stock
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
			set { SetField (ref name, value?.Trim()); }
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

		private IssueType issueType;
		[Display(Name = "Тип выдачи")]
		public virtual IssueType IssueType {
			get => issueType;
			set => SetField(ref issueType, value);
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

		private IList<Nomenclature> nomenclatures = new List<Nomenclature>();

		[Display(Name = "Номенклатура")]
		public virtual IList<Nomenclature> Nomenclatures {
			get { return nomenclatures; }
			set { SetField(ref nomenclatures, value, () => Nomenclatures); }
		}

		GenericObservableList<Nomenclature> observableNomenclatures;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Nomenclature> ObservableNomenclatures {
			get {
				if(observableNomenclatures == null)
					observableNomenclatures = new GenericObservableList<Nomenclature>(Nomenclatures);
				return observableNomenclatures;
			}
		}

		#endregion



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

	public enum IssueType
	{
		[Display(Name = "Персональная")]
		Personal,
		[Display(Name = "Коллективная")]
		Collective
	}

	public class IssueTypeEnumType : NHibernate.Type.EnumStringType
	{
		public IssueTypeEnumType() : base(typeof(IssueType))
		{
		}
	}
}

