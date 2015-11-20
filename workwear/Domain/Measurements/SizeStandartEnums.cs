using System;
using System.ComponentModel.DataAnnotations;

namespace workwear.Domain
{
	public enum SizeStandartWear
	{
		[Display(Name = "Российский", ShortName = "РФ")]
		Rus,
		[Display(Name = "Международный", ShortName = "Межд.")]
		Intl,
	}

	public enum SizeStandartShoes
	{
		[Display(Name = "Российский", ShortName = "РФ")]
		Rus,
		[Display(Name = "Международный", ShortName = "Межд.")]
		Intl,
	}

	public enum SizeStandartHeaddress
	{
		[Display(Name = "Российский", ShortName = "РФ")]
		Rus,
		[Display(Name = "Международный", ShortName = "Межд.")]
		Intl,
	}

	public enum SizeStandartGloves
	{
		[Display(Name = "Российский", ShortName = "РФ")]
		Rus,
		[Display(Name = "Международный", ShortName = "Межд.")]
		Intl,
	}

}

