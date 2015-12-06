using System;
using System.ComponentModel.DataAnnotations;
using workwear.Domain;

namespace workwear.Measurements
{
	public enum СlothesType
	{
		[Display(Name = "Женская одежда",
			Description = "Блузки, туники, куртки, платья")]
		[OnlyWoman]
		[NeedGrowth(Sex.F)]
		[SizeStandarts(typeof(SizeStandartWomenWear))]
		WomanWear,
/*		[Display(Name = "Брюки, юбки, шорты")]
		[OnlyWoman]
		WomanPants,
		[Display(Name = "Женские джинсы")]
		[OnlyWoman]
		WomanJeans,
*/		[Display(Name = "Женская обувь")]
		[OnlyWoman]
		[SizeStandarts(typeof(SizeStandartWomenShoes))]
		WomanShoes,
/*		[Display(Name = "Женские колготки и чулки")]
		[OnlyWoman]
		WomanTights,
		[Display(Name = "Женские носки")]
		[OnlyWoman]
		WomanSocks,
		[Display(Name = "Бюстгальтеры")]
		[OnlyWoman]
		WomanBras,
		[Display(Name = "Женское нижнее белье")]
		[OnlyWoman]
		WomanUnderwear,
*/		[Display(Name = "Мужская одежда",
			Description = "Пиджаки, джемпера, жилеты, халаты, свитера, куртки, рубашки")]
		[OnlyMen]
		[NeedGrowth(Sex.M)]
		[SizeStandarts(typeof(SizeStandartMenWear))]
		MenWear,
/*		[Display(Name = "Сорочки")]
		[OnlyMen]
		MenShirts,
		[Display(Name = "Мужские Брюки, шорты")]
		[OnlyMen]
		MenPants,
		[Display(Name = "Мужские джинсы")]
		[OnlyMen]
		MenJeans,
		[Display(Name = "Мужское нижнее бельё")]
		[OnlyMen]
		MenUnderwear,
		[Display(Name = "Мужские носки")]
		[OnlyMen]
		MenSocks,
*/		[Display(Name = "Мужская обувь")]
		[OnlyMen]
		[SizeStandarts(typeof(SizeStandartMenShoes))]
		MenShoes,
		[Display(Name = "Головные уборы")]
		[SizeStandarts(typeof(SizeStandartHeaddress))]
		Headgear,
/*		[Display(Name = "Ремни")]
		Belts,
*/		[Display(Name = "Перчатки")]
		[SizeStandarts(typeof(SizeStandartGloves))]
		Gloves,
	}

	public class СlothesTypeType : NHibernate.Type.EnumStringType
	{
		public СlothesTypeType () : base (typeof(СlothesType))
		{
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class OnlyWomanAttribute : Attribute 
	{

	}

	[AttributeUsage(AttributeTargets.Field)]
	public class OnlyMenAttribute : Attribute 
	{

	}

	[AttributeUsage(AttributeTargets.Field)]
	public class NeedGrowthAttribute : Attribute 
	{
		public Sex Sex { set; get;}

		public NeedGrowthAttribute (Sex sex)
		{
			Sex = sex;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class SizeStandartsAttribute : Attribute 
	{
		public Type StandartsEnumType { get; set;}

		public SizeStandartsAttribute(Type enumStd)
		{
			StandartsEnumType = enumStd;
		}
	}

}

