using System;
using System.ComponentModel.DataAnnotations;

namespace workwear.Measurements
{
	public enum СlothesType
	{
		[Display(Name = "Одежда")]
		[NeedGrowth]
		[SizeStandarts(typeof(SizeStandartWomenWear), ClothesSex.Women)]
		[SizeStandarts(typeof(SizeStandartMenWear), ClothesSex.Men)]
		[SizeStandarts(typeof(SizeStandartUnisexWear), ClothesSex.Universal, SizeUse.СlothesOnly)]
		Wear,
/*		[Display(Name = "Брюки, юбки, шорты")]
		[OnlyWoman]
		WomanPants,
		[Display(Name = "Женские джинсы")]
		[OnlyWoman]
		WomanJeans,
*/		[Display(Name = "Обувь")]
		[SizeStandarts(typeof(SizeStandartWomenShoes), ClothesSex.Women)]
		[SizeStandarts(typeof(SizeStandartMenShoes), ClothesSex.Men)]
		[SizeStandarts(typeof(SizeStandartUnisexShoes), ClothesSex.Universal, SizeUse.СlothesOnly)]
		Shoes,
		[Display(Name = "Зимняя обувь")]
		[SizeStandarts(typeof(SizeStandartWomenShoes), ClothesSex.Women)]
		[SizeStandarts(typeof(SizeStandartMenShoes), ClothesSex.Men)]
		[SizeStandarts(typeof(SizeStandartUnisexShoes), ClothesSex.Universal, SizeUse.СlothesOnly)]
		WinterShoes,
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
*/		
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
*/
		[Display(Name = "Головные уборы")]
		[SizeStandarts(typeof(SizeStandartHeaddress))]
		Headgear,
/*		[Display(Name = "Ремни")]
		Belts,
*/		[Display(Name = "Перчатки")]
		[SizeStandarts(typeof(SizeStandartGloves))]
		Gloves,
		[Display(Name = "Рукавицы")]
		[SizeStandarts(typeof(SizeStandartMittens))]
		Mittens,
		[Display(Name = "СИЗ")]
		PPE,
	}

	public class СlothesTypeType : NHibernate.Type.EnumStringType
	{
		public СlothesTypeType () : base (typeof(СlothesType))
		{
		}
	}

	public enum ClothesSex
	{
		[Display(Name = "Женская")]
		Women,
		[Display(Name = "Мужская")]
		Men,
		[Display(Name = "Универсальная")]
		Universal,
	}

	public enum SizeUse{
		Both,
		HumanOnly,
		СlothesOnly,
	}

	public enum SizeUsePlace{
		Human,
		Сlothes,
	}

	public class ClothesSexType : NHibernate.Type.EnumStringType
	{
		public ClothesSexType () : base (typeof(ClothesSex))
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
		public NeedGrowthAttribute ()
		{
			
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
	public class SizeStandartsAttribute : Attribute 
	{
		public Type StandartsEnumType { get; private set;}
		public ClothesSex Sex { get; private set;}
		public SizeUse Use { get; private set;}

		public SizeStandartsAttribute(Type enumStd)
		{
			StandartsEnumType = enumStd;
			Sex = ClothesSex.Universal;
		}

		public SizeStandartsAttribute(Type enumStd, ClothesSex sex, SizeUse use = SizeUse.Both)
		{
			StandartsEnumType = enumStd;
			Sex = sex;
			Use = use;
		}
	}

}

