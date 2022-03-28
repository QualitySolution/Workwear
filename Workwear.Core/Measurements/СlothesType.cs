using System;
using System.ComponentModel.DataAnnotations;

namespace Workwear.Measurements
{
	[Obsolete("Работа с размерами перенесена в классы Size, SizeType и SizeService")]
	public enum СlothesType
	{
		[Display(Name = "Одежда")]
		[NeedGrowth]
		[SizeStandarts(typeof(SizeStandartWomenWear), ClothesSex.Women)]
		[SizeStandarts(typeof(SizeStandartMenWear), ClothesSex.Men)]
		[SizeStandarts(typeof(SizeStandartUnisexWear), ClothesSex.Universal, SizeUse.СlothesOnly)]
		Wear,
		[Display(Name = "Обувь")]
		[SizeStandarts(typeof(SizeStandartWomenShoes), ClothesSex.Women)]
		[SizeStandarts(typeof(SizeStandartMenShoes), ClothesSex.Men)]
		[SizeStandarts(typeof(SizeStandartUnisexShoes), ClothesSex.Universal, SizeUse.СlothesOnly)]
		Shoes,
		[Display(Name = "Зимняя обувь")]
		[SizeStandarts(typeof(SizeStandartWomenShoes), ClothesSex.Women)]
		[SizeStandarts(typeof(SizeStandartMenShoes), ClothesSex.Men)]
		[SizeStandarts(typeof(SizeStandartUnisexShoes), ClothesSex.Universal, SizeUse.СlothesOnly)]
		WinterShoes,

		[Display(Name = "Головные уборы")]
		[SizeStandarts(typeof(SizeStandartHeaddress))]
		Headgear,
		[Display(Name = "Перчатки")]
		[SizeStandarts(typeof(SizeStandartGloves))]
		Gloves,
		[Display(Name = "Рукавицы")]
		[SizeStandarts(typeof(SizeStandartMittens))]
		Mittens,
		[Display(Name = "СИЗ")]
		PPE,
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

