using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QSBusinessCommon.Domain;
using QSOrmProject;
using workwear.Measurements;

namespace workwear.Domain
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "типы номерклатуры",
		Nominative = "тип номенклатуры")]
	public class ItemsType : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Display (Name = "Название")]
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
}

