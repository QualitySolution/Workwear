﻿using System;
using System.ComponentModel.DataAnnotations;

namespace workwear.Models.Import
{
	public enum ChangeType
	{
		[Display(Name = "Не изменилось")]
		NotChanged,
		[Display(Name = "Новый объект")]
		NewEntity,
		[Display(Name = "Изменилось значение")]
		ChangeValue,
		[Display(Name = "Ошибка разбора")]
		ParseError
	}
}
