using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;

namespace workwear.Domain.Stock
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "номенклатуры",
		Nominative = "номенклатура")]
	public class Nomenclature: PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Display (Name = "Название")]
		[Required (ErrorMessage = "Название номенклатуры должно быть заполнено.")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		ItemsType type;

		[Display (Name = "Группа номенклатур")]
		public virtual ItemsType Type {
			get { return type; }
			set { SetField (ref type, value, () => Type); }
		}

		string sizeStd;

		[Display (Name = "Стандарт размера")]
		public virtual string SizeStd {
			get { return sizeStd; }
			set { SetField (ref sizeStd, value, () => SizeStd); }
		}

		string size;

		[Display (Name = "Размер")]
		public virtual string Size { 
			get { return size; } 
			set	{ SetField (ref size, value, () => Size); }
		}

		string wearGrowth;

		[Display (Name = "Рост одежды")]
		public virtual string WearGrowth { 
			get { return wearGrowth; } 
			set	{ SetField (ref wearGrowth, value, () => WearGrowth); }
		}

		string wearGrowthStd;

		[Display (Name = "Стандарт роста")]
		public virtual string WearGrowthStd {
			get { return wearGrowthStd; }
			set { SetField (ref wearGrowthStd, value, () => WearGrowthStd); }
		}

		#endregion

		public Nomenclature ()
		{
			
		}
	}
}

