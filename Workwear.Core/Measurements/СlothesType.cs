using System.ComponentModel.DataAnnotations;

namespace Workwear.Measurements
{
	public enum ClothesSex {
		[Display(Name = "Женский")]
		Women,
		[Display(Name = "Мужской")]
		Men,
		[Display(Name = "Универсальный")]
		Universal,
	}
}

