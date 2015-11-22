using System;
using System.ComponentModel.DataAnnotations;

namespace workwear.Measurements
{
	public enum СlothesType
	{
		[Display(Name = "Блузки, туники, куртки, платья")]
		[OnlyWoman]
		WomanJacket,
		[Display(Name = "Брюки, юбки, шорты")]
		[OnlyWoman]
		WomanPants,
		[Display(Name = "Женские джинсы")]
		[OnlyWoman]
		WomanJeans,
		[Display(Name = "Женская обувь")]
		[OnlyWoman]
		WomanShoes,
		[Display(Name = "Женские колготки и чулки")]
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
		[Display(Name = "Пиджаки, джемпера, жилеты, халаты, свитера, куртки, рубашки")]
		[OnlyMen]
		MenJacket,
		[Display(Name = "Сорочки")]
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
		[Display(Name = "Мужская обувь")]
		[OnlyMen]
		MenShoes,
		[Display(Name = "Головные уборы")]
		Headgear,
		[Display(Name = "Ремни")]
		Belts,
		[Display(Name = "Перчатки")]
		Gloves,
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class OnlyWomanAttribute : Attribute 
	{

	}

	[AttributeUsage(AttributeTargets.Field)]
	public class OnlyMenAttribute : Attribute 
	{

	}
}

