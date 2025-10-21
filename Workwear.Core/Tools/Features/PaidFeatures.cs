using System;
using System.ComponentModel.DataAnnotations;

namespace Workwear.Tools.Features {
	[Flags]
	public enum PaidFeatures : uint {
		None = 0,
		[Display(Name = "Модуль маркировки")]
		Barcodes = 1,
		[Display(Name = "Модуль стирки")]
		ClothingService = 2,
		[Display(Name = "Модуль СКУД")]
		IdentityCards = 4
	}
}
