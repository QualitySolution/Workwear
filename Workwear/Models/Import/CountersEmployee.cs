using System;
using System.ComponentModel.DataAnnotations;

namespace workwear.Models.Import
{
	public enum CountersEmployee
	{
		[Display(Name = "Новых сотрудников")]
		NewEmployee,
		[Display(Name = "Измененных сотрудников")]
		ChangedEmployee,
		[Display(Name = "Сотрудников без изменений")]
		NotChangedEmployee,
		[Display(Name = "Новых подразделений")]
		NewSubdivisions,
		[Display(Name = "Новых отделов")]
		NewDepartments,
		[Display(Name = "Новых должностей")]
		NewPosts,
		[Display(Name = "Пропущено строк")]
		SkipRows,
		[Display(Name = "Несколько соответствий")]
		MultiMatch,
	}
}
