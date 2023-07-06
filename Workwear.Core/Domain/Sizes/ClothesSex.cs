using System.ComponentModel.DataAnnotations;

namespace Workwear.Domain.Sizes
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

