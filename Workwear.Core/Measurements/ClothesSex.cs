using System.ComponentModel.DataAnnotations;

namespace Workwear.Measurements
{
	public enum ClothesSex {
		[Display(Name = "Универсальный")]
		Universal,
		[Display(Name = "Женский")]
		Women,
		[Display(Name = "Мужской")]
		Men,
	}
}

