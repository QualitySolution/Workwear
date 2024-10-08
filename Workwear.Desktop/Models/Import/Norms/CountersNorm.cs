﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Workwear.Models.Import.Norms
{
	public enum CountersNorm
	{
		[Display(Name = "Новых норм")]
		NewNorms,
		[Display(Name = "Новых строк нормы")]
		NewNormItems,
		[Display(Name = "Измененных строк нормы")]
		ChangedNormItems,
		[Display(Name = "Строк нормы без изменений")]
		NotChangedNormItems,
		[Display(Name = "Новых подразделений")]
		NewSubdivisions,
		[Display(Name = "Новых отделов")]
		NewDepartments,
		[Display(Name = "Новых должностей")]
		NewPosts,
		[Display(Name = "Новых номенклатур нормы")]
		NewProtectionTools,
		[Display(Name = "Новых типов номенклатуры")]
		NewItemTypes,
		[Display(Name = "Новых условий нормы")]
		NewCondition,
		[Display(Name = "Не распознанных номенклатур")]
		UndefinedItemTypes,
		[Display(Name = "Неоднозначных норм")]
		AmbiguousNorms,
		[Display(Name = "Пропущено строк")]
		SkipRows
	}
}
