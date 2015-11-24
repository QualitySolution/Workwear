using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace workwear.Domain
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "типы номерклатуры",
		Nominative = "тип номенклатуры")]
	public class ItemsType : PropertyChangedBase, IDomainObject
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
		public virtual MeasurementUnits Units {
			get { return units; }
			set { SetField (ref units, value, () => Units); }
		}

		ItemTypeCategory category;

		[Display (Name = "Категория")]
		public virtual ItemTypeCategory Category {
			get { return category; }
			set { SetField (ref category, value, () => Category); }
		}

		#endregion


		public ItemsType ()
		{
		}
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

