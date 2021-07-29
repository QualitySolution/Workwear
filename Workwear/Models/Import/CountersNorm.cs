using System;
using System.ComponentModel.DataAnnotations;

namespace workwear.Models.Import
{
	public enum CountersNorm
	{
		[Display(Name = "Новых норм")]
		NewNorms,
		[Display(Name = "Новых строк нормы")]
		NewNormItems,
		[Display(Name = "Измененых строк нормы")]
		ChangedNormItems,
		[Display(Name = "Строк нормы без изменений")]
		NotChangedNormItems,
		[Display(Name = "Новых подразделений")]
		NewSubdivisions,
		[Display(Name = "Новых должностей")]
		NewPosts,
		[Display(Name = "Новых номеклатур нормы")]
		NewProtectionTools,
		[Display(Name = "Неоднозначных норм")]
		AmbiguousNorms,
		[Display(Name = "Пропущено строк")]
		SkipRows
	}
}
