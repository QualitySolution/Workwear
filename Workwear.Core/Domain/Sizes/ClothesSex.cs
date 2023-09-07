using System.ComponentModel.DataAnnotations;

namespace Workwear.Domain.Sizes
{
	public enum ClothesSex {
		[Display(Name = "Универсальный", ShortName = "уни.")]
		Universal,
		[Display(Name = "Женский", ShortName = "жен.")]
		Women,
		[Display(Name = "Мужской", ShortName = "муж.")]
		Men,
	}
}

