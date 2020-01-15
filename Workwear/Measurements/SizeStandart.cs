using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QSOrmProject;

namespace workwear.Measurements
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "стандарты размеров",
		Nominative = "стандарт размеров")]
	public class SizeStandart : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Display (Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		string shortName;

		[Display (Name = "Сокращение")]
		public virtual string ShortName {
			get { return shortName; }
			set { SetField (ref shortName, value, () => ShortName); }
		}

		СlothesType clothesType;

		[Display (Name = "Тип одежды")]
		public virtual СlothesType СlothesType {
			get { return clothesType; }
			set { SetField (ref clothesType, value, () => СlothesType); }
		}

		string columnName;

		[Display (Name = "Название колонки")]
		public virtual string ColumnName { 
			get { return columnName; } 
			set	{ SetField (ref columnName, value, () => ColumnName); }
		}

		#endregion

		public SizeStandart ()
		{
		}
	}
}

